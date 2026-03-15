using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Godot;

public class TaskManager
{
	private List<Task> _tasks;

	/// <summary>
	/// Tasks that are tracked by the task manager
	/// </summary>
	public ReadOnlyCollection<Task> Tasks => _tasks.AsReadOnly();

	private World _world;


	public TaskManager(World world)
	{
		_tasks = new List<Task>();
		_world = world;
	}


	/// <summary>
	/// Serve a task to an entity
	/// </summary>
	/// <param name="entity">Entity to which to serve a task</param>
	/// <returns>Whether a task was succesfully served</returns>
	public bool ServeTaskTo(Entity entity)
	{
		// First the first task that has no executor and can be executed with the highest priority
		Task task = _tasks.Where(t => !t.HasExecutor && entity.CanExecuteTask(t)).OrderBy(t => t.Priority).FirstOrDefault();
		if (task == null) return false;

		// Assign the task to the entity
		task.AssignTo(entity);
		return true;
	}


	/// <summary>
	/// Add a task to the task manager
	/// </summary>
	public bool AddTask(Task task)
	{
		_tasks.Add(task);
		task.Manager = this;

		return true;
	}


	/// <summary>
	/// Remove a task from the task manager
	/// </summary>
	public bool RemoveTask(Task task)
	{
		return _tasks.Remove(task);
	}
}
