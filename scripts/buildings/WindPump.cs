using Flood.Tasks;
using Godot;

public partial class WindPump : Building
{
	private Tile _inletTile;
	private Tile _outletTile;

	private Node3D _wicks;

	public override void _Ready()
	{
		_wicks = GetNode<Node3D>("Wicks");
		base._Ready();
	}


	protected override void Initialize()
	{
		_inletTile = world.GetTileAt(GetNode<Node3D>("InletPosition").GlobalPosition);
		_outletTile = world.GetTileAt(GetNode<Node3D>("OutletPosition").GlobalPosition);

		world.TaskManager.AddTask(Task.CreateManningTask(this));
	}


	public override void Rotate()
	{
		base.Rotate();

		_inletTile = world.GetTileAt(GetNode<Node3D>("InletPosition").GlobalPosition);
		_outletTile = world.GetTileAt(GetNode<Node3D>("OutletPosition").GlobalPosition);
	}


	public override void _Process(double deltaTime)
	{
		if (!IsManned) return;
		if (_inletTile == null || _outletTile == null) return;
		if (!_inletTile.IsWet) return;

		_wicks.RotateZ(3.141f * (float)deltaTime);

		float flow = 0.1f; // [m^3/s]
		_inletTile.WaterLevel -= flow * (float)deltaTime;
		_outletTile.WaterLevel += flow * (float)deltaTime;
	}
}
