using Flood.Tasks;
using Godot;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

[Tool]
public partial class Tile : Node3D
{
	#region Properties and fields

	/// <summary>
	/// Position of the tile in tile coordinates
	/// </summary>
	public Vector2I TilePosition { get; private set; }

	private TileOccupant _occupant;
	/// <summary>
	/// Occupant of the tile if any
	/// </summary>
	public TileOccupant Occupant
	{
		get { return _occupant; }
		set
		{
			if (Occupant != null) Occupant.Tile = null;

			_occupant = value;
			if (value != null) value.Tile = this;
		}
	}

	/// <summary>
	/// Whether the tile is occupied by an occupant
	/// </summary>
	public bool IsOccupied => _occupant != null;

	/// <summary>
	/// Whether the tile is mouse overed
	/// </summary>
	public bool IsMouseOvered { get; set; }

	/// <summary>
	/// Whether the tile is selected
	/// </summary>
	public bool IsSelected { get; set; }

	private float _groundLevel = 1.0f;
	/// <summary>
	/// Ground level [m]
	/// </summary>
	public float GroundLevel
	{
		get => _groundLevel;
		set
		{
			_groundLevel = value;
			OnChangeGroundLevel();
		}
	}

	private float _waterLevel = 0.8f;
	/// <summary>
	/// Water level above the ground [m]
	/// </summary>
	public float WaterLevel
	{
		get => _waterLevel;
		set
		{
			_waterLevel = value;
			OnChangeWaterLevel();
		}
	}

	/// <summary>
	/// Top level of the tile [m]
	/// </summary>
	public float Top
	{
		get
		{
			if (IsWet) return _groundLevel + _waterLevel;
			return _groundLevel;
		}
	}

	/// <summary>
	/// Top position of the tile in world space
	/// </summary>
	public Vector3 TopPosition
	{
		get
		{
			return new Vector3(Position.X, Top, Position.Z);
		}
	}

	/// <summary>
	/// Whether the tile has any water and is therefore wet
	/// </summary>
	public bool IsWet => _waterLevel > 0.0f;

	/// <summary>
	/// Whether the tile has a significant amount of water
	/// </summary>
	public bool HasWater => _waterLevel >= 0.01f;

	/// <summary>
	/// Whether the tile is wadable by land entities
	/// </summary>
	public bool IsWadable => _waterLevel <= 0.25f;

	private List<TileNeighbor> _neighbors;
	/// <summary>
	/// Read only collection of the neighboring tiles
	/// </summary>
	public ReadOnlyCollection<TileNeighbor> Neighbors => _neighbors.AsReadOnly();

	private float _groundTaskManipulationAmount = 0.5f;

	private List<Task> _tasks;

	private MeshInstance3D _groundMesh;
	private MeshInstance3D _waterMesh;

	private Area3D _selectionArea;

	private Label3D _stateLabel;

	private bool _hasAllMeshes = false;

	#endregion


	/// <summary>
	/// Initialize a tile
	/// </summary>
	public void Initialize(int x, int y)
	{
		TilePosition = new Vector2I(x, y);
		Position = new Vector3(x, 0, y);

		_neighbors = new List<TileNeighbor>();

		_groundMesh = GetNode<MeshInstance3D>("GroundMesh");
		_waterMesh = GetNode<MeshInstance3D>("WaterMesh");

		_selectionArea = GetNode<Area3D>("SelectionArea");

		_stateLabel = GetNode<Label3D>("StateLabel");

		_tasks = new List<Task>();

		OnChangeGroundLevel();
		OnChangeWaterLevel();
	}


	/// <summary>
	/// Add a tile as a neighbor of the tile
	/// </summary>
	public bool AddNeighbor(Tile tile)
	{
		if (_neighbors == null) return false;
		if (tile == null) return false;
		if (_neighbors.Any(n => n.Tile == tile)) return false;

		_neighbors.Add(new TileNeighbor(tile));
		return true;
	}


	/// <summary>
	/// Compute the water flows between the tile and its neighbors based on the current water levels
	/// </summary>
	public void ComputeWaterFlows()
	{
		// If the tile does not have any water, set the flows to zero
		if (!IsWet)
		{
			foreach (TileNeighbor neighbor in _neighbors) neighbor.Flow = 0.0f;
			return;
		}

		// Compute the flow to every neighbor
		foreach (TileNeighbor neighbor in _neighbors)
		{
			Tile other = neighbor.Tile;

			// If the neighbor is higher than the water level, set the flow to zero and continue
			if (other.Top > Top)
			{
				neighbor.Flow = 0.0f;
				continue;
			}

			// Set the flow based on the delta height
			float delta = Top - other.Top;
			neighbor.Flow = delta * 10.0f;
		}
	}


