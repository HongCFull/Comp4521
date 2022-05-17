using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

[RequireComponent(typeof(NavMeshAgent))]
public class MonsterController : MonoBehaviour
{
   // Set to false by default. Call EnablePlayerControl to enable the player control
   [HideInInspector] public bool enablePlayerControl = false;

   private const float RayCastDistance = 200f;
   private Monster controlledMonster;
   private Camera mainCamera;
   private NavMeshAgent navMeshAgent;
   private NavMeshPath path;
   
   private Vector3 gridPosition;
   private bool isAskingPlayerInput = false;
   private bool gotAvailableGridPos = false;
   
   private void Start() {
      path = new NavMeshPath();
      
      navMeshAgent = GetComponent<NavMeshAgent>();
      controlledMonster = GetComponent<Monster>();
      mainCamera = Camera.main;
   }

   private void Update()
   {
      if (!isAskingPlayerInput)
         return;
      HandlePlayerScreenTouchInput();
   }
   
   public void EnablePlayerControl()=> enablePlayerControl = true;

   public IEnumerator ProcessMonsterMovementCoroutine()
   {
      if (!enablePlayerControl) {
         AskForAIMovementInput();   //sync operation
      }else {
         yield return StartCoroutine(AskForPlayerMovementInput());   //async operation
      }

      GridCoordinate currentCoord= GetCurrentGridCoord();
      CombatManager.Instance.battleTerrain.UnregisterActorOnGrid(controlledMonster,currentCoord);
      ReRollGridMoveSetAtCurrentGrid(currentCoord);
      
      yield return StartCoroutine(MoveToPositionCoroutine(gridPosition));
      while (!HasReachedPosition(gridPosition)) {
         yield return null;
      }

      currentCoord = GetCurrentGridCoord();
      CombatManager.Instance.battleTerrain.RegisterTurnBasedActorOnGrid(controlledMonster,currentCoord);
      PerformMoveSetAtCurrentGrid(currentCoord);
      ResetMonsterControllerState();
   }

   void HandlePlayerScreenTouchInput()
   {
      if (!Input.GetMouseButtonDown(0))
         return;

      Vector3 screenToWorldPoint = mainCamera.ScreenToWorldPoint(Input.mousePosition);
      Vector3 direction = mainCamera.transform.rotation * Vector3.forward * 100f;
      Ray ray = new Ray(screenToWorldPoint, RayCastDistance * direction);
      RaycastHit hit;
      Vector3 gridCenter = Vector3.zero;
      Debug.DrawRay(ray.origin, RayCastDistance * ray.direction, Color.cyan, 3f);

      bool hitSomeThing = Physics.Raycast(ray, out hit, RayCastDistance);
      if (!hitSomeThing) {
         Debug.Log("cant hit something,remember to assign collider to the object!");
         return;
      }

      GameObject objectHit = hit.transform.gameObject;
      //Debug.Log("ray cast object name = "+objectHit +" with pos = "+objectHit.transform.position);
      if (objectHit.layer.Equals(LayerMask.NameToLayer("TurnBasedGrid"))) {
         gridCenter = CombatManager.Instance.battleTerrain.GetGridCenterOnNavmeshByWorldPos(objectHit.transform.position);
      }

      if (IsValidGrid(gridCenter)) {
         gridPosition = gridCenter;
         gotAvailableGridPos = true;
      }
   }
   
   void AskForAIMovementInput()
   {
      gridPosition = GetRandomReachablePosition(6);
   }

   IEnumerator AskForPlayerMovementInput()
   {
      isAskingPlayerInput = true;   //enable the update method to process player input 
      while (!gotAvailableGridPos) {   //Wait until the player clicked a valid grid 
         //Debug.Log("Waiting a valid pos");
         yield return null;
      }
   }
   
   private bool IsValidGrid(Vector3 worldPos)
   {
      int movementRange = controlledMonster.GetMonsterMovementRange();
      GridCoordinate targetCoord = CombatManager.Instance.battleTerrain.GetGridCoordByWorldPos(worldPos);
      GridCoordinate currentCoord = CombatManager.Instance.battleTerrain.GetGridCoordByWorldPos(gameObject.transform.position);

      int manhattanDist = Mathf.Abs(currentCoord.row - targetCoord.row) +
                          Mathf.Abs(currentCoord.col - targetCoord.col);
      if (manhattanDist > movementRange)
         return false;

      if (CombatManager.Instance.battleTerrain.gridMoveSets[targetCoord.row, targetCoord.col] == MoveSetOnGrid.MoveSetType.Obstacle)
         return false;

      return true;
   }
   
   IEnumerator MoveToPositionCoroutine(Vector3 position)
   {
      //Debug.Log("Moving to "+position);
      if (!PositionIsReachable(position)) {
         Debug.Log("Pos "+position+" is not reachable!");
         yield break;
      }
      
      navMeshAgent.destination = position;
      while (!HasReachedPosition(position)) {
         yield return new WaitForSeconds(0.2f);;
      }
   }

   void ReRollGridMoveSetAtCurrentGrid(GridCoordinate coord) =>CombatManager.Instance.battleTerrain.ReRollMoveSetAtGrid(coord);

   void PerformMoveSetAtCurrentGrid(GridCoordinate coord)
   {
      MoveSetOnGrid.MoveSetType moveSet = CombatManager.Instance.battleTerrain.PopMoveSetAtGrid(coord);
      controlledMonster.PerformMoveSet(moveSet);
   }

   GridCoordinate GetCurrentGridCoord()
   {
      BattleTerrain battleTerrain = CombatManager.Instance.battleTerrain;
      return battleTerrain.GetGridCoordByWorldPos(transform.position);
   }

   bool PositionIsReachable(Vector3 position)
   {
      navMeshAgent.CalculatePath(position, path);
      if (path.status == NavMeshPathStatus.PathPartial ||
          path.status == NavMeshPathStatus.PathInvalid)
         return false;
      return true;
   }
   
   private bool HasReachedPosition(Vector3 position,float tolerance = 0.5f) => (transform.position - position).magnitude < tolerance;
   
   private void ResetMonsterControllerState()
   {
      isAskingPlayerInput = false;
      gotAvailableGridPos = false;
   }
   
//NOTE: For Development Testing Only   
   Vector3 GetRandomReachablePosition(float walkRadius)
   {
      Vector3 randomPosition = transform.position;
      randomPosition.x += Random.insideUnitCircle.x*walkRadius;
      randomPosition.y = 0;
      randomPosition.z += Random.insideUnitCircle.y*walkRadius;
      return CombatManager.Instance.battleTerrain.GetGridCenterOnNavmeshByWorldPos(randomPosition);
   }
   
}
