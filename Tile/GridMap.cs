using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microlayer;
using ProgressionSystem;
using UnityEngine;
using Utility;

public class GridMap : MonoBehaviour, IProgressionEmitter
{
	public readonly Vector2 CellSize = new(5, 5);

    public Action ExecutionSourcesChanged;
    
    public event IProgressionEmitter.ProgressionValueChangedHandler ProgressionValueChanged;
    #pragma warning disable CS0067
    public event IProgressionEmitter.ProgressionStateChangedHandler ProgressionStateChanged;
    #pragma warning restore

    public TileDB Tiles;
    [SerializeField] private Vector2Int TopLeft = new(0, 0);
    [SerializeField] private Vector2Int BottomRight = new(0,0);
    [SerializeField] private float GridHeight;

    private Brain _brain;
    private readonly Dictionary<Vector2Int, Tile> _tiles = new();

    private readonly Dictionary<Vector2Int, GameObject> _ghostTiles = new();
    public GameObject GhostTile;

    public ColourPalette ColourPalette;

    [HideInInspector] public Material GridMat;
    [DebugOnly] public Vector3 Bounds;
    private static readonly int HighlightID = Shader.PropertyToID("_HighlightTile");
    [SerializeField] private Vector2Int highlightedTile = new(2, 3);


    public readonly HashSet<ExecutableModule> ExecutionSources = new();
    private readonly HashSet<Tile> _temporaryTiles = new();

    private const float ProgressionRemoveTile = 1;

    private void Awake()
    {
        Debug.Assert(Tiles, "A TileDB is required!", this);
        Debug.Assert(ColourPalette, "A colour palette is required!", this);
        _brain = GetComponentInParent<Brain>();

        RegisterChildren();

        // Build Mesh
        var mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        float half = CellSize.x / 2;
        mesh.vertices = new Vector3[]
        {
            new(TopLeft.x * CellSize.x - half, 0, BottomRight.y * CellSize.x + half),
            new(TopLeft.x * CellSize.x - half, 0, TopLeft.y * CellSize.x - half),
            new(BottomRight.x * CellSize.x + half, 0, TopLeft.y * CellSize.x - half),
            new(BottomRight.x * CellSize.x + half, 0, BottomRight.y * CellSize.x + half),
            new(TopLeft.x * CellSize.x - half, 0, BottomRight.y * CellSize.x + half), //4
            new(TopLeft.x * CellSize.x - half, 0, TopLeft.y * CellSize.x - half), //5
            new(BottomRight.x * CellSize.x + half, 0, TopLeft.y * CellSize.x - half), //6
            new(BottomRight.x * CellSize.x + half, 0, BottomRight.y * CellSize.x + half), //7
            new(TopLeft.x * CellSize.x - half, 0, BottomRight.y * CellSize.x + half), //8
            new(TopLeft.x * CellSize.x - half, 0, TopLeft.y * CellSize.x - half), //9
            new(BottomRight.x * CellSize.x + half, 0, TopLeft.y * CellSize.x - half), //10
            new(BottomRight.x * CellSize.x + half, 0, BottomRight.y * CellSize.x + half), //11
            new(TopLeft.x * CellSize.x - half, GridHeight - half, BottomRight.y * CellSize.x + half), // 4 --> 12
            new(TopLeft.x * CellSize.x - half, GridHeight - half, TopLeft.y * CellSize.x - half), //5 --> 13
            new(BottomRight.x * CellSize.x + half, GridHeight - half, TopLeft.y * CellSize.x - half), //6 --> 14
            new(BottomRight.x * CellSize.x + half,  GridHeight - half, BottomRight.y * CellSize.x + half), //7 --> 15
            new(TopLeft.x * CellSize.x - half, GridHeight - half, BottomRight.y * CellSize.x + half), //8 --> 16
            new(TopLeft.x * CellSize.x - half, GridHeight - half, TopLeft.y * CellSize.x - half), //9 --> 17
            new(BottomRight.x * CellSize.x + half, GridHeight - half, TopLeft.y * CellSize.x - half), //10 --> 18
            new(BottomRight.x * CellSize.x + half,  GridHeight - half, BottomRight.y * CellSize.x + half) //11 --> 19
        };
        mesh.uv = new Vector2[]
        {
            new(1, 0),
            new(1, 1),
            new(0, 1),
            new(0, 0),
            new(0, 0),
            new(0, 1),
            new(0, 0),
            new(0.125f, 0),
            new(1, 0.125f),
            new(1, 0),
            new(0.125f, 1),
            new(0, 0.125f),
            new(0.125f, 0),
            new(0.125f, 1),
            new(0, 0.125f),
            new(0, 0),
            new(1, 0),
            new(1, 0.125f),
            new(0, 1),
            new(0, 0)
        };


        mesh.triangles = new[]
        {
            2, 1, 0,
            0, 3, 2,
            10, 7, 15,
            15, 18, 10,
            8, 16, 19,
            19, 11, 8,
            17, 9, 6,
            6, 14, 17,
            13, 12, 4,
            4, 5, 13
        };

        mesh.RecalculateBounds();
        mesh.Optimize();
        mesh.RecalculateNormals();
        Bounds = new Vector3((BottomRight.x - TopLeft.x) + 1, 0, (BottomRight.y - TopLeft.y) + 1);

        // I am tired of a ton of file changes because we update this very material.
        GridMat = Instantiate(GetComponent<MeshRenderer>().sharedMaterial);
        GetComponent<MeshRenderer>().sharedMaterial = GridMat;

        //Highlight(highlightedTile);
        GridMat.SetVector(Shader.PropertyToID("_Tiling"), new Vector4((BottomRight.x - TopLeft.x) + 1, (BottomRight.y - TopLeft.y) + 1));
        GridMat.SetColor(Shader.PropertyToID("_ColorBase"), ColourPalette.GridBackground);
        GridMat.SetColor(Shader.PropertyToID("_ColorLines"), ColourPalette.GridLines);
    }

