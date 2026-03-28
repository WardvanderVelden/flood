using Godot;


[GlobalClass]
public partial class Vegetation : TileOccupant
{
    #region Properties and fields

    /// <summary>
    /// Time vegetation need to exist before being fully grown and usable [s]
    /// </summary>
    [Export]
    public double GrowTime { get; set; }

    /// <summary>
    /// Time vegetation can be in water before drowning [s]
    /// </summary>
    [Export]
    public double DrownTime { get; set; }

    /// <summary>
    /// Chance of vegetation spreading to neighboring tiles if possible [%]
    /// </summary>
    [Export]
    public double SpreadChance { get; set; }

    /// <summary>
    /// Whether or not the vegetation is fully grown
    /// </summary>
    public bool IsGrown => _growTimer >= GrowTime;

    /// <summary>
    /// Whether the vegetation has drowned
    /// </summary>
    public bool HasDrowned => _drownTimer >= DrownTime;

    private double _growTimer;
    private double _drownTimer;

    #endregion

    public override void _Ready()
    {
        base._Ready();
    }


    /// <summary>
    /// Process the vegetation
    /// </summary>
    /// <param name="deltaTime"></param>
    public override void _Process(double deltaTime)
    {
        if (Tile == null) return;

        // Update the drown and grow timer according to the water state of the tile the vegetation is occupying
        if (Tile.HasWater)
        {
            if (_drownTimer < DrownTime) _drownTimer += deltaTime;

        }
        else
        {
            if (_drownTimer > 0) _drownTimer -= deltaTime;
            if (!IsGrown) _growTimer += deltaTime;
        }

        base._Process(deltaTime);
    }
}
