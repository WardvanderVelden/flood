using Godot;
using System;

public class Navigation
{
	private World _world;

	private AStar2D _astarGeneral = new AStar2D();
	public AStar2D AstarWater = new AStar2D();
	public AStar2D AstarLand = new AStar2D();


	public Navigation(World world)
	{
		_world = world;
		GenerateGrid();
		GenerateWaterGrid();
	}


	private void GenerateGrid()
	{
		int id = 0;

		foreach (Tile tile in _world.Tiles)
		{
			Vector2 pos = new Vector2(tile.TilePosition.X, tile.TilePosition.Y);
			_astarGeneral.AddPoint(id, pos);

			if (tile.TilePosition.X > 0) _astarGeneral.ConnectPoints(id, id - World.Size);

			if (tile.TilePosition.Y > 0) _astarGeneral.ConnectPoints(id, id - 1);
			id++;
		} 
	}

	public void GenerateWaterGrid()
	{
		AstarWater = _astarGeneral;
		for (int y = 0; y < World.Size; y++)
		{
			for (int x = 0; x < World.Size; x++)
			{
				Tile tile = _world.Tiles[x, y];
				int id = y * World.Size + x;
				if (!tile.HasWater) AstarWater.SetPointDisabled(id, false);
			}
		}
	}
}