	/// <summary>
	/// Update the water level based on the flows that exist between the tile and its neighbors
	/// </summary>
	public void UpdateWaterLevel(double deltaTime)
	{
		// Update the water level based on the flows
		foreach (TileNeighbor neighbor in _neighbors)
		{
			_waterLevel -= (float)deltaTime * neighbor.Flow;
			neighbor.Tile.WaterLevel += (float)deltaTime * neighbor.Flow;
		}
		OnChangeWaterLevel();
	}


	/// <summary>
	/// Add a raise ground task to the tile
	/// </summary>
	public void RaiseGround(TaskManager manager)
	{
		// If there is already a dig task, remove that and return
		Task digTask = _tasks.FirstOrDefault(t => t.Type == Tasks.Dig);
		if (digTask != null)
		{
			digTask.Remove();
			_tasks.Remove(digTask);
			OnChangeTasks();
			return;
		}

		// Add a raise task to the tile and the manager
		Task task = Task.CreateTileTask(this, Tasks.Raise, 0.25, 0, (Entity entity) =>
		{
			GroundLevel += _groundTaskManipulationAmount;
			entity.Good = Goods.Nothing;
		});
		_tasks.Add(task);
		manager.AddTask(task);
		OnChangeTasks();
	}


	/// <summary>
	/// Add a lower ground task to the tile
	/// </summary>
	public void LowerGround(TaskManager manager)
	{
		// If there is already a raise task, remove that and return
		Task raiseTask = _tasks.FirstOrDefault(t => t.Type == Tasks.Raise);
		if (raiseTask != null)
		{
			raiseTask.Remove();
			_tasks.Remove(raiseTask);
			OnChangeTasks();
			return;
		}

		// Prevent a dig task from being added if it would cause the ground level to become less than zero
		int lowerTaskCount = _tasks.Where(t => t.Type == Tasks.Dig).Count();
		if (GroundLevel - lowerTaskCount * _groundTaskManipulationAmount <= 1.0) return;

		// Add a dig task to the tile and the manager
		Task task = Task.CreateTileTask(this, Tasks.Dig, 0.25, 0, (Entity entity) =>
		{
			GroundLevel -= _groundTaskManipulationAmount;
			entity.Good = Goods.Ground;
		});
		_tasks.Add(task);
		manager.AddTask(task);
		OnChangeTasks();
	}


	#region Event methods

	private void OnChangeTasks()
	{
		if (_tasks.Count == 0)
		{
			_stateLabel.Visible = false;
			return;
		}

		bool isRaising = _tasks[0].Type == Tasks.Raise;
		if (isRaising) _stateLabel.Text = "Raise x" + _tasks.Count;
		else _stateLabel.Text = "Dig x" + _tasks.Count;

		_stateLabel.Visible = true;
	}


	private void OnChangeGroundLevel()
	{
		if (_groundMesh == null || _selectionArea == null) return;

		// Remove the occupant if it has any
		if (IsOccupied)
		{
			RemoveChild(Occupant);
			Occupant = null;
		}

		// Update the meshes and labels
		_groundMesh.Scale = new Vector3(1.0f, _groundLevel, 1.0f);
		_groundMesh.Position = new Vector3(0.0f, 0.5f * _groundLevel, 0.0f);

		_selectionArea.Position = new Vector3(0.0f, _groundLevel, 0.0f);

		_stateLabel.Position = new Vector3(0.0f, Top, 0.0f);

		// Remove the that are done to get rid of dig/raise tasks
		_tasks.RemoveAll(t => t.IsDone);
		OnChangeTasks();
	}


	private void OnChangeWaterLevel()
	{
		if (_waterMesh == null) return;

		_waterMesh.Visible = HasWater;
		_waterMesh.Scale = new Vector3(1.0f, _waterLevel, 1.0f);
		_waterMesh.Position = new Vector3(0.0f, _groundLevel + 0.5f * _waterLevel, 0.0f);
	}

	#endregion
}


public class TileNeighbor
{
	public Tile Tile { get; }

	/// <summary>
	/// Water flow [m^3/s]
	/// </summary>
	public float Flow { get; set; }

	public TileNeighbor(Tile tile)
	{
		Tile = tile;
		Flow = 0;
	}
}


/// <summary>
/// Represents an occupant of a tile. A tile can only ever be occupied by one tile occupant. A tile occupant may be vegetation, or the foundation of a building. Such things.
/// </summary>
public partial class TileOccupant : Node3D
{
	/// <summary>
	/// Tile that the occupant is occupying
	/// </summary>
	public Tile Tile { get; set; } = null;


	public TileOccupant()
	{

	}
}
