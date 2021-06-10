﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Authentication;
using Connections;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using UserManagement;
using Utils;

namespace Chat
{
    public class ServerChat : Chat, IPersistent
    {
        /// <inheritdoc/>
        public override event EventHandler<LogEventArgs> OnLogEntry;
        /// <inheritdoc/>
        public override void RaiseLogEntry(LogEventArgs e) => OnLogEntry?.Invoke(this, e);

        /// <summary>
        /// <see cref="NetServer"/> instance to bind the packet handler to.
        /// </summary>
        private NetServer netServer;

        /// <summary>
        /// <see cref="ServerAuthenticator"/> instance used to check session validity.
        /// </summary>
        private ServerAuthenticator authenticator;

        /// <summary>
        /// <see cref="ServerUserManager"/> instance used to check login state and source UUID validity.
        /// </summary>
        private ServerUserManager userManager;

        /// <summary>
        /// List of chat messages sent to users on connect.
        /// </summary>
        private List<ChatMessage> messageHistory;
        /// <summary>
        /// Lock file to prevent race conditions when accessing <see cref="messageHistory"/>.
        /// </summary>
        private object messageHistoryLock = new object();
        /// <summary>
        /// Maximum number of chat messages to buffer in history.
        /// </summary>
        private int messageHistoryCapacity;

        /// <summary>
        /// Initialises a new <see cref="ServerChat"/> instance.
        /// </summary>
        /// <param name="netServer"></param>
        /// <param name="authenticator"></param>
        /// <param name="userManager"></param>
        /// <param name="messageHistoryCapacity"></param>
        public ServerChat(NetServer netServer, ServerAuthenticator authenticator, ServerUserManager userManager, int messageHistoryCapacity)
        {
            this.netServer = netServer;
            this.authenticator = authenticator;
            this.userManager = userManager;
            this.messageHistoryCapacity = messageHistoryCapacity;

            this.messageHistory = new List<ChatMessage>();

            netServer.RegisterPacketHandler(MODULE_NAME, packetHandler);
            userManager.OnLogin += loginHandler;
        }

        /// <summary>
        /// Initialises a new <see cref="ServerChat"/> instance and loads the chat history from the given file.
        /// </summary>
        /// <param name="netServer"></param>
        /// <param name="authenticator"></param>
        /// <param name="userManager"></param>
        /// <param name="messageHistoryCapacity"></param>
        /// <param name="messageHistoryStorePath"></param>
        public ServerChat(NetServer netServer, ServerAuthenticator authenticator, ServerUserManager userManager, int messageHistoryCapacity, string messageHistoryStorePath) : this(netServer, authenticator, userManager, messageHistoryCapacity)
        {
            Load(messageHistoryStorePath);
        }

