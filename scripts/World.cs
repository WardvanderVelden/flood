using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class World : Node3D
{
	#region Properties and fields

	public const int Size = 15;

	public Tile SelectedTile { get; set; }

	private Tile[,] Tiles;

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
		InstantiateTiles();
		GenerateSimpleWorld();

		GD.Print("Instantiated world!");
		_hasWorld = true;
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

				AddChild(tile);
				tile.Owner = GetTree().EditedSceneRoot;
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
		double radius = 4;
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


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double deltaTime)
	{
		ProcessWater(deltaTime);
	}


	public override void _UnhandledInput(InputEvent @event)
	{
		// Handle the mouse selection
		if (Input.IsMouseButtonPressed(MouseButton.Left))
		{
			foreach (Tile tile in Tiles)
			{
				tile.IsSelected = tile.IsMouseOvered;
				if (tile.IsSelected) SelectedTile = tile;
			}
		}

		if (SelectedTile == null) return;
		if (Input.IsActionJustReleased("raise_ground")) SelectedTile.GroundLevel += 0.5f;
		if (Input.IsActionJustReleased("lower_ground")) SelectedTile.GroundLevel -= 0.5f;

		if (Input.IsActionJustReleased("construct_windpump"))
		{
			PackedScene windPumpScene = GD.Load<PackedScene>("res://scenes/buildings/wind_pump.tscn");
			WindPump windPump = windPumpScene.Instantiate<WindPump>();
			windPump.Initialize(SelectedTile);
			AddChild(windPump);
		}

		if (Input.IsActionJustReleased("construct_ship"))
		{
			PackedScene shipScene = GD.Load<PackedScene>("res://scenes/entities/ship.tscn");
			Ship ship = shipScene.Instantiate<Ship>();
			ship.Initialize(SelectedTile);
			AddChild(ship);
		}
	}


	private void ProcessWater(double deltaTime)
	{
		// Set the water level in a tile to a value to simulate a wave
		_waveTimer += deltaTime;
		Tiles[2, 2].WaterLevel = 0.8f + (float)Math.Sin(_waveTimer / _wavePeriod * 2.0 * Math.PI) * 0.4f;
		//for (int y = 0; y < Size; y++) Tiles[0, y].WaterLevel = 0.8f + (float)Math.Sin(_waveTimer / _wavePeriod * 2.0 * Math.PI) * 0.4f;

		// Compute the water flows and update the water levels for each tile
		foreach (Tile tile in Tiles) tile.ComputeWaterFlows();
		foreach (Tile tile in Tiles) tile.UpdateWaterLevel(deltaTime);
	}
}
