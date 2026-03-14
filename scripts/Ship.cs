using Godot;
using System;

public partial class Ship : Node3D
{
	private Tile _tile;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	public void Initialize(Tile tile)
	{
		_tile = tile;

		Position = new Vector3(tile.Position.X, tile.Top, tile.Position.Z);
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Position = new Vector3(_tile.Position.X, _tile.Top, _tile.Position.Z);
	}
}
