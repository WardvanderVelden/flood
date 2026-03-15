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
		MovementSpeed = 2.5f; // [m/s]
		Work = 3600.0; // [s]
		restTime = 1.0; // [hr]
	}


	public override bool CanExecuteTask(Task task)
	{
		// TEMPORARY: For now all the villagers can execute all types of tasks
		return true;
	}
}
