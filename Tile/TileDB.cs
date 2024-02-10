using System;
using System.Collections.Generic;
using Microlayer;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "TileDB", menuName = "Data/TileDB")]
public class TileDB : ScriptableObject
{

    public enum ModuleGroup
    {
        None,
        Logic,
        Actor,
        Sensor,
    }

    [Serializable]
	public struct Entry
	{
		public string Type;
        public ModuleGroup Group;
		public GameObject Prefab;
        public Texture2D Icon;
        public bool ShowInMenu;
        public LocalizedString[] PortTexts;

        public LocalizedString GetName()
        {
            return new LocalizedString("Modules", Type + "_Name");
        }

        public LocalizedString GetDescription()
        {
            return new LocalizedString("Modules", Type + "_Description");
        }

        public LocalizedString GetModuleInfo()
        {
            return new LocalizedString("Modules", Type + "_Info");
        }
    }

    public ColourPalette ReferencePalette;

    [SerializeField]
    private List<Entry> Tiles;
    private readonly Dictionary<string, Type> _types = new Dictionary<string, Type>();
	private readonly Dictionary<Type, Entry> _entries = new();

	private void OnEnable()
	{
		foreach (Entry entry in Tiles)
		{
			RegisterTile(entry);
		}
	}

	/// <summary>
	/// Registers tiles into the tile database. Entries set in the TileDB asset are automatically serialised.
	/// Use this method if you desire an addition at a later stage.
	///
	/// This method is the entry for assembly modding. Keep it public, please.
	/// </summary>
	/// <param name="data"></param>
	public void RegisterTile(Entry data)
	{
		var prefab = data.Prefab;
		if (!prefab)
		{
			Debug.LogError("Can't register tile without a prefab.", this);
			return;
		}
		var tile = prefab.GetComponent<Tile>();
		if (_types.ContainsKey(data.Type))
		{
			Debug.LogError($"'{data.Type}' has already been registered. Please choose a unique type.", this);
			return;
		}
		if (!tile)
		{
			Debug.LogError($"A tile derived script must be added to {data.Type}.", this);
			return;
		}
		_types[data.Type] = tile.GetType();
		_entries[_types[data.Type]] = data;
	}

	/// <summary>
	/// See GetTile
	/// </summary>
	/// <typeparam name="T">The tile to get.</typeparam>
	/// <returns>The prefab that was registered earlier or null if none exists (together with an error).</returns>
	public GameObject GetTile<T>() where T : Tile
	{
		return GetTile(typeof(T));
	}

	/// <summary>
	/// Returns the prefab of a real tile type.
	/// </summary>
	/// <param name="type">The tile to get.</param>
	/// <returns>The prefab that was registered earlier or null if none exists (together with an error).</returns>
	public GameObject GetTile(Type type)
	{
		if (!_entries.ContainsKey(type))
		{
			Debug.LogError($"Unknown tile requested. Type was {type}", this);
			return null;
		}
		return _entries[type].Prefab;
	}

	/// <summary>
	/// Returns the prefab of a string tile type.
	/// </summary>
	/// <param name="type">String type of the tile.</param>
	/// <returns>The prefab that was registered earlier or null if none exists (together with an error).</returns>
	public GameObject GetTile(string type)
	{
		if (!_types.ContainsKey(type))
		{
			Debug.LogError($"Unknown tile requested. Type was {type}", this);
			return null;
		}
		return _entries[_types[type]].Prefab;
	}

	/// <summary>
	/// Returns the script tile type from the string tile type. If the string type is unknown, the returned value is Type.Missing.GetType()
	/// </summary>
	/// <param name="type"></param>
	/// <returns></returns>
	public Type GetTileType(string type)
	{
		if (!_types.ContainsKey(type))
		{
			Debug.LogError($"Unknown tile type requested. Type was {type}", this);
			return Type.Missing.GetType();
		}
		return _types[type];
	}

	/// <summary>
	/// Returns all entries in the tile database.
	/// </summary>
	/// <returns></returns>
	public Dictionary<Type, Entry> GetEntries()
	{
		return _entries;
	}

    /// <summary>
    /// Returns all entries in the tile database.
    /// </summary>
    /// <returns></returns>
    public Entry GetEntry(Type type)
    {
        if (_entries.TryGetValue(type, out var entry)) return entry;
        Debug.LogError($"Unknown tile type requested. Type was {type}", this);
        return new Entry();
    }
}
