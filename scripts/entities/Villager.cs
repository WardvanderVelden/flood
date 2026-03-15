using System;
using Godot;

/// <summary>
/// Represents a villager in the game
/// </summary>
public partial class Villager : Entity
{
	public override bool CanExecuteTask(Task task)
	{
		// TEMPORARY: For now all the villagers can execute all types of tasks
		return true;
	}
}