        /// <inheritdoc />
        /// <summary>
        /// Saves the chat history to the given file, overwriting if it exists.
        /// </summary>
        /// <param name="path">Chat history store path</param>
        public void Save(string path)
        {
            lock (messageHistoryLock)
            {
                // Create the store from the message list
                ChatHistoryStore store = new ChatHistoryStore
                {
                    ChatMessages = { messageHistory.Select(message => message.ToChatMessageStore()) }
                };

                // Create or truncate the file
                FileStream fs = File.Exists(path)
                    ? File.Open(path, FileMode.Truncate, FileAccess.Write)
                    : File.Create(path);
                using (fs)
                {
                    using (CodedOutputStream cos = new CodedOutputStream(fs))
                    {
                        // Write the store to disk
                        store.WriteTo(cos);
                    }
                }

                RaiseLogEntry(new LogEventArgs(string.Format("Saved {0} chat message{1}", messageHistory.Count, messageHistory.Count != 1 ? "s" : "")));
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Loads the chat history from the given file.
        /// </summary>
        /// <param name="path">Chat history store path</param>
        public void Load(string path)
        {
            lock (messageHistoryLock)
            {
                // Create a new store if one doesn't already exist
                if (!File.Exists(path))
                {
                    RaiseLogEntry(new LogEventArgs("No chat history file, generating a new one"));

                    // Create a new message history list
                    messageHistory = new List<ChatMessage>();

                    // Save a new store to disk
                    Save(path);

                    // Stop here
                    return;
                }

                // Pull the store from disk
                ChatHistoryStore store;
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    using (CodedInputStream cis = new CodedInputStream(fs))
                    {
                        store = ChatHistoryStore.Parser.ParseFrom(cis);
                    }
                }

                // Parse the messages
                messageHistory = store.ChatMessages.Select(ChatMessage.FromChatMessageStore).ToList();

                RaiseLogEntry(new LogEventArgs(string.Format("Loaded {0} chat message{1}", messageHistory.Count, messageHistory.Count != 1 ? "s" : "")));
            }
        }

        private void loginHandler(object sender, ServerLoginEventArgs args)
        {
            lock (messageHistoryLock)
            {
                // Create a chat history packet
                ChatHistoryPacket packet = new ChatHistoryPacket();

                // Convert each chat message to their packet counterparts
                foreach (ChatMessage chatMessage in messageHistory)
                {
                    // Create a chat message packet and add it to the history packet
                    packet.ChatMessages.Add(
                        new ChatMessagePacket
                        {
                            Uuid = chatMessage.SenderUuid,
                            MessageId = chatMessage.MessageId,
                            Message = chatMessage.Message,
                            Timestamp = chatMessage.Timestamp.ToTimestamp()
                        }
                    );
                }

                // Pack the history packet
                Any packedPacket = ProtobufPacketHelper.Pack(packet);

                // Send it on its way
                if (!netServer.TrySend(args.ConnectionId, MODULE_NAME, packedPacket.ToByteArray()))
                {
                    RaiseLogEntry(new LogEventArgs("Failed to send ChatHistoryPacket to connection " + args.ConnectionId.Highlight(HighlightType.ConnectionID), LogLevel.ERROR));
                }
            }
        }

        /// <summary>
        /// Handles incoming packets.
        /// </summary>
        /// <param name="module">Target module</param>
        /// <param name="connectionId">Original connection ID</param>
        /// <param name="data">Data payload</param>
        private void packetHandler(string module, string connectionId, byte[] data)
        {
            // Discard packet if it fails validation
            if (!ProtobufPacketHelper.ValidatePacket(typeof(ServerChat).Namespace, MODULE_NAME, module, data, out Any message, out TypeUrl typeUrl)) return;

            // Determine what to do with the packet
            switch (typeUrl.Type)
            {
                case "ChatMessagePacket":
                    RaiseLogEntry(new LogEventArgs(string.Format("Got a ChatMessagePacket from {0}", connectionId.Highlight(HighlightType.ConnectionID)), LogLevel.DEBUG));
                    chatMessagePacketHandler(connectionId, message.Unpack<ChatMessagePacket>());
                    break;
                default:
                    RaiseLogEntry(new LogEventArgs("Got an unknown packet type (" + typeUrl.Type + "), discarding...", LogLevel.DEBUG));
                    break;
            }
        }

        /// <summary>
        /// Handles incoming <see cref="ChatMessagePacket"/>s.
        /// </summary>
        /// <param name="connectionId">Original connection ID</param>
        /// <param name="packet">Incoming <see cref="ChatMessagePacket"/></param>
        private void chatMessagePacketHandler(string connectionId, ChatMessagePacket packet)
        {
            // Refuse packets from non-authenticated sessions
            if (!authenticator.IsAuthenticated(connectionId, packet.SessionId))
            {
                sendFailedChatMessageResponse(connectionId, packet.MessageId);

                // Stop here
                return;
            }

            // Refuse packets from non-logged in users
            if (!userManager.IsLoggedIn(connectionId, packet.Uuid))
            {
                sendFailedChatMessageResponse(connectionId, packet.MessageId);

                // Stop here
                return;
            }

            // Get a copy of the packet's original message ID
            string originalMessageId = packet.MessageId;

            // Generate a new, guaranteed-to-be-unique message ID since we can't trust clients
            string newMessageId = Guid.NewGuid().ToString();

            // Sanitise the message content
            string sanitisedMessage = TextHelper.SanitiseRichText(packet.Message);

            // Get the current time
            DateTime timestamp = DateTime.UtcNow;

            // Add the message to the message history
            addMessageToHistory(new ChatMessage(newMessageId, packet.Uuid, sanitisedMessage, timestamp));

            // Send a response to the sender
            sendChatMessageResponse(connectionId, true, originalMessageId, newMessageId, sanitisedMessage);

            // Broadcast the chat packet to everyone but the sender
            broadcastChatMessage(packet.Uuid, newMessageId, sanitisedMessage, timestamp, new[]{connectionId});
        }

        /// <summary>
        /// Broadcasts a <see cref="ChatMessagePacket"/> to all currently logged-in users.
        /// </summary>
        /// <param name="senderUuid">Sender's UUID</param>
        /// <param name="messageId">Message ID</param>
        /// <param name="message">Message content</param>
        /// <param name="timestamp">Timestamp</param>
        /// <param name="excludedConnectionIds">Array of connection IDs to be excluded from the broadcast</param>
        private void broadcastChatMessage(string senderUuid, string messageId, string message, DateTime timestamp, string[] excludedConnectionIds = null)
        {
            // Create and pack a ChatMessagePacket
            ChatMessagePacket packet = new ChatMessagePacket
            {
                Uuid = senderUuid,
                MessageId = messageId,
                Message = message,
                Timestamp = timestamp.ToTimestamp()
            };
            Any packedPacket = ProtobufPacketHelper.Pack(packet);

            // Get an array of connection IDs for each logged in user
            string[] connectionIds = userManager.GetConnections();

            // Remove the connection IDs to be excluded from the broadcast (if any)
            if (excludedConnectionIds != null)
            {
                connectionIds = connectionIds.Except(excludedConnectionIds).ToArray();
            }

            // Send it to each of the remaining connection IDs
            foreach (string connectionId in connectionIds)
            {
                // Try send the chat message
                if (!netServer.TrySend(connectionId, MODULE_NAME, packedPacket.ToByteArray()))
                {
                    RaiseLogEntry(new LogEventArgs("Failed to send ChatMessagePacket to connection " + connectionId.Highlight(HighlightType.ConnectionID), LogLevel.ERROR));
                }
            }
        }

        /// <summary>
        /// Sends a <see cref="ChatMessagePacket"/> to the user.
        /// </summary>
        /// <param name="connectionId">Destination connection ID</param>
        /// <param name="senderUuid">Sender's UUID</param>
        /// <param name="messageId">Message ID</param>
        /// <param name="message">Message content</param>
        /// <param name="timestamp">Timestamp</param>
        private void sendChatMessage(string connectionId, string senderUuid, string messageId, string message, DateTime timestamp)
        {
            // Create and pack our chat message packet
            ChatMessagePacket packet = new ChatMessagePacket
            {
                MessageId = messageId,
                Uuid = senderUuid,
                Message = message,
                Timestamp = timestamp.ToTimestamp()
            };
            Any packedPacket = ProtobufPacketHelper.Pack(packet);

            // Send it on its way
            if (!netServer.TrySend(connectionId, MODULE_NAME, packedPacket.ToByteArray()))
            {
                RaiseLogEntry(new LogEventArgs("Failed to send ChatMessagePacket to connection " + connectionId.Highlight(HighlightType.ConnectionID), LogLevel.ERROR));
            }
        }

        /// <summary>
        /// Adds the given <see cref="ChatMessage"/> to the message history.
        /// </summary>
        /// <param name="chatMessage"><see cref="ChatMessage"/> to store</param>
        private void addMessageToHistory(ChatMessage chatMessage)
        {
            lock (messageHistoryLock)
            {
                // Add the message to history
                messageHistory.Add(chatMessage);

                // Check if we've exceeded the history capacity
                if (messageHistory.Count > messageHistoryCapacity)
                {
                    // Remove the oldest message
                    messageHistory.RemoveAt(0);
                }
            }

            // Try get the user's display name
            if (!userManager.TryGetDisplayName(chatMessage.SenderUuid, out string displayName))
            {
                displayName = "??? (" + chatMessage.SenderUuid + ")";
            }

            // Print the message to the log
            RaiseLogEntry(new LogEventArgs(String.Format("{0}: {1}", TextHelper.StripRichText(displayName), chatMessage.Message)));
        }

        /// <summary>
        /// Creates and sends a <see cref="ChatMessageResponsePacket"/> to the given connection ID.
        /// </summary>
        /// <param name="connectionId">Destination connection ID</param>
        /// <param name="success">Whether the chat message was processed successfully</param>
        /// <param name="originalMessageId">Original message ID</param>
        /// <param name="newMessageId">Newly-generated message ID</param>
        /// <param name="message">Message content</param>
        private void sendChatMessageResponse(string connectionId, bool success, string originalMessageId, string newMessageId, string message)
        {
            // Prepare and pack a ChatMessageResponsePacket
            ChatMessageResponsePacket packet = new ChatMessageResponsePacket
            {
                Success = success,
                OriginalMessageId = originalMessageId,
                NewMessageId = newMessageId,
                Message = message
            };
            Any packedPacket = ProtobufPacketHelper.Pack(packet);

            // Send it on its way
            if (!netServer.TrySend(connectionId, MODULE_NAME, packedPacket.ToByteArray()))
            {
                RaiseLogEntry(new LogEventArgs("Failed to send ChatMessageResponsePacket to connection " + connectionId.Highlight(HighlightType.ConnectionID), LogLevel.ERROR));
            }
        }

        /// <summary>
        /// Creates and sends a failed <see cref="ChatMessageResponsePacket"/> to the given connection ID.
        /// </summary>
        /// <param name="connectionId">Destination connection ID</param>
        /// <param name="originalMessageId">The original message ID</param>
        private void sendFailedChatMessageResponse(string connectionId, string originalMessageId)
        {
            sendChatMessageResponse(connectionId, false, originalMessageId, "", "");
        }
    }
}
