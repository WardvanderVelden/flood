using Godot;
using System;
using System.Reflection.Metadata.Ecma335;


[Tool]
public partial class Controller : Node3D
{
	#region Properties and fields

	public Node3D SelectedNode { get; set; }

	[Export]
	private float _cameraPitch;
	public float CameraPitch
	{
		get => _cameraPitch;
		set
		{
			_cameraPitch = value;
			UpdateOrientation();
		}
	}

	[Export]
	private float _cameraDistance;

	[Export]
	private float _cameraPanSpeed;

	private Vector3 _cameraNormal;
	private Vector3 _cameraOrthogonal;
	private Vector3 _cameraUp;

	private float _focusAngle = 45.0f / 180.0f * 3.14159265f;
	public float FocusAngle
	{
		get => _focusAngle;
		set
		{
			_focusAngle = value;
			UpdateOrientation();
		}
	}

	private Vector2 _focusPosition;
	private Vector2 _focusUp;
	private Vector2 _focusLeft;

	private Camera3D _camera;
	private RayCast3D _rayCast;
	private MeshInstance3D _selectionMesh;

	[Export]
	private World _world;

	#endregion


	public override void _Ready()
	{
		_camera = GetNode<Camera3D>("Camera");
		_rayCast = GetNode<RayCast3D>("Camera/RayCast");
		_selectionMesh = GetNode<MeshInstance3D>("SelectionMesh");

		UpdateOrientation();
	}


	public override void _Process(double deltaTime)
	{
		if (Engine.IsEditorHint()) return;

		ControlCamera(deltaTime);
		PositionCamera();

		HandleSelection();
	}

	 
	private void ControlCamera(double deltaTime)
	{
		// Control the focus position and the view direction using the camera controls
		if (!Engine.IsEditorHint())
		{
			float panStep = (float)deltaTime * _cameraPanSpeed;

			if (Input.IsActionPressed("camera_pan_up")) _focusPosition += panStep * _focusUp;
			if (Input.IsActionPressed("camera_pan_down")) _focusPosition -= panStep * _focusUp;
			if (Input.IsActionPressed("camera_pan_left")) _focusPosition += panStep * _focusLeft;
			if (Input.IsActionPressed("camera_pan_right")) _focusPosition -= panStep * _focusLeft;

			if (Input.IsActionJustPressed("camera_rotate_clockwise")) FocusAngle += (float)Math.PI / 4;
			if (Input.IsActionJustPressed("camera_rotate_counter_clockwise")) FocusAngle -= (float)Math.PI / 4;
		}
	}


	private void PositionCamera()
	{
		_camera.Position = new Vector3(_focusPosition.X, 0.0f, _focusPosition.Y) - _cameraNormal * _cameraDistance;
		_camera.Size = _cameraDistance;
	}


	private void UpdateOrientation()
	{
		if (_camera == null) return;

		// Compute the focus up and left (in 2D space on the XZ plane)
		_focusUp = new Vector2(1.0f, 0.0f).Rotated(-FocusAngle);
		_focusLeft = new Vector2(0.0f, -1.0f).Rotated(-FocusAngle);

		// Set the orientation of the camera
		float cameraPitch = -_cameraPitch / 180.0f * (float)Math.PI;
		_camera.Rotation = new Vector3(cameraPitch, FocusAngle - (float)Math.PI / 2, 0.0f);

		// Compute the 3D vectors that map the viewport to 3D space
		_cameraNormal = new Vector3(1.0f, 0.0f, 0.0f).Rotated(new Vector3(0.0f, 0.0f, 1.0f), cameraPitch).Rotated(new Vector3(0.0f, 1.0f, 0.0f), FocusAngle);
		_cameraOrthogonal = Vector3.Up.Cross(_cameraNormal);
		_cameraUp = _cameraNormal.Cross(_cameraOrthogonal);

		// Set the position of the camera
		_camera.Position = new Vector3(_focusPosition.X, 0.0f, _focusPosition.Y) - _cameraNormal * _cameraDistance;
	}


	private void HandleSelection()
	{
		if (Input.IsMouseButtonPressed(MouseButton.Left))
		{
			Vector2 mousePosition = GetViewport().GetMousePosition();

			//PhysicsDirectSpaceState3D spaceState = _camera.GetWorld3D().DirectSpaceState;
			//Vector3 rayStart = _camera.ProjectRayOrigin(mousePosition);
			//Vector3 rayEnd = rayStart + _cameraNormal * 100.0f; // _camera.ProjectLocalRayNormal(mousePosition) * 100.0f;
			//var result = spaceState.IntersectRay(PhysicsRayQueryParameters3D.Create(rayStart, rayEnd));

			//if (result.Count > 0)
			//{
			//	GD.Print("Has result!");
			//}

			// Move the raycast to the position on the screen
			Vector2 halfScreenSize = 0.5f * GetViewport().GetVisibleRect().Size;

			_rayCast.Position = new Vector3((mousePosition.X - halfScreenSize.X) / halfScreenSize.X * _cameraDistance, -(mousePosition.Y - halfScreenSize.Y) / halfScreenSize.Y * _cameraDistance, 0.0f);
			////_rayCast.Position = _camera.ProjectRayOrigin(mousePosition);

			//GD.Print(_rayCast.Position.ToString());

			////_rayCast.TargetPosition = _camera.ProjectLocalRayNormal(mousePosition) * 100.0f;
			////_rayCast.Position = _camera.ProjectRayOrigin(mousePosition);
			//_rayCast.ForceRaycastUpdate();

			// Check if the raycast is colliding
			if (_rayCast.IsColliding())
			{
				GodotObject node = _rayCast.GetCollider();
	
				if (node is Area3D area)
				{
					if (area.Owner is Tile tile)
					{
						SelectedNode = tile;
						_selectionMesh.GlobalPosition = tile.GlobalPosition + tile.Top * Vector3.Up;
					}
					if (area.Owner is Building building) SelectedNode = building;

					if (SelectedNode != null) _selectionMesh.GlobalPosition = SelectedNode.GlobalPosition;
				}
			}
		}
	}
}
