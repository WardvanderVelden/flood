using Godot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

[Tool]
public partial class Tile : Node3D
{
	#region Properties and fields

	public Vector2I TilePosition { get; private set; }

	public bool IsMouseOvered { get; set; }
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
			UpdateGroundMeshes();
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
			UpdateWaterMesh();
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
	public bool HasWater => _waterLevel >= 0.05f;

	/// <summary>
	/// Whether the tile is wadable by land entities
	/// </summary>
	public bool IsWadable => _waterLevel <= 0.25f; 

	private bool _hasGrass = true;
	/// <summary>
	/// Whether the tile has grass on it
	/// </summary>
	public bool HasGrass
	{
		get => _hasGrass;
		set
		{
			if (!value &&  _hasGrass)
			{
				_grassTimer = 0.0;
				_grassMesh.Visible = false;
			}
			if (value && !_hasGrass && _grassMesh != null) _grassMesh.Visible = true;

			_hasGrass = value;
		}
	}

	public bool IsOccupied { get; set; } = false;

	private List<TileNeighbor> _neighbors;
	public ReadOnlyCollection<TileNeighbor> Neighbors => _neighbors.AsReadOnly();

	private MeshInstance3D _groundMesh;
	private MeshInstance3D _waterMesh;

	private MeshInstance3D _grassMesh;
	private double _grassTimer;
	private const double _grassGrowTime = 15.0;

	private Area3D _selectionArea;

	private bool _hasAllMeshes = false;

	#endregion


	public override void _Ready()
	{
		_neighbors = new List<TileNeighbor>();

		_groundMesh = GetNode<MeshInstance3D>("GroundMesh");
		_waterMesh = GetNode<MeshInstance3D>("WaterMesh");
		_grassMesh = GetNode<MeshInstance3D>("GrassMesh");

		_selectionArea = GetNode<Area3D>("SelectionArea");

		UpdateGroundMeshes();
		UpdateWaterMesh();
	}


	public void Initialize(int x, int y)
	{
		TilePosition = new Vector2I(x, y);
		Position = new Vector3(x, 0, y);
	}


	public bool AddNeighbor(Tile tile)
	{
		if (_neighbors == null) return false;
		if (tile == null) return false;
		if (_neighbors.Any(n => n.Tile == tile)) return false;

		_neighbors.Add(new TileNeighbor(tile));
		return true;
	}


	public override void _Process(double deltaTime)
	{
		// Handle the grass growing logic
		if (HasWater && HasGrass) HasGrass = false;

		if (!HasWater && !HasGrass)
		{
			_grassTimer += deltaTime;
			HasGrass = (_grassTimer > _grassGrowTime);
		}
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
		UpdateWaterMesh();
	}


	private void UpdateGroundMeshes()
	{
		if (_groundMesh == null || _grassMesh == null || _selectionArea == null) return;

		_groundMesh.Scale = new Vector3(1.0f, _groundLevel, 1.0f);
		_groundMesh.Position = new Vector3(0.0f, 0.5f * _groundLevel, 0.0f);
		
		_grassMesh.Position = new Vector3(0.0f, _groundLevel + 0.05f, 0.0f);
		_selectionArea.Position = new Vector3(0.0f, _groundLevel, 0.0f);
	}


	private void UpdateWaterMesh()
	{
		if (_waterMesh == null) return;

		_waterMesh.Visible = HasWater;
		_waterMesh.Scale = new Vector3(1.0f, _waterLevel, 1.0f);
		_waterMesh.Position = new Vector3(0.0f, _groundLevel + 0.5f * _waterLevel, 0.0f);
	}
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
