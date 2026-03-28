using Godot;
using System;
using System.Collections.Generic;


[Tool]
public partial class World : Node3D
{
    #region Properties and fields

    /// <summary>
    /// Tile count along either side of the world
    /// </summary>
    public int Size { get; private set; } = 32;

    /// <summary>
    /// Time in the world [s]
    /// </summary>
    public double Time { get; private set; } = 3600.0 * 11.0;

    /// <summary>
    /// Strength of the wind [m/s] (This will have a relation to the water height)
    /// </summary>
    public double Wind { get; private set; }

    /// <summary>
    /// Directional angle of the wind [rad]
    /// </summary>
    public float WindAngle { get; private set; }

    public TaskManager TaskManager { get; private set; }

    public Tile[,] _tiles;
    private List<Building> _buildings = new List<Building>();
    private List<Entity> _entities = new List<Entity>();
    public Navigation Navigation { get; set; }

    private double _waveTimer = 0.0;
    private double _wavePeriod = 5.0;

    private bool _hasTiles = false;

    private Node3D _tilesNode;
    private Node3D _buildingsNode;
    private Node3D _entitiesNode;

    [Export]
    private DirectionalLight3D _sun;

    #endregion


    #region Instantation methods

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        TaskManager = new TaskManager(this);

        _tilesNode = GetNode<Node3D>("Tiles");
        _buildingsNode = GetNode<Node3D>("Buildings");
        _entitiesNode = GetNode<Node3D>("Entities");

        InstantiateWorld();
    }


    private void InstantiateWorld()
    {
        if (_hasTiles) return;

        InstantiateTiles();
        GenerateSimpleWorld();
        Navigation = new Navigation(this);
        
        _hasTiles = true;
    }
    
    private void InstantiateTiles() 
    {

		_tiles = new Tile[Size, Size];
		for (int y = 0; y < Size; y++)
		{
			for (int x = 0; x < Size; x++)
			{
				Tile tile = tileScene.Instantiate<Tile>();
				tile.Initialize(x, y);

				_tilesNode.AddChild(tile);
				_tiles[x, y] = tile;
			}
		}

		// Assign neighboring tiles
		for (int y = 0; y < Size; y++)
		{
			for (int x = 0; x < Size; x++)
			{
				Tile tile = _tiles[x, y];
				tile.AddNeighbor((x > 0) ? _tiles[x - 1, y] : null);
				tile.AddNeighbor((x < Size - 1) ? _tiles[x + 1, y] : null);
				tile.AddNeighbor((y > 0) ? _tiles[x, y - 1] : null);
				tile.AddNeighbor((y < Size - 1) ? _tiles[x, y + 1] : null);
			}
		}
	}


	private void GenerateSimpleWorld()
	{
		Vector2I center = new Vector2I(Size / 2, Size / 2);
		double radius = 5.0;

		for (int y = 0; y < Size; y++)
		{
			for (int x = 0; x < Size; x++)
			{
				Vector2I position = new Vector2I(x, y);
				if (center.DistanceTo(position) < radius)
				{
					Tile tile = _tiles[x, y];
					tile.GroundLevel = 2.0f;
					tile.WaterLevel = 0.0f;
				}
			}
		}
	}

	#endregion


	public Tile GetTileAt(int x, int y)
	{
		if (x < 0 || y < 0 || x >= Size || y >= Size) return null;
		return _tiles[x, y];
	}
	public Tile GetTileAt(Vector2 position) => GetTileAt((int)position.X, (int)position.Y);
	public Tile GetTileAt(Vector3 position) => GetTileAt((int)position.X, (int)position.Z);

	public void AddBuilding(Building building)
	{
		_buildings.Add(building);
		_buildingsNode.AddChild(building);
	}


	public void RemoveBuilding(Building building)
	{
		_buildings.Remove(building);
		_buildingsNode.RemoveChild(building);
	}


	public override void _Process(double deltaTime)
	{
		if (Engine.IsEditorHint())
		{
			if (!_hasTiles) InstantiateWorld();
			return;
		}

		
		ProcessTime(deltaTime);
		ProcessWater(deltaTime);
		Navigation.GenerateWaterGrid();
	}


	private void ProcessTime(double deltaTime)
	{
		Time = (Time + deltaTime * 288.0) % (3600.0 * 24.0);

		double dayAngle = (Time - (4.0 * 3600.0)) / (16.0 * 3600.0) * Math.PI;
		_sun.Rotation = new Vector3((float)-dayAngle, 52.0f / 180.0f * (float)Math.PI, 0.0f);
	}


	/// <summary>
	/// Process the water in the world
	/// </summary>
	private void ProcessWater(double deltaTime)
	{
		// Set the edge of the tiles to the 
		_waveTimer += deltaTime;
		float waveHeight = 0.8f + (float)Math.Sin(_waveTimer / _wavePeriod * 2.0 * Math.PI) * 0.3f;
		for (int i = 0; i < Size; i++)
		{
			_tiles[i, 0].WaterLevel = waveHeight;
			_tiles[i, Size - 1].WaterLevel = waveHeight;
			_tiles[0, i].WaterLevel = waveHeight;
			_tiles[0, Size - 1].WaterLevel = waveHeight;
		}

		// Compute the water flows and update the water levels for each tile
		foreach (Tile tile in _tiles) tile.ComputeWaterFlows();
		foreach (Tile tile in _tiles) tile.UpdateWaterLevel(deltaTime);
	}
}