    private IEnumerator Start()
    {
        foreach (Transform child in transform)
        {
            var tile = child.GetComponent<Tile>();
            Debug.Assert(tile, child.gameObject);
            if (!tile.Inactive) continue;

            tile.gameObject.SetActive(false);
            var cell = WorldToCell(child.position);
            _ghostTiles.Add(cell, Instantiate(GhostTile, CellToWorld(cell), Quaternion.identity, transform.parent));
        }

        yield return null;
        ExecutionSourcesChanged?.Invoke();
    }


    /// <summary>
	/// Returns cell position at world position. Ignores y.
	/// </summary>
	/// <param name="world">The world position we want to use to get our cell coordinates.</param>
	/// <returns>The cell coordinates of the world position.</returns>
	public Vector2Int WorldToCell(Vector3 world)
	{
		return LocalToCell(transform.worldToLocalMatrix * world);
	}


	/// <summary>
	/// Returns the world position of a cell coordinate.
	/// </summary>
	/// <param name="cell">The cell to get the world position of.</param>
	/// <returns>The world position of the cell.</returns>
	public Vector3 CellToWorld(Vector2Int cell)
	{
		return transform.localToWorldMatrix * CellToLocal(cell);
	}


	/// <summary>
	/// Returns cell position at local position. Ignores y.
	/// </summary>
	/// <param name="localPosition">The local position whose cell position we want to get.</param>
	/// <returns>The cell position of the local position.</returns>
	public Vector2Int LocalToCell(Vector3 localPosition)
	{
		return new Vector2Int(
			Mathf.FloorToInt(localPosition.x / CellSize.x + 0.5f),
			Mathf.FloorToInt(localPosition.z / CellSize.y + 0.5f)
			);
	}


	/// <summary>
	/// Converts the cell into a local position. y-axis is hard coded to 0.
	/// </summary>
	/// <param name="cell">The cell to get the local position of.</param>
	/// <returns>The local position of the cell.</returns>
	public Vector3 CellToLocal(Vector2Int cell)
	{
		return new Vector3(cell.x * CellSize.x, 0, cell.y * CellSize.y);
	}


	/// <summary>
	/// Returns tile or null at cell position.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="cell"></param>
	/// <returns></returns>
	public T GetTile<T>(Vector2Int cell) where T : Tile
    {
        return !_tiles.ContainsKey(cell) ? null : _tiles[cell] as T;
    }


    /// <summary>
	/// Creates a tile using the tiles system type at the given coordinates.
	/// </summary>
	/// <param name="cell">The cell coordinates where to create the tile.</param>
	/// <param name="temporaryTile">If true, doesn't update the execution graph. Use with caution!</param>
	/// <typeparam name="T">The tile type to create.</typeparam>
	/// <returns>The created tile.</returns>
	public T AddTile<T>(Vector2Int cell, bool temporaryTile = false) where T : Tile
	{
		Debug.Assert(!HasTile(cell), $"A tile is already present at {cell} ({GetTile<Module>(cell)})");
		var prefab = Tiles.GetTile<T>();
		T tile;
		if (prefab)
		{
			tile = Instantiate(prefab, CellToLocal(cell), Quaternion.identity, transform).GetComponent<T>();
		}
		else
		{
			// Replace with dedicated not implemented tile.
			var emptyTile = new GameObject();
			emptyTile.transform.parent = transform;
			emptyTile.transform.position = CellToLocal(cell);
			tile = emptyTile.AddComponent<T>();
		}

		RegisterTile(tile, cell, temporaryTile);
		return tile;
	}


