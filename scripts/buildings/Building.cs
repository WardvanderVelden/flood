using Godot;
using System;


[GlobalClass]
public partial class Building : TileOccupant
{
	public bool IsManned { get; set; }

	protected World world;
	protected Tile tile;

	[Export]
	protected Area3D selectionArea;


	/// <summary>
	/// Try to place a building on a tile
	/// </summary>
	public bool TryToPlace(World world, Tile tile)
	{
		if (!CanPlaceOnTile(tile)) return false;

		world.AddBuilding(this);

		this.world = world;
		this.tile = tile;

		tile.Occupant = this;

		Position = new Vector3(tile.Position.X, tile.GroundLevel, tile.Position.Z);

		Initialize();
		return true;
	}


	/// <summary>
	/// Checks whether a tile can be used to place the building on
	/// </summary>
	/// <param name="tile">Tile to check</param>
	/// <returns>Returns whether the building can be placed on the tile</returns>
	protected virtual bool CanPlaceOnTile(Tile tile)
	{
		return !tile.IsOccupied;
	}


	/// <summary>
	/// Initialize the building
	/// </summary>
	protected virtual void Initialize() { }


	/// <summary>
	/// Remove the building
	/// </summary>
	public virtual void Remove()
	{
		tile.Occupant = null;
		world.RemoveBuilding(this);
		QueueFree();
	}


	/// <summary>
	/// Rotate the building
	/// </summary>
	public virtual void Rotate()
	{
		RotateY((float)Math.PI / 2);
	}
}
