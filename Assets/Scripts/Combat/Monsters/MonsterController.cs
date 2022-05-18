using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

[RequireComponent(typeof(NavMeshAgent),typeof(MonsterAnimator))]
public class MonsterController : MonoBehaviour
{
   // Set to false by default. Call EnablePlayerControl to enable the player control
   [HideInInspector] public bool enablePlayerControl {get;private set;} = false;

   private const float RayCastDistance = 200f;
   
   private BattleMap battleMap = null;
   private Monster controlledMonster;
   private MonsterAnimator monsterAnimator;
   private Camera mainCamera;
   private NavMeshAgent navMeshAgent;
   private NavMeshPath path;
   
   private Vector3 gridPosition;
   private bool isAskingPlayerInput = false;
   private bool gotAvailableGridPos = false;
   private bool gotTargetGridForTheMove = false;
   
   private void Start() {
      path = new NavMeshPath();
      
      navMeshAgent = GetComponent<NavMeshAgent>();
      controlledMonster = GetComponent<Monster>();
      monsterAnimator = GetComponent<MonsterAnimator>();
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
      CacheBattleMapForTheFirstTime();
      GridCoordinate originalCoord = GetCurrentGridCoord();

      if (!enablePlayerControl) {
         AskForAIMovementInput();   //AI
      }else {
         yield return StartCoroutine(AskForPlayerMovementInput(originalCoord));   //Player
      }

      yield return StartCoroutine(MoveToGridPositionCoroutine(gridPosition));
      
      GridCoordinate finalCoord = GetCurrentGridCoord();
      yield return PerformMoveSetAtGrid(finalCoord);

      ReportActionToBattleMap(originalCoord,finalCoord);
      ResetMonsterControllerState();
   }

   private void CacheBattleMapForTheFirstTime()
   {
      if (!battleMap)
         battleMap = BattleMap.Instance;
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
      //Debug.DrawRay(ray.origin, RayCastDistance * ray.direction, Color.cyan, 3f);

      bool hitSomeThing = Physics.Raycast(ray, out hit, RayCastDistance);
      if (!hitSomeThing) {
         Debug.Log("cant hit something,remember to assign collider to the object!");
         return;
      }

      GameObject objectHit = hit.transform.gameObject;
      //Debug.Log("ray cast object name = "+objectHit +" with pos = "+objectHit.transform.position);
      if (objectHit.layer.Equals(LayerMask.NameToLayer("TurnBasedGrid"))) {
         gridCenter = battleMap.GetGridCenterOnNavmeshByWorldPos(objectHit.transform.position);
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

   IEnumerator AskForPlayerMovementInput(GridCoordinate currentCoord)
   {
      battleMap.HighlightGridsInRange(currentCoord, controlledMonster.movementRange);

      isAskingPlayerInput = true;   //enable the update method to process player input 
      while (!gotAvailableGridPos) {   //Wait until the player clicked a valid grid 
         //Debug.Log("Waiting a valid pos");
         yield return null;
      }
      battleMap.UnHighlightPreviousGrids();
   }
   
   bool IsValidGrid(Vector3 worldPos)
   {
      int movementRange = controlledMonster.GetMonsterMovementRange();
      GridCoordinate targetCoord = battleMap.GetGridCoordByWorldPos(worldPos);
      GridCoordinate currentCoord = battleMap.GetGridCoordByWorldPos(gameObject.transform.position);

      int manhattanDist = Mathf.Abs(currentCoord.row - targetCoord.row) +
                          Mathf.Abs(currentCoord.col - targetCoord.col);
      if (manhattanDist > movementRange)
         return false;

      if (battleMap.gridMoveSets[targetCoord.row, targetCoord.col] == MoveSetOnGrid.MoveSetType.Obstacle)
         return false;

      return true;
   }
   
   IEnumerator MoveToGridPositionCoroutine(Vector3 gridPos)
   {
      if (!PositionIsReachable(gridPos)) {
         Debug.Log("Pos "+gridPos+" is not reachable!");
         yield break;
      }
      
      monsterAnimator.SetMovingBool(true);
      navMeshAgent.destination = gridPos;
      while (!HasReachedPosition(gridPos)) {
         yield return null;
      }
      monsterAnimator.SetMovingBool(false);
   }
   
//TODO:

   IEnumerator PerformMoveSetAtGrid(GridCoordinate coord)
   {
      MoveSetOnGrid.MoveSetType moveSet = battleMap.PopMoveSetAtGrid(coord);
      bool needUserInput = controlledMonster.RequireUserInputForSkill(moveSet);
      if (!needUserInput) {
         controlledMonster.CastSkillAtGrid(moveSet, coord);
         yield break;
      }
      
      yield return null;
   }

   GridCoordinate GetCurrentGridCoord() => battleMap.GetGridCoordByWorldPos(transform.position);
   
   void ReportActionToBattleMap(GridCoordinate originalPos, GridCoordinate finalPos)
   {
      battleMap.UnregisterActorOnGrid(controlledMonster,originalPos);
      battleMap.ReRollMoveSetAtGrid(originalPos);
      battleMap.RegisterTurnBasedActorOnGrid(controlledMonster,finalPos);
   }

   bool PositionIsReachable(Vector3 position)
   {
      navMeshAgent.CalculatePath(position, path);
      if (path.status == NavMeshPathStatus.PathPartial ||
          path.status == NavMeshPathStatus.PathInvalid)
         return false;
      return true;
   }
   
   bool HasReachedPosition(Vector3 position,float tolerance = 0.5f) => (transform.position - position).magnitude < tolerance;
   
   void ResetMonsterControllerState()
   {
      isAskingPlayerInput = false;
      gotAvailableGridPos = false;
      gotTargetGridForTheMove = false;
   }
   
//NOTE: For Development Testing Only   
   Vector3 GetRandomReachablePosition(float walkRadius)
   {
      Vector3 randomPosition = transform.position;
      randomPosition.x += Random.insideUnitCircle.x*walkRadius;
      randomPosition.y = 0;
      randomPosition.z += Random.insideUnitCircle.y*walkRadius;
      return battleMap.GetGridCenterOnNavmeshByWorldPos(randomPosition);
   }
   
}
