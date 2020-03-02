// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: Users/UserStore.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace UserManagement {

  /// <summary>Holder for reflection information generated from Users/UserStore.proto</summary>
  public static partial class UserStoreReflection {

    #region Descriptor
    /// <summary>File descriptor for Users/UserStore.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static UserStoreReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "ChVVc2Vycy9Vc2VyU3RvcmUucHJvdG8SDlVzZXJNYW5hZ2VtZW50GhBVc2Vy",
            "cy9Vc2VyLnByb3RvIoQBCglVc2VyU3RvcmUSMwoFVXNlcnMYASADKAsyJC5V",
            "c2VyTWFuYWdlbWVudC5Vc2VyU3RvcmUuVXNlcnNFbnRyeRpCCgpVc2Vyc0Vu",
            "dHJ5EgsKA2tleRgBIAEoCRIjCgV2YWx1ZRgCIAEoCzIULlVzZXJNYW5hZ2Vt",
            "ZW50LlVzZXI6AjgBYgZwcm90bzM="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::UserManagement.UserReflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::UserManagement.UserStore), global::UserManagement.UserStore.Parser, new[]{ "Users" }, null, null, null, new pbr::GeneratedClrTypeInfo[] { null, })
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class UserStore : pb::IMessage<UserStore> {
    private static readonly pb::MessageParser<UserStore> _parser = new pb::MessageParser<UserStore>(() => new UserStore());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<UserStore> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::UserManagement.UserStoreReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public UserStore() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public UserStore(UserStore other) : this() {
      users_ = other.users_.Clone();
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public UserStore Clone() {
      return new UserStore(this);
    }

    /// <summary>Field number for the "Users" field.</summary>
    public const int UsersFieldNumber = 1;
    private static readonly pbc::MapField<string, global::UserManagement.User>.Codec _map_users_codec
        = new pbc::MapField<string, global::UserManagement.User>.Codec(pb::FieldCodec.ForString(10, ""), pb::FieldCodec.ForMessage(18, global::UserManagement.User.Parser), 10);
    private readonly pbc::MapField<string, global::UserManagement.User> users_ = new pbc::MapField<string, global::UserManagement.User>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::MapField<string, global::UserManagement.User> Users {
      get { return users_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as UserStore);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(UserStore other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!Users.Equals(other.Users)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      hash ^= Users.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      users_.WriteTo(output, _map_users_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      size += users_.CalculateSize(_map_users_codec);
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(UserStore other) {
      if (other == null) {
        return;
      }
      users_.Add(other.users_);
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            users_.AddEntriesFrom(input, _map_users_codec);
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code
