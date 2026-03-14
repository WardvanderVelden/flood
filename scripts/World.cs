using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[Tool]
public partial class World : Node3D
{
	#region Properties and fields

	public const int Size = 15;

	public Tile SelectedTile { get; set; }
	public Building SelectedBuilding { get; set; }

	private Tile[,] _tiles;
	private List<Building> _buildings = new List<Building>();

	private double _waveTimer = 0.0;
	private double _wavePeriod = 10.0;

	private bool _hasWorld = false;

	#endregion


	#region Instantation methods

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		InstantiateWorld();
	}


	private void InstantiateWorld()
	{
		if (_hasWorld) return;

		InstantiateTiles();
		GenerateSimpleWorld();

		GD.Print("Instantiated world!");
		_hasWorld = true;
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

				AddChild(tile);
				tile.Owner = GetTree().EditedSceneRoot;
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
		AddChild(building);
	}


	public override void _Process(double deltaTime)
	{
		ProcessWater(deltaTime);
	}


	public override void _UnhandledInput(InputEvent @event)
	{
		// Handle the mouse selection
		if (Input.IsMouseButtonPressed(MouseButton.Left))
		{
			foreach (Tile tile in _tiles)
			{
				tile.IsSelected = tile.IsMouseOvered;
				if (tile.IsSelected) SelectedTile = tile;
			}

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
			if (Input.IsActionJustPressed("remove_building"))
			{
				SelectedBuilding.Remove();
				_buildings.Remove(SelectedBuilding);
				SelectedBuilding = null;
			}

			if (Input.IsActionJustPressed("rotate_building")) SelectedBuilding.Rotate();
		}
	}


	private void ProcessWater(double deltaTime)
	{
		// Set the water level in a tile to a value to simulate a wave
		_waveTimer += deltaTime;
		_tiles[2, 2].WaterLevel = 0.8f + (float)Math.Sin(_waveTimer / _wavePeriod * 2.0 * Math.PI) * 0.4f;
		//for (int y = 0; y < Size; y++) Tiles[0, y].WaterLevel = 0.8f + (float)Math.Sin(_waveTimer / _wavePeriod * 2.0 * Math.PI) * 0.4f;

		// Compute the water flows and update the water levels for each tile
		foreach (Tile tile in _tiles) tile.ComputeWaterFlows();
		foreach (Tile tile in _tiles) tile.UpdateWaterLevel(deltaTime);
	}
}
