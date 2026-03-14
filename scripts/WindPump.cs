using System;
using Godot;

public partial class WindPump : Building
{
	private Tile _tile;

	private Tile _inletTile;
	private Tile _outletTile;

	private Node3D _wicks;


	public override void _Ready()
	{
		_wicks = GetNode<Node3D>("Wicks");
	}

	public void Initialize(Tile tile)
	{
		_tile = tile;

		_inletTile = tile.Neighbors[3].Tile;
		_outletTile = tile.Neighbors[2].Tile;

		Position = new Vector3(tile.Position.X, tile.Top, tile.Position.Z);
	}


	public override void _Process(double deltaTime)
	{
		if (_inletTile == null || _outletTile == null) return;
		//if (_inletTile.WaterLevel < 0.1f) return;
		if (!_inletTile.HasWater) return;

		_wicks.RotateZ(3.141f * (float)deltaTime);

		float flow = 0.1f; // [m^3/s]
		_inletTile.WaterLevel -= flow * (float)deltaTime;
		_outletTile.WaterLevel += flow * (float)deltaTime;
	}
}
