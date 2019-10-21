using UnityEngine;

namespace PhinixClient.GUI.Basic_Widgets
{
    public class BlankWidget : Displayable
    {
        /// <inheritdoc />
        public override void Draw(Rect inRect)
        {
            // Don't draw anything, just return
            return;
        }
    }
}