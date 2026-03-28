using Godot;

/// <summary>
/// Represents a villager in the game
/// </summary>
public partial class Villager : Entity
{
	private Label3D _stateLabel;

	public override void _Ready()
	{
		base._Ready();

		_stateLabel = GetNode<Label3D>("StateLabel");

		MovementSpeed = 2.5f; // [m/s]
		Work = 16.0 * 3600.0; // [s]
		restTime = 8.0; // [hr]
	}


	public override void _Process(double deltaTime)
	{
		base._Process(deltaTime);

		_stateLabel.Text = GetStateText();
	}


	private string GetStateText()
	{
		if (Task == null) return "No task\n" + Good.ToString();

		if (Task.Percentage > 0.0) return Task.Type.ToString() + " " + Task.Percentage.ToString("F0") + "%\n" + Good.ToString();
		return Task.Type.ToString() + "\n" + Good.ToString();
	}


	public override bool CanExecuteTask(Task task)
	{
		// Determine if the task can be processed based on the good that the entity carries
		switch (task.Type)
		{
			case Tasks.Raise: return Good == Goods.Ground;
			default: return Good == Goods.Nothing;
		}
	}
}
