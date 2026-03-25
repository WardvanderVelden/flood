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
	public Navigation Navigation { get; set; }

	public Tile[,] Tiles;
	private List<Building> _buildings = new List<Building>();
	private List<Node3D> _entities = new List<Node3D>();

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
		PackedScene tileScene = GD.Load<PackedScene>("res://scenes/tile.tscn");

		Tiles = new Tile[Size, Size];
		for (int y = 0; y < Size; y++)
		{
			for (int x = 0; x < Size; x++)
			{
				Tile tile = tileScene.Instantiate<Tile>();

				bool isOnWorldEdge = (x == 0 || y == 0 || x == Size - 1 || y == Size - 1);
				tile.Initialize(x, y, isOnWorldEdge);

				_tilesNode.AddChild(tile);
				//tile.Owner = GetTree().EditedSceneRoot;
				Tiles[x, y] = tile;
			}
		}

		// Assign neighboring tiles
		for (int y = 0; y < Size; y++)
		{
			for (int x = 0; x < Size; x++)
			{
				Tile tile = Tiles[x, y];
				tile.AddNeighbor((x > 0) ? Tiles[x - 1, y] : null);
				tile.AddNeighbor((x < Size - 1) ? Tiles[x + 1, y] : null);
				tile.AddNeighbor((y > 0) ? Tiles[x, y - 1] : null);
				tile.AddNeighbor((y < Size - 1) ? Tiles[x, y + 1] : null);
			}
		}
	}


	private void GenerateSimpleWorld()
	{
		double radius = 5;
		double angle = 0.0;
		double angleStep = 5.0;

		Tile centerTile = Tiles[Size / 2, Size / 2];
		centerTile.GroundLevel = 0.5f;
		
		while (angle < 360.0)
		{
			int x = Size / 2 + (int)(Math.Cos(angle) * radius);
			int y = Size / 2 + (int)(Math.Sin(angle) * radius);

			Tile tile = Tiles[x, y];
			tile.GroundLevel = 2.0f;

			angle += angleStep;
		}
	}

	#endregion


	public Tile GetTileAt(Vector3 position)
	{
		int x = (int)Math.Round(position.X);
		int y = (int)Math.Round(position.Z);

		if (x < 0 || y < 0 || x >= Size || y >= Size) return null;
		return Tiles[x, y];
	}

	public Tile GetTileAt(Vector2 position)
	{
		return GetTileAt(new Vector3(position.X, 0.0f, position.Y));
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

		Navigation.GenerateWaterGrid();
	}


	private void ProcessWater(double deltaTime)
	{
		// Set the water level in a tile to a value to simulate a wave
		_waveTimer += deltaTime;
		//_tiles[0, 0].WaterLevel = 0.8f + (float)Math.Sin(_waveTimer / _wavePeriod * 2.0 * Math.PI) * 0.4f;
		for (int x = 0; x < Size; x++) Tiles[x, 0].WaterLevel = 0.8f + (float)Math.Sin(_waveTimer / _wavePeriod * 2.0 * Math.PI) * 0.3f;

		// Compute the water flows and update the water levels for each tile
		foreach (Tile tile in Tiles) tile.ComputeWaterFlows();
		foreach (Tile tile in Tiles) tile.UpdateWaterLevel(deltaTime);
	}


	public override void _UnhandledInput(InputEvent @event)
	{
		if (Engine.IsEditorHint()) return;

		// Handle the mouse selection
		if (Input.IsMouseButtonPressed(MouseButton.Left))
		{
			foreach (Building building in _buildings)
			{
				building.IsSelected = building.IsMouseHovered;
				if (building.IsSelected) SelectedBuilding = building;
			}
		}

		if (SelectedTile != null)
		{
			if (Input.IsActionJustReleased("raise_ground")) SelectedTile.GroundLevel += 0.5f;
			if (Input.IsActionJustReleased("lower_ground")) SelectedTile.GroundLevel -= 0.5f;

			if (Input.IsActionJustReleased("construct_windpump"))
			{
				PackedScene windPumpScene = GD.Load<PackedScene>("res://scenes/buildings/wind_pump.tscn");
				WindPump windPump = windPumpScene.Instantiate<WindPump>();

				windPump.TryToPlace(this, SelectedTile);
				SelectedBuilding = windPump;
			}
		}

		if (SelectedBuilding != null)
		{
			if (Input.IsActionJustPressed("remove_building")) SelectedBuilding.Remove();
			if (Input.IsActionJustPressed("rotate_building")) SelectedBuilding.Rotate();
		}
	}
}
