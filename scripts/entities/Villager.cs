using System;
using Godot;

/// <summary>
/// Represents a villager in the game
/// </summary>
public partial class Villager : Entity
{
    public override void _Ready()
    {
        base._Ready();
		MovementSpeed = 2.5f;
    }

	public override bool CanExecuteTask(Task task)
	{
		// TEMPORARY: For now all the villagers can execute all types of tasks
		return true;
	}
}
