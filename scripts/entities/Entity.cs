using System;
using Godot;


/// <summary>
/// Represents an entity in the game that is subject to pathfinding and can perform jobs
/// </summary>
[GlobalClass]
public partial class Entity : Node3D
{
	public float MovementSpeed { get; set; }

	public Task Task { get; set; }

	public bool HasTask => Task != null;

	public bool IsAtTask
	{
		get
		{
			if (Task == null) return true;
			return GlobalPosition.DistanceTo(Task.Position) <= 0.1f;
		}
	}

	[Export]
	private World _world;


	public override void _Process(double deltaTime)
	{
		PerformTask(deltaTime);
	}


	/// <summary>
	/// Perform tasks by getting served tasks and executing them
	/// </summary>
	private void PerformTask(double deltaTime)
	{
		// If the entity does not have a task, attempt to get one served
		if (!HasTask) _world.TaskManager.ServeTaskTo(this);

		// If the entity has no task, return
		if (!HasTask) return;

		if (!IsAtTask) GlobalPosition += GlobalPosition.DirectionTo(Task.Position) * MovementSpeed * (float)deltaTime;
		else
		{
			if (Task.Building != null) Task.Building.IsManned = true;
			Task.Progress += deltaTime * 288.0;
		}
	}


	/// <summary>
	/// Evaluates whether an entity can execute a certain task
	/// </summary>
	/// <param name="task">Task to check</param>
	/// <returns>Returns whether the entity can execute the specified task</returns>
	public virtual bool CanExecuteTask(Task task)
	{
		// By default, entities cannot process tasks
		return false;
	}
}
