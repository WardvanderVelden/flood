using System;
using Godot;


[GlobalClass]
public partial class Building : Node3D
{
	protected World world;
	protected Tile tile;

	public bool IsSelected { get; set; }
	public bool IsMouseHovered { get; private set; }

	[Export]
	protected Area3D selectionArea;

	public override void _Ready()
	{
		selectionArea.MouseEntered += () => IsMouseHovered = true;
		selectionArea.MouseExited += () => IsMouseHovered = false;
	}


	/// <summary>
	/// Try to place a building on a tile
	/// </summary>
	/// <param name="world"></param>
	/// <param name="tile"></param>
	/// <returns></returns>
	public bool TryToPlace(World world, Tile tile)
	{
		if (!CanPlaceOnTile(tile)) return false;

		world.AddBuilding(this);

		this.world = world;
		this.tile = tile;

		tile.IsOccupied = true;

		Position = new Vector3(tile.Position.X, tile.GroundLevel, tile.Position.Z);

		Initialize();
		return true;
	}


	protected virtual bool CanPlaceOnTile(Tile tile)
	{
		return !tile.IsOccupied;
	}


	protected virtual void Initialize() { }


	public virtual void Remove()
	{
		tile.IsOccupied = false;
		world.RemoveBuilding(this);
		QueueFree();
	}


	public virtual void Rotate()
	{
		RotateY((float)Math.PI / 2);
	}
}
