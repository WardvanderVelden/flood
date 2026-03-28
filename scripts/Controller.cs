using Godot;
using System;
using System.Linq;


[Tool]
public partial class Controller : Node3D
{
    #region Properties and fields

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

    private Control _interactionButtons;

    private Interactions _interaction = Interactions.None;
    private float _interactionAngle = 0.0f;

    private Node3D _hoveredNode;
    private Tile _previousManipulatedTile;

    #endregion


    public override void _Ready()
    {
        _camera = GetNode<Camera3D>("Camera");
        _rayCast = GetNode<RayCast3D>("Camera/RayCast");

        _interactionButtons = GetNode<Control>("UserInterface/InteractionButtons");

        _selectionMesh = GetNode<MeshInstance3D>("SelectionMesh");
        _selectionMesh.Visible = false;

        UpdateOrientation();
    }


    public override void _Process(double deltaTime)
    {
        if (Engine.IsEditorHint()) return;

        double hours = Math.Round(_world.Time / 3600.0, 1);
        GetNode<Label>("UserInterface/WorldStateLabels/TimeValue").Text = hours.ToString();
        GetNode<Label>("UserInterface/WorldStateLabels/WindValue").Text = _world.Wind.ToString();

        ControlCamera(deltaTime);
        PositionCamera();
    }


    #region Camera control

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

            if (Input.IsActionJustPressed("camera_rotate_clockwise")) FocusAngle -= (float)Math.PI / 2;
            if (Input.IsActionJustPressed("camera_rotate_counter_clockwise")) FocusAngle += (float)Math.PI / 2;
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

    #endregion


    #region Interactions

    public override void _UnhandledInput(InputEvent @event)
    {
        UpdateHoveredNode();

        bool hasGroundManipulationInteraction = _interaction == Interactions.RaiseGround || _interaction == Interactions.LowerGround;
        if (Input.IsActionJustReleased("controller_interact")) Interact();
        if (hasGroundManipulationInteraction && Input.IsActionPressed("controller_interact")) Interact();
        if (hasGroundManipulationInteraction && Input.IsActionJustReleased("controller_interact")) _previousManipulatedTile = null;
        if (Input.IsActionJustPressed("controller_rotate_interaction"))
        {
            _interactionAngle += (float)Math.PI / 2;
            _selectionMesh.RotateY((float)Math.PI / 2);
        }
        if (Input.IsActionJustPressed("controller_deselect")) SetInteraction();
    }


    private void UpdateHoveredNode()
    {
        // Set the hovered not to zero by default
        _hoveredNode = null;
        _selectionMesh.Visible = false;
        if (_interaction == Interactions.None) return;

        // Set the raycast origin to the mouse position
        Vector2 mousePosition = GetViewport().GetMousePosition();
        _rayCast.GlobalPosition = _camera.ProjectRayOrigin(mousePosition);
        _rayCast.ForceRaycastUpdate();

        // Check if the raycast is colliding
        if (_rayCast.IsColliding())
        {
            GodotObject node = _rayCast.GetCollider();

            if (node is not Area3D area) return;

            if (area.Owner is Tile tile)
            {
                _hoveredNode = tile;
                _selectionMesh.GlobalPosition = tile.GlobalPosition + new Vector3(0.0f, tile.GroundLevel, 0.0f);
            }
            else if (area.Owner is Building building)
            {
                _hoveredNode = building;
                _selectionMesh.GlobalPosition = building.GlobalPosition;
            }
        }

        // Set the visibility of the selection mesh if the hovered node is non zero
        _selectionMesh.Visible = (_hoveredNode != null);
    }


    /// <summary>
    /// Interact with the world using the selected interaction
    /// </summary>
    private void Interact()
    {
        if (_hoveredNode == null) return;

        switch (_interaction)
        {
            case Interactions.RaiseGround: ManipulateGround(true); break;
            case Interactions.LowerGround: ManipulateGround(); break;
            case Interactions.PlaceWindPump:
                PlaceBuilding("wind_pump");
                SetInteraction();
                break;
            default: break;
        }
    }


    /// <summary>
    /// Set the interaction of the controller using the <see cref="Interactions"/> enum
    /// </summary>
    public void SetInteraction(Interactions interaction = Interactions.None)
    {
        // Set the interaction
        _interaction = interaction;

        // Toggle the appropriate buttons based on the selected interaction
        _interactionButtons.GetChildren().OfType<Button>().ToList().ForEach(button => button.ButtonPressed = false);
        switch (interaction)
        {
            case Interactions.RaiseGround: _interactionButtons.GetNode<Button>("RaiseGroundButton").ButtonPressed = true; break;
            case Interactions.LowerGround: _interactionButtons.GetNode<Button>("LowerGroundButton").ButtonPressed = true; break;
            case Interactions.PlaceWindPump: _interactionButtons.GetNode<Button>("PlaceWindPumpButton").ButtonPressed = true; break;
            default: break;
        }
    }


    /// <summary>
    /// Adds tasks for manipulating the ground to the world task manager
    /// </summary>
    /// <returns>Returns whether the tasks were created</returns>
    public bool ManipulateGround(bool raiseGround = false)
    {
        if (_hoveredNode == null || _hoveredNode is not Tile tile) return false;
        if (tile.IsOccupied) return false;
        if (tile == _previousManipulatedTile) return false;

        if (raiseGround) tile.RaiseGround(_world.TaskManager);
        else tile.LowerGround(_world.TaskManager);

        _previousManipulatedTile = tile;

        return true;
    }


    /// <summary>
    /// Attempts to place a building of a certain scene in the world
    /// </summary>
    /// <returns>Returns whether the building was succesfully placed in the world</returns>
    public bool PlaceBuilding(string buildingSceneName)
    {
        if (_hoveredNode is not Tile tile) return false;

        PackedScene buildingScene = GD.Load<PackedScene>("res://scenes/buildings/" + buildingSceneName + ".tscn");
        Building building = buildingScene.Instantiate<Building>();
        building.RotateY(_interactionAngle);

        return building.TryToPlace(_world, tile);
    }

    #endregion
}


/// <summary>
/// Interactions that the controller may have with the environment
/// </summary>
public enum Interactions
{
    None = 0,
    Select = 1,
    RaiseGround = 2,
    LowerGround = 3,
    PlaceWindPump = 4,
}
