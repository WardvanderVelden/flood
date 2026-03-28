using Godot;

[GlobalClass]
public partial class Grass : Vegetation
{
	private MeshInstance3D _growingMesh;
	private MeshInstance3D _grownMesh;


	public override void _Ready()
	{
		_growingMesh = GetNode<MeshInstance3D>("GrowingMesh");
		_grownMesh = GetNode<MeshInstance3D>("GrownMesh");
	}


	public override void _Process(double deltaTime)
	{
		base._Process(deltaTime);

		_growingMesh.Visible = !IsGrown;
		_grownMesh.Visible = IsGrown;
	}
}
