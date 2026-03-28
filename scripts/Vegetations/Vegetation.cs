using Godot;
using System;
using System.Collections.Generic;
using System.Linq;


[GlobalClass]
public partial class Vegetation : TileOccupant
{
    #region Properties and fields

    /// <summary>
    /// Time vegetation need to exist before being fully grown and usable [s]
    /// </summary>
    [Export]
    public double GrowTimeAverage { get; set; }

    /// <summary>
    /// Spread of the average grow time [s]
    /// </summary>
    [Export]
    public double GrowTimeSpread { get; set; }

    /// <summary>
    /// Time vegetation can be in water before drowning [s]
    /// </summary>
    [Export]
    public double DrownTime { get; set; }

    /// <summary>
    /// Chance of vegetation spreading to a nieghboring tile per grow time [%]
    /// </summary>
    [Export]
    public double SpreadChance { get; set; }

    /// <summary>
    /// Whether or not the vegetation is fully grown
    /// </summary>
    public bool IsGrown => _growTimer >= _growTime;

    /// <summary>
    /// Whether or not the vegetation just grew
    /// </summary>
    public bool JustGrew { get; private set; } = false;

    /// <summary>
    /// Whether the vegetation has drowned
    /// </summary>
    public bool HasDrowned => _drownTimer >= DrownTime;

    /// <summary>
    /// Whether or not the vegetation just drowned
    /// </summary>
    public bool JustDrowned { get; private set; } = false;

    private double _growTime;

    private double _growTimer = 0.0;
    private double _drownTimer = 0.0;
    private double _spreadTimer = 0.0;

    private Random _random = new Random();

    #endregion


    public override void _Process(double deltaTime)
    {
        if (Engine.IsEditorHint()) return;
        if (Tile == null) return;

        if (_growTime <= 0.0) _growTime = GrowTimeAverage + GrowTimeSpread * (_random.NextDouble() * 2.0 - 1.0);

        double gameDeltaTime = deltaTime * 288.0;

        // Set the flags to low
        JustGrew = false;
        JustDrowned = false;

        // Update the drown and grow timer according to the water state of the tile the vegetation is occupying
        if (Tile.HasWater)
        {
            if (!HasDrowned)
            {
                _drownTimer += gameDeltaTime;
                if (_drownTimer > DrownTime) JustDrowned = true;
            }
        }
        else
        {
            if (!IsGrown)
            {
                _growTimer += gameDeltaTime;
                if (_growTimer > _growTime) JustGrew = true;
            }
            if (_drownTimer > 0) _drownTimer -= gameDeltaTime;
        }


        // Spread grown vegetation at random if possible
        if (IsGrown)
        {
            _spreadTimer += gameDeltaTime;
            if (_spreadTimer > _growTime)
            {
                if (_random.NextDouble() < SpreadChance / 100) TryToSpread();
                _spreadTimer %= _growTime;
            }
        }

        base._Process(deltaTime);
    }


    /// <summary>
    /// Try to spread vegetation among the neighboring tiles
    /// </summary>
    /// <returns>Returns whether the vegetation was succesfully spread</returns>
    private bool TryToSpread()
    {
        // Collect the eligable neighboring tiles
        List<Tile> tiles = Tile.Neighbors.Select(t => t.Tile).Where(t => !t.HasWater && !t.IsOccupied).ToList();
        if (tiles.Count == 0) return false;

        // Select an eligable tile at random
        Tile tile = tiles[_random.Next(tiles.Count)];

        // Add a new vegetation scene to the tile and set the occupant of the tile to be the vegetation scene
        PackedScene vegetationScene = GD.Load<PackedScene>("res://scenes/Vegetations/" + GetType().Name + ".tscn");

        Vegetation vegetation = vegetationScene.Instantiate<Vegetation>();
        vegetation.Position = new Vector3(0.0f, tile.Top, 0.0f);

        tile.AddChild(vegetation);
        tile.Occupant = vegetation;

        return true;
    }
}
