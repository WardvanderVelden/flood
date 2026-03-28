using Godot;
using System;
using System.Runtime;

public partial class NavigationAgent : Node
{
	private World _world;

	private float _movementSpeed;
	private MovementTypes _movementType;

	private bool _reachedLocalTarget = true;
	private bool _reachedTarget = false;

	private Vector3 _localVelocity;
	private Vector2 _localTargetPosition;

	private NavigationAgent3D _navigationAgent;

	public NavigationAgent(NavigationAgent3D godotAgent, World World, float MovementSpeed, MovementTypes MovementType)
		{
			_movementSpeed = MovementSpeed;
			_movementType = MovementType;
			_world = World;
			_navigationAgent = godotAgent;
			_navigationAgent.PathDesiredDistance = 0.1f;
			_navigationAgent.TargetDesiredDistance = 0.1f;
		}



	public Vector3 GenerateVelocity(Vector3 currentAgentPosition, Vector2 movementTarget)
		{
			if (_reachedTarget)
			{
				return new Vector3(0, 0, 0);
			}

			else if(!_reachedLocalTarget)
			{
				if ((Math.Abs(movementTarget.X - currentAgentPosition.X) < _navigationAgent.TargetDesiredDistance) && Math.Abs(movementTarget.Y - currentAgentPosition.Z) < _navigationAgent.TargetDesiredDistance)
				{
					_reachedTarget = true;
				}
				if ((Math.Abs(_localTargetPosition.X - currentAgentPosition.X) < _navigationAgent.TargetDesiredDistance) && Math.Abs(_localTargetPosition.Y - currentAgentPosition.Z) < _navigationAgent.TargetDesiredDistance)
				{
					_reachedLocalTarget = true;
				}
				return _localVelocity;
			}

			else
			{
				Vector2 currentPosition2D = new Vector2(currentAgentPosition.X, currentAgentPosition.Z);
				_localTargetPosition = GenerateTarget(currentPosition2D, movementTarget);

				Vector2 navigationalVelocity = currentPosition2D.DirectionTo(_localTargetPosition) * _movementSpeed;
				_localVelocity = new Vector3(navigationalVelocity.X, 0, navigationalVelocity.Y);
				_reachedLocalTarget = false;
				GD.Print(_localVelocity, _localTargetPosition, currentPosition2D, navigationalVelocity, _movementSpeed);
				return _localVelocity;
			}
		}

	private Vector2 GenerateTarget(Vector2 currentAgentPosition, Vector2 movementTarget)
		{
			if (_movementType == MovementTypes.WaterBased) return GenerateWaterTarget(currentAgentPosition, movementTarget);
			else if(_movementType == MovementTypes.LandBased) return GenerateLandTarget(currentAgentPosition, movementTarget);
			else return currentAgentPosition;
		}

	private Vector2 GenerateWaterTarget(Vector2 currentAgentPosition, Vector2 movementTarget)
		{
			long currentId = _world.Navigation.AstarWater.GetClosestPoint(currentAgentPosition);

			Vector2 targetPosition = new Vector2(movementTarget.X, movementTarget.Y);

			// this could lead to problems when targetPosition is not in the grid 
			long targetId = _world.Navigation.AstarWater.GetClosestPoint(targetPosition); 

			Vector2 astarPosition = _world.Navigation.AstarWater.GetPointPath(currentId, targetId)[1];
			return astarPosition;
		}

	private Vector2 GenerateLandTarget(Vector2 currentAgentPosition, Vector2 movementTarget)
		{
			long currentId = _world.Navigation.AstarWater.GetClosestPoint(currentAgentPosition);

			Vector2 targetPosition = new Vector2(movementTarget.X, movementTarget.Y);

			// this could lead to problems when targetPosition is not in the grid 
			long targetId = _world.Navigation.AstarWater.GetClosestPoint(targetPosition); 

			Vector2 astarPosition = _world.Navigation.AstarWater.GetPointPath(currentId, targetId)[1];
			return astarPosition;
		}

	
	public void Update(float delta)
		{
			
		}

}
