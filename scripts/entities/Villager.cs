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
		Work = 16.0 * 3600.0; // [s]
		restTime = 8.0; // [hr]
	}


	public override bool CanExecuteTask(Task task)
	{
		// Determine if the task can be processed based on the good that the entity carries
		switch (task.Type)
		{
			case Tasks.PlaceGround: return Good == Goods.Ground;
			default: return Good == Goods.Nothing;
		}
	}
}
