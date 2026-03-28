using Godot;
using System;

public partial class Ship : Entity
{
    MovementTypes MovementType = MovementTypes.WaterBased;
    private Vector2 _movementTargetPosition = new Vector2(1.0f, 10.0f);

    public override void _Ready()
    {
        MovementSpeed = 2.0f;
        base._Ready();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        // Set the Y of the ship to the top of the tile it is currently in
        Tile tile = _world.GetTileAt(GlobalPosition);
        GlobalPosition = new Vector3(GlobalPosition.X, tile.Top, GlobalPosition.Z);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        Tile tile = _world.GetTileAt(GlobalPosition);
        GlobalPosition = new Vector3(GlobalPosition.X, tile.Top, GlobalPosition.Z);
        Velocity = agent.GenerateVelocity(GlobalPosition, _movementTargetPosition);
        MoveAndSlide();

    }
}
