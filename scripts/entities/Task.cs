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
			if (Tile != null) return Tile.GlobalPosition;
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
			_progress = value;
			if (_progress >= Time) Finish();
		}
	}

	///// <summary>
	///// When the task is no longer required to be executed
	///// </summary>
	//public double Timeout { get; set; }

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
	public bool HasExecutor => Executor?.Task == this;
	
	private TaskManager _manager;

	public delegate void CallbackMethod();
	private CallbackMethod _callbackMethod;


	/// <summary>
	/// Create a tile based task
	/// </summary>
	public static Task CreateTileTask(Tile tile, Tasks type, double time, CallbackMethod callbackMethod = null)
	{
		return new Task()
		{
			Tile = tile,
			Type = type,
			Time = time,
			_callbackMethod = callbackMethod
		};
	}


	/// <summary>
	/// Create a building based task
	/// </summary>
	public static Task CreateBuildingTask(Building building, Tasks type, double time, CallbackMethod callbackMethod = null)
	{
		return new Task()
		{
			Building = building,
			Type = type,
			Time = time,
			_callbackMethod = callbackMethod
		};
	}


	public bool SetManager(TaskManager manager)
	{
		if (_manager != null) return false;
		_manager = manager;

		return true;
	}

	/// <summary>
	/// Finish the task
	/// </summary>
	public void Finish()
	{
		if (_callbackMethod != null) _callbackMethod();
		_manager.RemoveTask(this);
	}
}


public enum Tasks
{
	DigGround,
	PlaceGround,
	Man,
}