    /// <summary>
    /// Register an existing tile into the grid.
    /// </summary>
    /// <param name="cell">The coordinates where to find the tile.</param>
    /// <param name="object">The tile object.</param>
    /// <param name="temporaryTile">If true, doesn't update the execution graph. Use with caution!</param>
    public void AddTile(Vector2Int cell, Transform @object, bool temporaryTile = false)
	{
		if (HasTile(cell))
		{
            Debug.LogWarning($"Tried to add tile at {cell} despite existing tile.", this);
			return;
		}
		RegisterTile(@object.GetComponent<Tile>(), cell, temporaryTile);
	}

    public void ConvertTemporaryTiles()
    {
        foreach (var tile in _temporaryTiles)
        {
            if (tile is not Connection connection) continue;
            connection.OnBecamePermanent();
        }
        _temporaryTiles.Clear();
        ExecutionSourcesChanged?.Invoke();
    }

	private void RegisterTile(Tile tile, Vector2Int cell, bool temporaryTile)
	{
		tile.Grid = this;
		tile.Cell = cell;
		_tiles[cell] = tile;
		HandleTileType(tile);
		tile.OnAddTile();

        if (temporaryTile)
        {
            _temporaryTiles.Add(tile);
        }
        else
        {
            ExecutionSourcesChanged?.Invoke();
        }
    }

    /// <summary>
    /// Removes a tile from the grid.
    /// </summary>
    /// <param name="cell">The position of the tile to remove.</param>
    public void RemoveTile(Vector2Int cell)
	{
		if (!HasTile(cell))
		{
            Debug.LogWarning($"Tried to remove non existent tile at {cell}.", this);
			return;
		}
		var tile = _tiles[cell];
		tile.OnRemoveTile();
		_tiles.Remove(cell);
		RemoveTileFromTypeDictionary(tile);
        if (_temporaryTiles.Contains(tile))
        {
            _temporaryTiles.Remove(tile);
        }
        else
        {
            ExecutionSourcesChanged?.Invoke();
            ProgressionValueChanged?.Invoke(ProgressionRemoveTile);
        }
    }


    /// <summary>
	/// Returns true if the cell coordinates are occupied.
	/// </summary>
	/// <param name="cell">The position to query.</param>
	/// <returns>True if a tile exists at cell, false if not.</returns>
	public bool HasTile(Vector2Int cell)
	{
		return _tiles.ContainsKey(cell) && _tiles[cell] is not null;
	}


	/// <summary>
	/// Snaps the vector onto the grid.
	/// </summary>
	/// <param name="localPosition">The vector to snap</param>
	/// <returns>The snapped position of the vector</returns>
    private Vector3 Snap(Vector3 localPosition)
	{
		return new Vector3(
			Mathf.Floor(localPosition.x / CellSize.x + 0.5f) * CellSize.x,
			0,
			Mathf.Floor(localPosition.z / CellSize.y + 0.5f) * CellSize.y
			);
	}


	/// <summary>
	/// Snaps a world position into the grid.
	/// </summary>
	/// <param name="worldPosition">The position to snap.</param>
	/// <returns>The snapped world position.</returns>
	public Vector3 SnapWorld(Vector3 worldPosition)
	{
		var xform = transform;
		return xform.localToWorldMatrix * Snap(xform.worldToLocalMatrix * worldPosition);
	}


	/// <summary>
	/// Snaps a world position into the grid and constrains the position to be inside the bounds of the grid.
	/// </summary>
	/// <param name="worldPosition">The position to snap.</param>
	/// <returns>The snapped and constrained world position.</returns>
	public Vector3 SnapWorldConstrained(Vector3 worldPosition)
	{
		var xform = transform;
		var cell = LocalToCell(Snap(xform.worldToLocalMatrix * worldPosition));
        cell = Vector2Int.Min(cell, BottomRight);
        cell = Vector2Int.Max(cell, TopLeft);
        return CellToWorld(cell);
	}


	public Vector3 Constrain(Vector3 worldPosition)
	{
		worldPosition = Vector3.Min(worldPosition, CellToWorld(BottomRight));
		worldPosition = Vector3.Max(worldPosition, CellToWorld(TopLeft));
		return worldPosition;
	}


