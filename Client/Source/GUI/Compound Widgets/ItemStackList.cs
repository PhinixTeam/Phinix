using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace PhinixClient.GUI.Compound_Widgets
{
    public class ItemStackList : Displayable
    {
        /// <inheritdoc cref="Displayable.IsFluidWidth"/>
        public override bool IsFluidWidth => generatedLayout.IsFluidWidth;
        /// <inheritdoc cref="Displayable.IsFluidHeight"/>
        public override bool IsFluidHeight => generatedLayout.IsFluidHeight;

        /// <summary>
        /// Collection of <see cref="StackedThings"/> to be shown in the list.
        /// </summary>
        public readonly List<StackedThings> itemStacks;
        
        /// <summary>
        /// Collection of <see cref="ItemStackRow"/>s (wrapped in <see cref="MinimumContainer"/>s) derived from
        /// <see cref="itemStacks"/>.
        /// </summary>
        private readonly List<MinimumContainer> itemStackRows;
        /// <summary>
        /// Container that holds a ready-to-draw copy of <see cref="itemStackRows"/>.
        /// </summary>
        private readonly VerticalFlexContainer itemStackRowFlexContainer = new VerticalFlexContainer();
        /// <summary>
        /// Whether <see cref="itemStackRowFlexContainer"/> needs to be refreshed to accommodate changes to
        /// <see cref="itemStackRows"/>.
        /// </summary>
        private bool itemStackRowsUpdated = false;
        /// <summary>
        /// Lock object to prevent race conditions when accessing <see cref="itemStackRows"/>.
        /// </summary>
        private readonly object itemStackRowsLock = new object();

        /// <summary>
        /// Whether the item counts should be modifiable by the user.
        /// </summary>
        private bool interactive;

        /// <summary>
        /// Minimum height of each row.
        /// </summary>
        private float minimumHeight;

        /// <summary>
        /// The generated widget layout to be drawn.
        /// </summary>
        private readonly Displayable generatedLayout;

        public ItemStackList() : this(new List<StackedThings>())
        {
        }

        public ItemStackList(List<StackedThings> itemStacks, bool interactive = false, float minimumHeight = 30f)
        {
            this.itemStacks = itemStacks;
            this.interactive = interactive;
            this.minimumHeight = minimumHeight;
            
            // Pre-generate layout
            generatedLayout = new VerticalScrollContainer(itemStackRowFlexContainer);
        }

        /// <inheritdoc cref="Displayable.Draw"/>
        public override void Draw(Rect inRect)
        {
            // Check if the rows have been updated
            if (itemStackRowsUpdated)
            {
                // Try lock the row list, otherwise wait until the next cycle to refresh content
                if (Monitor.TryEnter(itemStackRowsLock))
                {
                    // Clear out the old rows and add the new ones
                    itemStackRowFlexContainer.Contents.Clear();
                    itemStackRowFlexContainer.Contents.AddRange(itemStackRows);
                    
                    // Clear the change flag
                    itemStackRowsUpdated = false;
                    
                    Monitor.Exit(itemStackRowsLock);
                }
            }
            
            // Draw the pre-generated layout
            generatedLayout.Draw(inRect);
        }

        /// <inheritdoc cref="Displayable.Update"/>
        public override void Update()
        {
            // Update each row
            foreach (MinimumContainer row in itemStackRows)
            {
                row.Update();
            }
        }

        /// <inheritdoc cref="Displayable.CalcWidth"/>
        public override float CalcWidth(float height)
        {
            return generatedLayout.CalcWidth(height);
        }

        /// <inheritdoc cref="Displayable.CalcHeight"/>
        public override float CalcHeight(float width)
        {
            return generatedLayout.CalcHeight(width);
        }

        /// <summary>
        /// Clears and regenerates <see cref="itemStackRows"/> with the contents of <see cref="itemStacks"/>.
        /// </summary>
        private void generateItemStackRows()
        {
            lock (itemStackRowsLock)
            {
                itemStackRows.Clear();
                
                for (int i = 0; i < itemStacks.Count; i++)
                {
                    // Create an ItemStackRow from this item
                    ItemStackRow row = new ItemStackRow(
                        itemStack: itemStacks[i],
                        interactive: interactive,
                        alternateBackground: i % 2 != 0
                    );

                    // Contain the row within a minimum-height container
                    MinimumContainer container = new MinimumContainer(
                        row,
                        minHeight: minimumHeight
                    );

                    // Add it to the row list
                    itemStackRows.Add(container);
                }

                // Flag the rows to be updated next frame
                itemStackRowsUpdated = true;
            }
        }
    }
}