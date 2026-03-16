using System;
using Godot;

/// <summary>
/// Represents an entity in the game that is subject to pathfinding and can perform jobs
/// </summary>
[GlobalClass]
public partial class Entity : Node3D
{
	#region Properties and fields

	/// <summary>
	/// Movement speed [m/s]
	/// </summary>
	public float MovementSpeed { get; set; }

	/// <summary>
	/// Good that is carried by the entity
	/// </summary>
	public Goods Good { get; set; } = Goods.Nothing;

	/// <summary>
	/// Amount of work that the entity can still provide [s]
	/// </summary>
	public double Work { get; set; }

	/// <summary>
	/// The active task that is assigned to the entity
	/// </summary>
	public Task Task { get; set; }

	/// <summary>
	/// Whether the entity has a task
	/// </summary>
	public bool HasTask => Task != null;

	/// <summary>
	/// Whether the entity is at the task position
	/// </summary>
	public bool IsAtTaskPosition
	{
		get
		{
			if (Task == null) return true;
			return GlobalPosition.DistanceTo(Task.Position) <= 0.1f;
		}
	}

	protected double restTime = 2.0;
	protected Vector3 restPosition;

	[Export]
	private World _world;

	#endregion

	public override void _Ready()
	{
		restPosition = Position;
	}


	public override void _Process(double deltaTime)
	{
		PerformTask(deltaTime);
	}


	/// <summary>
	/// Perform tasks by getting served tasks and executing them
	/// </summary>
	private void PerformTask(double deltaTime)
	{
		// If the entity does not have a task, attempt to get one served. If we cannot serve a task to the entity, return
		if (!HasTask && !_world.TaskManager.ServeTaskTo(this)) return;

		// Track the amount of work that can still be done
		if (Task.Type == Tasks.Rest) Work += 2.0 * deltaTime * 288.0;
		else Work -= deltaTime * 288.0;

		// Execute the task
		if (!IsAtTaskPosition) GlobalPosition += GlobalPosition.DirectionTo(Task.Position) * MovementSpeed * (float)deltaTime;
		else
		{
			if (Task.Building != null) Task.Building.IsManned = true;
			Task.Progress += deltaTime * 288.0;
		}

		// If the entity can do no more work, unassign the current task and take a rest
		if (Work <= 0.0)
		{
			Task.Unassign();
			Task.CreateTileTask(_world.GetTileAt(restPosition), Tasks.Rest, restTime).AssignTo(this);
		}
	}


	/// <summary>
	/// Evaluates whether an entity can execute a certain task
	/// </summary>
	/// <param name="task">Task to check</param>
	/// <returns>Returns whether the entity can execute the specified task</returns>
	public virtual bool CanExecuteTask(Task task)
	{
		return false;
	}
}
