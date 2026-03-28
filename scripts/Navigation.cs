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
		GenerateWaterLandGrids();
	}


	private void GenerateGrid()
	{
		int id = 0;

		foreach (Tile tile in _world._tiles)
		{
			Vector2 pos = new Vector2(tile.TilePosition.X, tile.TilePosition.Y);
			_astarGeneral.AddPoint(id, pos);

			if (tile.TilePosition.X > 0) _astarGeneral.ConnectPoints(id, id - _world.Size);

			if (tile.TilePosition.Y > 0) _astarGeneral.ConnectPoints(id, id - 1);
			id++;
		} 
	}

	public void GenerateWaterLandGrids()
	{
		AstarWater = _astarGeneral;
		AstarLand = _astarGeneral;
		for (int y = 0; y < _world.Size; y++)
		{
			for (int x = 0; x < _world.Size; x++)
			{
				Tile tile = _world._tiles[x, y];
				int id = y * _world.Size + x;
				if (!tile.IsWadable) AstarWater.SetPointDisabled(id, false);
				else AstarLand.SetPointDisabled(id, false);
			}
		}
	}
}
