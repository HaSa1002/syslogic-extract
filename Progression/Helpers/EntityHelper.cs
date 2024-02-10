using Macrolayer;

namespace ProgressionSystem.Helpers
{
    public class EntityHelper : Helper
    {
        public MacroEntity Entity;

        public override void Highlight()
        {
            Entity.Highlight();
        }

        public override void Hide()
        {
            Entity.HideHighlight();
        }
    }
}
