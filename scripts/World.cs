using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


[Tool]
public partial class World : Node3D
{
	#region Properties and fields

	public const int Size = 32;

	public Tile SelectedTile { get; set; }
	public Building SelectedBuilding { get; set; }

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

				bool isOnWorldEdge = (x == 0 || y == 0 || x == Size - 1 || y == Size - 1);
				tile.Initialize(x, y, isOnWorldEdge);

				_tilesNode.AddChild(tile);
				//tile.Owner = GetTree().EditedSceneRoot;
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
		if (SelectedBuilding == building) SelectedBuilding = null;
	}


	public override void _Process(double deltaTime)
	{
		if (Engine.IsEditorHint())
		{
			if (!_hasTiles) InstantiateWorld();
			return;
		}

		ProcessWater(deltaTime);
	}


	private void ProcessWater(double deltaTime)
	{
		// Set the water level in a tile to a value to simulate a wave
		_waveTimer += deltaTime;
		//_tiles[0, 0].WaterLevel = 0.8f + (float)Math.Sin(_waveTimer / _wavePeriod * 2.0 * Math.PI) * 0.4f;
		for (int x = 0; x < Size; x++) _tiles[x, 0].WaterLevel = 0.8f + (float)Math.Sin(_waveTimer / _wavePeriod * 2.0 * Math.PI) * 0.3f;

		// Compute the water flows and update the water levels for each tile
		foreach (Tile tile in _tiles) tile.ComputeWaterFlows();
		foreach (Tile tile in _tiles) tile.UpdateWaterLevel(deltaTime);
	}
}
