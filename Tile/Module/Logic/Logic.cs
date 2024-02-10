namespace Microlayer
{
    public abstract class Logic : ExecutableModule
    {
        public override void OnAddTile()
        {
            base.OnAddTile();

            GlowMaterial = Grid.ColourPalette.LogicGlow;
            _glowMat = Instantiate(Grid.ColourPalette.LogicGlow);
            SetGlowMaterial();
        }

    }
}