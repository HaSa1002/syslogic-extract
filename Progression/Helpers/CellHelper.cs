using UnityEngine;

namespace ProgressionSystem.Helpers
{
    public class CellHelper : Helper
    {
        public GridMap Grid;
        public Vector2Int Cell;

        public override void Highlight()
        {
            Grid.Highlight(Cell);
        }

        public override void Hide()
        {
            Grid.HideHighlight();
        }
    }
}
