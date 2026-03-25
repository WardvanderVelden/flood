using Godot;
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

public partial class Ship : CharacterBody3D
{
	[Export]
	private World _world;

	private float _movementSpeed = 1.0f;
	private Vector3 _movementTargetPosition = new Vector3(1.0f, 1.0f, 10.0f);
	private bool _reachedLocalTarget = true;
	private bool _reachedTarget = false;

	private Vector3 _localVelocity;
	private Vector2 _localTargetPosition;

	private NavigationAgent3D _navigationAgent;

	public Vector3 MovementTarget
	{
		get { return _navigationAgent.TargetPosition; }
		set { _navigationAgent.TargetPosition = value; }
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_navigationAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");
		_navigationAgent.PathDesiredDistance = 0.1f;
		_navigationAgent.TargetDesiredDistance = 0.1f;

		MovementTarget = _movementTargetPosition;
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

		if (_reachedTarget)
		{
			GD.Print("Ship reached target");
			// has to wait for new target
		}

		else if (_reachedLocalTarget)
		{
			// GD.Print("going to new local target");
			Vector3 currentAgentPosition = GlobalTransform.Origin;
			Vector2 currentPosition = new Vector2(currentAgentPosition.X, currentAgentPosition.Z);
			long currentId = _world.Navigation.AstarWater.GetClosestPoint(currentPosition);

			Vector2 targetPosition = new Vector2(_movementTargetPosition.X, _movementTargetPosition.Z);
			long targetId = _world.Navigation.AstarWater.GetClosestPoint(targetPosition);
			
			Vector2 astarPosition = _world.Navigation.AstarWater.GetPointPath(currentId, targetId)[1];
			Vector2 navigationalVelocity = currentPosition.DirectionTo(astarPosition) * _movementSpeed;
		
			Velocity = new Vector3(navigationalVelocity.X, 0, navigationalVelocity.Y);
			_localVelocity = Velocity;
			_localTargetPosition = astarPosition;
			MoveAndSlide();
			_reachedLocalTarget = false;
			GD.Print("Astar path: ");
			foreach (Vector2 position in _world.Navigation.AstarWater.GetPointPath(currentId, targetId))
			{
				GD.Print(position);
			}
			GD.Print("current goal:", _world.Navigation.AstarWater.GetPointPath(currentId, targetId)[1], ", ", astarPosition);
			// GD.Print("current position: ", currentPosition, ", local goal: ", astarPosition, ", local velocity: ", navigationalVelocity);
			// GD.Print("A star path: ", _world.Navigation.AstarWater.GetPointPath(currentId, targetId)[0], ",", _world.Navigation.AstarWater.GetPointPath(currentId, targetId)[1], ",", _world.Navigation.AstarWater.GetPointPath(currentId, targetId)[2]);
		}
		
		else
		{
			// GD.Print("moving");
			Vector3 currentAgentPosition = GlobalTransform.Origin;
			Velocity = _localVelocity;
			MoveAndSlide();
			// GD.Print("current pos:", currentAgentPosition, ", target pos: ", _movementTargetPosition);
			if((Math.Abs(_movementTargetPosition.X - currentAgentPosition.X) < _navigationAgent.TargetDesiredDistance) && (_movementTargetPosition.Z - currentAgentPosition.Z) < _navigationAgent.TargetDesiredDistance)
			{
				_reachedTarget = true;
				GD.Print("Reached Target");
			}
			if((Math.Abs(_localTargetPosition.X - currentAgentPosition.X) < _navigationAgent.TargetDesiredDistance) && (_localTargetPosition.Y - currentAgentPosition.Z) < _navigationAgent.TargetDesiredDistance)
			{
				_reachedLocalTarget = true;
				GD.Print("Reached Local Target");
			}
		}

		
		

	}
}