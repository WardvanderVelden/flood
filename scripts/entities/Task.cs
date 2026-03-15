using Godot;
using System;
using static Task;


/// <summary>
/// Represents a task that can be executed by an entity
/// </summary>
public class Task
{
	/// <summary>
	/// Tile associated with the task
	/// </summary>
	public Tile Tile { get; private set; }

	/// <summary>
	/// Building associated with the task
	/// </summary>
	public Building Building { get; private set; }

	/// <summary>
	/// Position of the task
	/// </summary>
	public Vector3 Position
	{
		get
		{
			if (Tile != null) return Tile.TopPosition;
			if (Building != null) return Building.GlobalPosition;
			return Vector3.Zero;
		}
	}

	/// <summary>
	/// Type of task
	/// </summary>
	public Tasks Type { get; private set; }

	/// <summary>
	/// Amount of time the task takes
	/// </summary>
	public double Time { get; private set; }

	private double _progress = 0.0;
	/// <summary>
	/// Amount of time that has already been spent on completing the task
	/// </summary>
	public double Progress
	{
		get => _progress;
		set
		{
			if (Time <= 0.0) return;

			_progress = value;
			if (_progress >= Time) Complete();
		}
	}

	/// <summary>
	/// Priority of the task
	/// </summary>
	public int Priority { get; set; } = 0;

	/// <summary>
	/// Whether the task is feasible
	/// </summary>
	public bool IsFeasible { get; set; } = true;

	/// <summary>
	/// Entity that is executing the task
	/// </summary>
	public Entity Executor { get; set; }

	/// <summary>
	/// Whether the task is being executed
	/// </summary>
	public bool IsExecuting => Executor?.Task == this;
	
	private TaskManager _manager;
	/// <summary>
	/// Task manager of the task
	/// </summary>
	public TaskManager Manager
	{
		get => _manager;
		set
		{
			if (_manager != null) return;
			_manager = value;
		}
	}

	public delegate void CallbackMethod();
	private CallbackMethod _callbackMethod;


	/// <summary>
	/// Clone a task
	/// </summary>
	/// <returns>Returns a cloned task</returns>
	public Task Clone()
	{
		return new Task()
		{
			Tile = Tile,
			Building = Building,
			Type = Type,
			Time = Time,
			Priority = Priority,
			_callbackMethod = _callbackMethod
		};
	}


	/// <summary>
	/// Create a tile based task
	/// </summary>
	/// <param name="time">Amount of time [hr] the task taskes. If the time is zero, the task is persistant</param>
	public static Task CreateTileTask(Tile tile, Tasks type, double time, int priority = 0, CallbackMethod callbackMethod = null)
	{
		return new Task()
		{
			Tile = tile,
			Type = type,
			Time = time * 3600.0,
			Priority = priority,
			_callbackMethod = callbackMethod
		};
	}


	/// <summary>
	/// Create a building based task
	/// </summary>
	/// /// <param name="time">Amount of time [hr] the task taskes. If the time is zero, the task is persistant</param>
	public static Task CreateBuildingTask(Building building, Tasks type, double time, int priority = 0, CallbackMethod callbackMethod = null)
	{
		return new Task()
		{
			Building = building,
			Type = type,
			Time = time * 3600.0,
			Priority = priority,
			_callbackMethod = callbackMethod
		};
	}


	/// <summary>
	/// Create a manning task for a certain building
	/// </summary>
	public static Task CreateManningTask(Building building)
	{
		return Task.CreateBuildingTask(building, Tasks.Man, 0, 1);
	}


	/// <summary>
	/// Assign a task to an entity
	/// </summary>
	/// <param name="entity">Entity to assign to the task</param>
	public void AssignTo(Entity entity)
	{
		if (entity.Task != null)
		{
			Task previousTask = entity.Task;
			if (previousTask.Building != null) previousTask.Building.IsManned = false;
			previousTask.Executor = null;
		}

		entity.Task = this;
		Executor = entity;
	}


	/// <summary>
	/// Complete the task
	/// </summary>
	public void Complete()
	{
		// Call the callback function
		if (_callbackMethod != null) _callbackMethod();

		// Abandon the task
		Abandon();

        // Remove the task from the task manager if it has one
        _manager?.RemoveTask(this);
    }


	/// <summary>
	/// Abandon the current task by stopping work on it by the currently assigned entity
	/// </summary>
	public bool Abandon()
	{
		if (Executor == null) return false;
		Executor.Task = null;

        // If the task is a building related task, remove the manned flag
        if (Building != null) Building.IsManned = false;

        return true;
	}
}


public enum Tasks
{
	Rest,
	DigGround,
	PlaceGround,
	Man,
}
