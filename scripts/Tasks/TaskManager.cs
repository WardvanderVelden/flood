using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;


namespace Flood.Tasks;

public class TaskManager
{
    #region Properties and fields

    private List<Task> _tasks;

	/// <summary>
	/// Tasks that are tracked by the task manager
	/// </summary>
	public ReadOnlyCollection<Task> Tasks => _tasks.AsReadOnly();

	private readonly World _world;

	#endregion


	/// <summary>
	/// Instantiate a task manager
	/// </summary>
	/// <param name="world"></param>
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
		// Compile a list of tasks that are not being executed and are executable by the entity
		List<Task> executableTasks = _tasks.Where(t => !t.IsExecuting && entity.CanExecuteTask(t)).ToList();
		if (executableTasks.Count == 0) return false;

		// Compile a list of tasks that have the same priority and are of the highest priority in the list of executable tasks
		int priority = executableTasks[0].Priority;
		List<Task> samePriorityTasks = executableTasks.Where(t => t.Priority == priority).ToList();

		// Sort the list of same priority tasks by there proximity to the entity
		samePriorityTasks = samePriorityTasks.OrderBy(t => t.Position.DistanceTo(entity.GlobalPosition)).ToList();
		Task task = samePriorityTasks[0];

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

		// Sort the tasks after adding a new one so this does not have to be repeated
		_tasks = _tasks.OrderBy(t => t.Priority).ToList();

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
