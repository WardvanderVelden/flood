using Godot;
using System;

public partial class Ship : CharacterBody3D
{
	private Tile _tile;

	private NavigationAgent3D _navigationAgent;

	private float _movementSpeed = 2.0f;
	private Vector3 _movementTargetPosition = new Vector3(10.0f, 1.0f, 10.0f);

	public Vector3 MovementTarget
	{
		get { return _navigationAgent.TargetPosition; }
		set { _navigationAgent.TargetPosition = value; }
	}


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_navigationAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");

		_navigationAgent.PathDesiredDistance = 0.5f;
		_navigationAgent.TargetDesiredDistance = 0.5f;

		Callable.From(ActorSetup).CallDeferred();
	}

	public void Initialize(Tile tile)
	{
		_tile = tile;

		Position = new Vector3(tile.Position.X, 1.4f, tile.Position.Z);
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		if (_navigationAgent.IsNavigationFinished())
		{
			return;
		}
		
		Vector3 currentAgentPosition = GlobalTransform.Origin;
		Vector3 nextPathPosition = _navigationAgent.GetNextPathPosition();

		Velocity = currentAgentPosition.DirectionTo(nextPathPosition) * _movementSpeed;

		GD.Print("navigating ship", currentAgentPosition, nextPathPosition, MovementTarget);
		MoveAndSlide();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// Position = new Vector3(_tile.Position.X, _tile.Top, _tile.Position.Z);
	}

	private async void ActorSetup()
	{
		// Wait for the first physics frame so the NavigationServer can sync.
		await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

		// Now that the navigation map is no longer empty, set the movement target.
		MovementTarget = _movementTargetPosition;
	}

}
