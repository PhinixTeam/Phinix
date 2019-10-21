using System;
using UnityEngine;

namespace PhinixClient.GUI.Containers
{
    /// <summary>
    /// A switching container that draws different contents depending on a condition.
    /// </summary>
    internal class SwitchContainer : Displayable
    {
        /// <inheritdoc />
        public override bool IsFluidHeight => condition() ? childIfTrue.IsFluidHeight : childIfFalse.IsFluidHeight;

        /// <inheritdoc />
        public override bool IsFluidWidth => condition() ? childIfTrue.IsFluidWidth : childIfFalse.IsFluidWidth;
        
        /// <summary>
        /// Child to be drawn if <see cref="condition"/> is true.
        /// </summary>
        private Displayable childIfTrue;
        /// <summary>
        /// Child to be drawn if <see cref="condition"/> is false.
        /// </summary>
        private Displayable childIfFalse;

        /// <summary>
        /// Condition that determines which child to draw.
        /// </summary>
        private Func<bool> condition;

        /// <summary>
        /// Creates a new <see cref="SwitchContainer"/> instance with the given children, condition, and dimensions.
        /// </summary>
        /// <param name="childIfTrue">Child to be drawn if <see cref="condition"/> is true</param>
        /// <param name="childIfFalse">Child to be drawn if <see cref="condition"/> is false</param>
        /// <param name="condition">Condition that determines which child to draw</param>
        public SwitchContainer(Displayable childIfTrue, Displayable childIfFalse, Func<bool> condition)
        {
            this.childIfTrue = childIfTrue;
            this.childIfFalse = childIfFalse;
            this.condition = condition;
        }
        
        /// <inheritdoc />
        public override float CalcHeight(float width)
        {
            // Pass responsibility on to the respective child
            return condition() ? childIfTrue.CalcHeight(width) : childIfFalse.CalcHeight(width);
        }

        /// <inheritdoc />
        public override float CalcWidth(float height)
        {
            // Pass responsibility on to the respective child
            return condition() ? childIfTrue.CalcWidth(height) : childIfFalse.CalcWidth(height);
        }

        /// <inheritdoc />
        public override void Draw(Rect inRect)
        {
            if (condition())
            {
                childIfTrue.Draw(inRect);
            }
            else
            {
                childIfFalse.Draw(inRect);
            }
        }
    }
}