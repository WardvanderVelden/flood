using Godot;
using System;

public partial class Camera : Camera3D
{
	private Vector3 _direction;
	private Vector3 _orthogonal;

	[Export]
	public float Speed = 10.0f;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_direction = new Vector3((float)-Math.Sin(Rotation.Y), 0.0f, (float)-Math.Cos(Rotation.Y));
		_orthogonal = new Vector3(_direction.Z, 0.0f, -_direction.X);
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionPressed("pan_up")) GlobalPosition += (float)delta * Speed * _direction;
		if (Input.IsActionPressed("pan_down")) GlobalPosition -= (float)delta * Speed * _direction;
		if (Input.IsActionPressed("pan_left")) GlobalPosition += (float)delta * Speed * _orthogonal;
		if (Input.IsActionPressed("pan_right")) GlobalPosition -= (float)delta * Speed * _orthogonal;
	}
}
