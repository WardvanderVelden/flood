using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


[Tool]
public partial class World : Node3D
{
	#region Properties and fields

	public const int Size = 32;
	/// <summary>
	/// Time in the world [s]
	/// </summary>
	public double Time { get; private set; } = 3600 * 8;

	/// <summary>
	/// Strength of the wind [m/s] (This will have a relation to the water height)
	/// </summary>
	public double Wind { get; private set; }

	/// <summary>
	/// Directional angle of the wind [rad]
	/// </summary>
	public float WindAngle { get; private set; }

	public TaskManager TaskManager { get; private set; }

	private Tile[,] _tiles;
	private List<Building> _buildings = new List<Building>();
	private List<Entity> _entities = new List<Entity>();

	private double _waveTimer = 0.0;
	private double _wavePeriod = 5.0;

	private bool _hasTiles = false;

	private Node3D _tilesNode;
	private Node3D _buildingsNode;
	private Node3D _entitiesNode;

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

		_hasTiles = true;
	}


	private void InstantiateTiles()
	{
		PackedScene tileScene = GD.Load<PackedScene>("res://scenes/tile.tscn");

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
		double radius = 5;
		double angle = 0.0;
		double angleStep = 5.0;

		Tile centerTile = _tiles[Size / 2, Size / 2];
		centerTile.GroundLevel = 0.5f;
		
		while (angle < 360.0)
		{
			int x = Size / 2 + (int)(Math.Cos(angle) * radius);
			int y = Size / 2 + (int)(Math.Sin(angle) * radius);

			Tile tile = _tiles[x, y];
			tile.GroundLevel = 2.0f;

			angle += angleStep;
		}
	}

	#endregion


	public Tile GetTileAt(Vector3 position)
	{
		int x = (int)position.X;
		int y = (int)position.Z;

		if (x < 0 || y < 0 || x >= Size || y >= Size) return null;
		return _tiles[x, y];
	}


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

		double nextTime = Time + deltaTime * 288.0;
		Time = nextTime;
		//if (Time < 8 * 3600 && nextTime > 8 * 3600) ProcessDailyTasks();

		ProcessWater(deltaTime);
	}


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
