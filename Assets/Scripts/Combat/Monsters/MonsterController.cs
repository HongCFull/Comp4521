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
   [HideInInspector] public bool enablePlayerControl {get;private set;} = false;

   private const float RayCastDistance = 200f;
   private Monster controlledMonster;
   private Camera mainCamera;
   private NavMeshAgent navMeshAgent;
   private NavMeshPath path;
   private Animator animator;
   
   private Vector3 gridPosition;
   private bool isAskingPlayerInput = false;
   private bool gotAvailableGridPos = false;
   
   private void Start() {
      path = new NavMeshPath();
      
      navMeshAgent = GetComponent<NavMeshAgent>();
      controlledMonster = GetComponent<Monster>();
      animator = GetComponent<Animator>();
      mainCamera = Camera.main;
   }

   private void Update()
   {
      if (!isAskingPlayerInput)
         return;
   }
   
   public void EnablePlayerControl()=> enablePlayerControl = true;

   public IEnumerator ProcessMonsterMovementCoroutine()
   {
      GridCoordinate currentCoord= GetCurrentGridCoord();

      if (!enablePlayerControl) {
         AskForAIMovementInput();   //sync operation
      }else {
         CombatManager.Instance.battleTerrain.HighlightGridsInRange(currentCoord, controlledMonster.movementRange);
         ResetMonsterControllerState();
         yield return StartCoroutine(AskForPlayerInput(null));   //async operation
      }

      CombatManager.Instance.battleTerrain.UnregisterActorOnGrid(controlledMonster,currentCoord);
      CombatManager.Instance.battleTerrain.ReRollMoveSetAtGrid(currentCoord);
      CombatManager.Instance.battleTerrain.UnHighlightPreviousGrids();

      yield return StartCoroutine(MoveToPositionCoroutine(gridPosition));
      while (!HasReachedPosition(gridPosition)) {
         yield return null;
      }

      currentCoord = GetCurrentGridCoord();
      CombatManager.Instance.battleTerrain.RegisterTurnBasedActorOnGrid(controlledMonster,currentCoord);

      // Handle Moveset/Skills
      MoveSetOnGrid.MoveSetType moveSet = CombatManager.Instance.battleTerrain.PopMoveSetAtGrid(currentCoord);
      SkillAttribute skill = controlledMonster.GetSkillFromMoveSet(moveSet);
      if(skill != null) {
         if(enablePlayerControl) {
            CombatManager.Instance.battleTerrain.HighlightPotentialSkillTargets(currentCoord, skill);
            List<GridCoordinate> confirmingGrids = new List<GridCoordinate>();
            while(true) {
               ResetMonsterControllerState();
               yield return StartCoroutine(AskForPlayerInput(skill));
               GridCoordinate targetGrid = CombatManager.Instance.battleTerrain.GetGridCoordByWorldPos(gridPosition);
               CombatManager.Instance.battleTerrain.HighlightPotentialSkillTargets(currentCoord, skill);

               if(confirmingGrids.Contains(targetGrid)) {
                  break;
               } else {
                  confirmingGrids.Clear();
                  switch(skill.targetType) {
                     case SkillAttribute.TargetType.Single:
                        confirmingGrids.Add(targetGrid);
                        break;
                     case SkillAttribute.TargetType.Directional:
                        for(int i = 0; i < 4; i++) {
                           List<Vector2Int> _directionalGrids = skill.RotatedTargetTiles(i);
                           if(_directionalGrids.Contains(new Vector2Int(targetGrid.row - currentCoord.row, targetGrid.col - currentCoord.col))) {
                              foreach(Vector2Int target in _directionalGrids) {
                                 confirmingGrids.Add(new GridCoordinate(currentCoord.row + target.x, currentCoord.col + target.y));
                              }
                              break;
                           }
                        }
                        break;
                     case SkillAttribute.TargetType.Centered:
                        for(int i = 0; i < 4; i++) {
                           foreach(Vector2Int target in skill.RotatedTargetTiles(i)) {
                              confirmingGrids.Add(new GridCoordinate(currentCoord.row + target.x, currentCoord.col + target.y));
                           }
                        }
                        break;
                  }
                  CombatManager.Instance.battleTerrain.HighlightSkillTargetGrids(confirmingGrids);
               }
            }
            
         } else {
            AskForAISkillInput(skill);
         }
      }
   }

   void HandlePlayerScreenTouchInput(SkillAttribute skill) {
      if (!Input.GetMouseButtonDown(0))
         return;

      Vector3 screenToWorldPoint = mainCamera.ScreenToWorldPoint(Input.mousePosition);
      Vector3 direction = mainCamera.transform.rotation * Vector3.forward * 100f;
      Ray ray = new Ray(screenToWorldPoint, RayCastDistance * direction);
      RaycastHit hit;
      Vector3 gridCenter = Vector3.zero;
      Debug.DrawRay(ray.origin, RayCastDistance * ray.direction, Color.cyan, 3f);

      if (!Physics.Raycast(ray, out hit, RayCastDistance)) {
         Debug.Log("cant hit something,remember to assign collider to the object!");
         return;
      }

      GameObject objectHit = hit.transform.gameObject;
      // Debug.Log("ray cast object name = "+objectHit +" with pos = "+objectHit.transform.position);
      if (objectHit.layer.Equals(LayerMask.NameToLayer("TurnBasedGrid"))) {
         gridCenter = CombatManager.Instance.battleTerrain.GetGridCenterOnNavmeshByWorldPos(objectHit.transform.position);
      }

      if (IsValidGrid(gridCenter, skill)) {
         gridPosition = gridCenter;
         gotAvailableGridPos = true;
      }
   }
   
   void AskForAIMovementInput() {
      gridPosition = GetRandomReachablePosition(6);
      gotAvailableGridPos = true;
   }

   void AskForAISkillInput(SkillAttribute skill) {

      GridCoordinate currentCoord = CombatManager.Instance.battleTerrain.GetGridCoordByWorldPos(gameObject.transform.position);
      List<GridCoordinate> potentialTargets = new List<GridCoordinate>();
      for(int i = 0; i < 4; i++) {
         foreach(Vector2Int target in skill.RotatedTargetTiles(i)) {
            potentialTargets.Add(new GridCoordinate(currentCoord.row + target.x, currentCoord.col + target.y));
         }
      }

      gridPosition = CombatManager.Instance.battleTerrain.GetGridCenterOnNavmeshByCoord(potentialTargets[Random.Range(0, potentialTargets.Count)]);
      gotAvailableGridPos = true;
   }

   IEnumerator AskForPlayerInput(SkillAttribute skill) {
      isAskingPlayerInput = true;   // enable the update method to process player input
      while (!gotAvailableGridPos) {   // Wait until the player clicked a valid grid
         // Debug.Log("Waiting a valid pos");
         HandlePlayerScreenTouchInput(skill);
         yield return null;
      }
      isAskingPlayerInput = false;
   }
   
   private bool IsValidGrid(Vector3 worldPos, SkillAttribute skill) {
      if(skill == null) { // Movement judge
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
      } else {
         GridCoordinate currentCoord = CombatManager.Instance.battleTerrain.GetGridCoordByWorldPos(gameObject.transform.position);
         List<GridCoordinate> potentialTargets = new List<GridCoordinate>();
         for(int i = 0; i < 4; i++) {
            foreach(Vector2Int target in skill.RotatedTargetTiles(i)) {
               potentialTargets.Add(new GridCoordinate(currentCoord.row + target.x, currentCoord.col + target.y));
            }
         }

         GridCoordinate targetCoord = CombatManager.Instance.battleTerrain.GetGridCoordByWorldPos(worldPos);
         if(potentialTargets.Contains(targetCoord)) return true;
         return false;
      }
   }
   
   IEnumerator MoveToPositionCoroutine(Vector3 position)
   {
      //Debug.Log("Moving to "+position);
      if (!PositionIsReachable(position)) {
         Debug.Log("Pos "+position+" is not reachable!");
         yield break;
      }

      animator.SetBool("Moving", true);
      navMeshAgent.destination = position;
      while (!HasReachedPosition(position)) {
         yield return new WaitForSeconds(0.2f);;
      }
      animator.SetBool("Moving", false);
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