	/// <summary>
	/// Returns all adjacent neighbours within the grid's bounds.
	/// </summary>
	/// <param name="cell">The cell who's neighbours we want to get.</param>
	/// <returns>The neighbours.</returns>
	public IList<Vector2Int> GetNeighboursOf(Vector2Int cell)
	{

		var neighbours = new List<Vector2Int>
		{
			cell + new Vector2Int(1, 0),
			cell + new Vector2Int(0, 1),
			cell + new Vector2Int(-1, 0),
			cell + new Vector2Int(0, -1),
		};
    repeat:
        foreach (var neighbour in neighbours.Where(neighbour => !IsInBounds(neighbour)))
        {
	        neighbours.Remove(neighbour);
	        goto repeat;
        }
		return neighbours;
	}

    /// <summary>
    /// Returns all adjacent neighbours within the grid's bounds.
    /// </summary>
    /// <param name="cell">The cell who's neighbours we want to get.</param>
    /// <returns>The neighbour tiles. Non existent tiles or tiles that don't match the type are returned as null</returns>
    public T[] GetNeighboursOf<T>(Vector2Int cell) where T : Tile
    {
        var neighbourPositions = GetNeighboursOf(cell);
        var neighbours = new T[neighbourPositions.Count];
        for (int i = 0; i < neighbourPositions.Count; i++)
        {
            neighbours[i] = GetTile<T>(neighbourPositions[i]);
            if (neighbours[i] != null && !neighbours[i].isActiveAndEnabled)
            {
                neighbours[i] = null;
            }
        }

        return neighbours;
    }

	/// <summary>
	/// Checks if the given cell is inside the grid's bounds.
	/// </summary>
	/// <param name="cell">The cell to check.</param>
	/// <returns>True if in bounds, false otherwise.</returns>
	public bool IsInBounds(Vector2Int cell)
    {
		return cell.x <= BottomRight.x && cell.y <= BottomRight.y && cell.x >= TopLeft.x && cell.y >= TopLeft.y;
    }


    private void HandleTileType(Tile tile)
    {
        switch (tile)
        {
            case Actor actor:
                actor.Target = _brain.Target;
                break;
            case Sensor sensor:
                sensor.MacroSensorPosition = _brain.Target;
                ExecutionSources.Add(sensor);
                break;
            case ExecutableModule module:
                if (!module.IsExecutionSource()) break;

                ExecutionSources.Add(module);
                goto default;
            default:
                break;
        }
    }

	private void RemoveTileFromTypeDictionary(Tile tile)
	{
		if (tile is ExecutableModule module && module.IsExecutionSource())
		{
			ExecutionSources.Remove(module);
		}
	}

    public void EnableTile(Tile tile)
    {
        if (!_ghostTiles.TryGetValue(tile.Cell, out GameObject ghost))
            return;
        _ghostTiles.Remove(tile.Cell);
        Destroy(ghost);
        tile.gameObject.SetActive(true);
        tile.Inactive = false;
        var module = tile.gameObject.GetComponent<Module>();
        module.SetupOnUnlock();
        if (gameObject.layer == LayerMask.NameToLayer("Hidden"))
        {
            _brain.ReHide();
        }
    }

    public bool IsTemporary(Tile tile)
    {
        return _temporaryTiles.Contains(tile);
    }

    private void RegisterChildren()
    {
        foreach (Transform child in transform)
        {
            var tile = child.GetComponent<Tile>();
            Debug.Assert(tile, child.gameObject);
            var cell = WorldToCell(child.position);
            AddTile(cell, child);
            child.localPosition = CellToLocal(cell);
        }
    }
    public void Highlight(Vector2Int cell)
    {
        highlightedTile = cell;
        var bigger = (int)((Bounds.x < Bounds.z ? Bounds.z : Bounds.x) * CellSize.x);
        var view = (Vector3.ProjectOnPlane(SnapWorld(CellToWorld(highlightedTile)), Vector3.up) - Bounds * (CellSize.x * 0.5f)) /
                   bigger;
        var highlightPos = new Vector2(view.x, view.z);
        GridMat.SetVector(Shader.PropertyToID("_PosTileHighlight"), new Vector4(highlightPos.x, highlightPos.y));
        GridMat.SetFloat(HighlightID, 1);
    }

    public void HideHighlight()
    {
        GridMat.SetFloat(HighlightID, 0);
    }

    #region Tile Preview and Building

    public void PreviewModule(Module module)
    {
        module.Grid = this;
        module.SetMaterials(ColourPalette, Tiles.ReferencePalette);
    }

    #endregion
}