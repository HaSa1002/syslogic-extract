using UnityEngine;
using UnityEngine.Serialization;
using Utility;

namespace Microlayer
{
    public abstract class Tile : MonoBehaviour
    {
        [DebugOnly]
        public Vector2Int Cell = new(int.MaxValue, int.MaxValue);
        [HideInInspector]
        public GridMap Grid;

        [FormerlySerializedAs("IndependentActiveState")]
        [Tooltip("When set, the tile is deactivated and can't be interacted with.")]
        public bool Inactive;

        public virtual void OnRemoveTile()
        {
            Destroy(gameObject);
        }

        public virtual void OnAddTile() {}

        protected T[] GetNeighbours<T>() where T : Tile
        {
            return Grid.GetNeighboursOf<T>(Cell);
        }

        protected void Remove()
        {
            Grid.RemoveTile(Cell);
        }
    }
}