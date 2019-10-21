// Original file provided by Longwelwind (https://github.com/Longwelwind)
// as a part of the RimWorld mod Phi (https://github.com/Longwelwind/Phi)

namespace PhinixClient.GUI.Containers
{
    internal class HeightContainer : Container
    {
        /// <inheritdoc />
        public override bool IsFluidHeight => false;
        
        public HeightContainer(Displayable child, float height): base(child, Displayable.FLUID, height)
        {

        }
    }
}
