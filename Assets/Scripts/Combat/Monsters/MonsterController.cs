using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Analytics;
using Grid = System.Windows.Forms.DataVisualization.Charting.Grid;
using Random = UnityEngine.Random;

[RequireComponent(typeof(NavMeshAgent),typeof(MonsterAnimator))]
public class MonsterController : MonoBehaviour
{
   // Set to false by default. Call EnablePlayerControl to enable the player control
   [HideInInspector] public bool enablePlayerControl {get; private set;} = false;

   private const float RayCastDistance = 200f;
   
   private BattleMap battleMap = null;
   private Monster controlledMonster;
   private MonsterAnimator monsterAnimator;
   private Camera mainCamera;
   private NavMeshAgent navMeshAgent;
   private NavMeshPath path;

   private bool isAskingPlayerMovementInput = false;
   private bool gotWalkableGridPos = false;
   private Vector3 targetGridForMovement;
   
   private bool isAskingPlayerFirstCastConfirm = false;
   private bool gotFirstCastConfirm = false;
   private GridCoordinate firstCastConfirmedGrid;
   
   private bool isAskingPlayerSecondCastConfirm = false;
   private bool gotSecondCastConfirm = false;
   private bool isTargetChanged = false;
   
   private List<GridCoordinate> potentialCastingGrids;
   private List<GridCoordinate> confirmingGrids;
   private List<GridCoordinate> aiAttackTargets;

   private void Start() {
      path = new NavMeshPath();
      potentialCastingGrids = new List<GridCoordinate>();
      confirmingGrids = new List<GridCoordinate>();
      aiAttackTargets = new List<GridCoordinate>();
      
      navMeshAgent = GetComponent<NavMeshAgent>();
      controlledMonster = GetComponent<Monster>();
      monsterAnimator = GetComponent<MonsterAnimator>();
      
      mainCamera = Camera.main;
   }

   private void Update()
   {
      if (isAskingPlayerMovementInput) {
         HandlePlayerScreenTouchInputForMovement();
      }

      if (isAskingPlayerFirstCastConfirm || isAskingPlayerSecondCastConfirm) {
         HandlePlayerScreenTouchInputForSkill();
      }
   }
   
   public void EnablePlayerControl()=> enablePlayerControl = true;

   public IEnumerator ProcessMonsterMovementCoroutine()
   {
      CacheBattleMapForTheFirstTime();
      GridCoordinate originalCoord = GetCurrentGridCoord();

      if (!enablePlayerControl) {
         AskForAIMovementInput(originalCoord);   //AI
      }else {
         yield return StartCoroutine(AskForPlayerMovementInput(originalCoord));   //Player
      }

      ReportToBattleMapForLeavingGrid(originalCoord);
      
      yield return StartCoroutine(MoveToGridPositionCoroutine(targetGridForMovement));
      
      GridCoordinate finalCoord = GetCurrentGridCoord();
      yield return PerformMoveSetAtGrid(finalCoord);

      ReportActionToBattleMap(finalCoord);
      ResetMonsterControllerState();
   }

   void CacheBattleMapForTheFirstTime()
   {
      if (!battleMap)
         battleMap = controlledMonster.battleMap;
   } 
   
   void HandlePlayerScreenTouchInputForMovement()
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

      if (IsWalkableGrid(gridCenter)) {
         targetGridForMovement = gridCenter;
         gotWalkableGridPos = true;
      }
   }
   
   void HandlePlayerScreenTouchInputForSkill()
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

      GridCoordinate clickedGrid = battleMap.GetGridCoordByWorldPos(gridCenter); 
      if (isAskingPlayerFirstCastConfirm && IsValidCastGrid(gridCenter)) {
         firstCastConfirmedGrid = clickedGrid;
         gotFirstCastConfirm = true;
         isAskingPlayerFirstCastConfirm = false;
      }
      
      if(isAskingPlayerSecondCastConfirm) {
         
         //invalid attacking target 
         if (!potentialCastingGrids.Contains(clickedGrid))
            return;

         if (IsClickingOnConfirmingGrids(clickedGrid)) {
            gotSecondCastConfirm = true;
            isAskingPlayerSecondCastConfirm = false;
         }

         //Player swapped target to attack
         if (!IsClickingOnConfirmingGrids(clickedGrid) && IsValidCastGrid(gridCenter)) {
            firstCastConfirmedGrid = clickedGrid;
            isAskingPlayerSecondCastConfirm = false;
            isTargetChanged = true; //this need to be the last in the if statement
         }
      }
     
   }
//==============================================================================================================================
//Action Coroutine
//==============================================================================================================================

   IEnumerator PerformMoveSetAtGrid(GridCoordinate targetGrid)
   {
      MoveSetOnGrid.MoveSetType moveSet = battleMap.PopMoveSetAtGrid(targetGrid);
      SkillAttribute skill = controlledMonster.GetSkillFromMoveSet(moveSet);
      
      bool needUserInput = controlledMonster.RequireUserInputForSkill(moveSet);
      
      //AI is performing 
      if (!enablePlayerControl) {
         AskForAISkillInput(skill);
         if (needUserInput) {
            List<TurnBasedActorType> actorsToExclude = new List<TurnBasedActorType>();
            actorsToExclude.Add(TurnBasedActorType.EnemyMonster);
            
            TrimOutNonDamageTarget(aiAttackTargets,actorsToExclude);
            foreach (var VARIABLE in aiAttackTargets) {
               Debug.Log("final target grid "+VARIABLE);
            }

            controlledMonster.PerformSkillToGrids(moveSet, aiAttackTargets);
            yield break;
         }
         else {
            controlledMonster.PerformSkillToGrids(moveSet,skill.GetPotentialCastingPoint(targetGrid,battleMap.GetNumOfRow(),battleMap.GetNumOfCol()));
         }
         
      }

      //Player is performing
      potentialCastingGrids = skill.GetPotentialCastingPoint(targetGrid, battleMap.GetNumOfRow(), battleMap.GetNumOfCol());
      if (needUserInput) {
         battleMap.HighlightCasterGrid(targetGrid);
         battleMap.HighlightPotentialCastingGrid(potentialCastingGrids);
         yield return StartCoroutine(AskForFirstPlayerSkillInputConfirm(skill,targetGrid));
         yield return StartCoroutine(AskForSecondPlayerSkillInputConfirm(moveSet,skill,targetGrid));
         battleMap.UnhighlightGrid(targetGrid);
      }
      else {
         controlledMonster.PerformSkillToGrids(moveSet,skill.GetPotentialCastingPoint(targetGrid,battleMap.GetNumOfRow(),battleMap.GetNumOfCol()));
      }
  
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
   
//==============================================================================================================================
//Input Request
//==============================================================================================================================
   void AskForAIMovementInput(GridCoordinate currentPos)
   {
      targetGridForMovement = GetOneGridForAIMovement(currentPos,controlledMonster.GetMonsterMovementRange());
   }

   IEnumerator AskForPlayerMovementInput(GridCoordinate currentCoord)
   {
      battleMap.HighlightWalkableGridsInRange(currentCoord, controlledMonster.movementRange);

      isAskingPlayerMovementInput = true;   //enable the update method to process player input 
      while (!gotWalkableGridPos) {   //Wait until the player clicked a valid grid 
         //Debug.Log("Waiting a valid pos");
         yield return null;
      }

      isAskingPlayerMovementInput = false;
      battleMap.UnHighlightWalkableGrids();
   }
   
   void AskForAISkillInput(SkillAttribute skill) {
      GridCoordinate currentCoord = battleMap.GetGridCoordByWorldPos(transform.position);
      Debug.Log("Ai walked to grid:"+currentCoord +" to perform skill:"+skill.name);
      if (skill.GetSkillType()==SkillAttribute.SkillType.Attack)
      {
         float maxDamage = 0f;
         List<GridCoordinate> potentTargets = skill.GetPotentialCastingPoint(currentCoord, battleMap.GetNumOfRow(), battleMap.GetNumOfCol());
         Debug.Log("Ai potential target = "+potentTargets.Count);
         // foreach (var VARIABLE in potentTargets) {
         //    Debug.Log("AI potential grid = "+VARIABLE);
         // }
         foreach (var potentTarget in potentTargets) {
            float totalDmgDealInThisGrid = 0f;
            firstCastConfirmedGrid = potentTarget;
            UpdateConfirmingGrids(skill, currentCoord);
            Debug.Log("there are "+confirmingGrids.Count+" confirming grids at potential gird "+potentTarget);
            foreach (var target in confirmingGrids) {
               if (!battleMap.actorsCoord.ContainsKey(target))
                  continue;

               if (battleMap.actorsCoord[target].turnBasedActorType == TurnBasedActorType.FriendlyControllableMonster ||
                   battleMap.actorsCoord[target].turnBasedActorType == TurnBasedActorType.FriendlyUncontrollableMonster) {
                  totalDmgDealInThisGrid += skill.Damage;
               }
            }

            if (totalDmgDealInThisGrid > maxDamage) {
               maxDamage = totalDmgDealInThisGrid;
               aiAttackTargets = new List<GridCoordinate>(confirmingGrids);
               foreach (var grid in aiAttackTargets) {
                  Debug.Log("AI selected:" +grid+" to attack");
               }
            }
         }
      }
   }

   IEnumerator AskForFirstPlayerSkillInputConfirm(SkillAttribute skill,GridCoordinate monsterCoord) {
    
      isAskingPlayerFirstCastConfirm = true;   
      while (!gotFirstCastConfirm) {
         yield return null;
      }
      isAskingPlayerFirstCastConfirm = false;
      
      //Get the first confirm
      UpdateConfirmingGrids(skill,monsterCoord);
      battleMap.HighlightConfirmingGrids(confirmingGrids);
   }
   
   IEnumerator AskForSecondPlayerSkillInputConfirm(MoveSetOnGrid.MoveSetType moveSet,SkillAttribute skill,GridCoordinate monsterCoord) {
    
      while (true)
      {
         isAskingPlayerSecondCastConfirm = true;
         while (!gotSecondCastConfirm) {
            if(isTargetChanged) {   //if the player swapped target
               //battleMap.UnHighlightConfirmingGrids();
               battleMap.HighlightPotentialCastingGrid(potentialCastingGrids);
               UpdateConfirmingGrids(skill,monsterCoord);
               battleMap.HighlightConfirmingGrids(confirmingGrids);
               break;
            }
            yield return null;
         }

         if (isTargetChanged) {
            isTargetChanged = false;
            continue;
         }
         
         battleMap.UnHighlightPotentialCastingGrids();
         battleMap.UnHighlightConfirmingGrids();
         
         Debug.Log("Performing "+skill.name);
         //battleMap.DealDamageToGrids(confirmingGrids, skill.Damage);
         
         List<TurnBasedActorType> actorsToExclude = new List<TurnBasedActorType>();
         actorsToExclude.Add(TurnBasedActorType.FriendlyControllableMonster);
         actorsToExclude.Add(TurnBasedActorType.FriendlyUncontrollableMonster);
         TrimOutNonDamageTarget(confirmingGrids,actorsToExclude);
         
         controlledMonster.PerformSkillToGrids(moveSet, confirmingGrids);
         isAskingPlayerSecondCastConfirm = false;
         yield break;
         
      }
   }

//===============================================================================================================================
//Helper
//===============================================================================================================================
   GridCoordinate GetCurrentGridCoord() => battleMap.GetGridCoordByWorldPos(transform.position);

   void ReportToBattleMapForLeavingGrid(GridCoordinate originalPos)
   {
      battleMap.UnregisterActorOnGrid(controlledMonster,originalPos);
      battleMap.ReRollMoveSetAtGrid(originalPos);
   }
   void ReportActionToBattleMap( GridCoordinate finalPos)
   {
      battleMap.RegisterTurnBasedActorOnGrid(controlledMonster,finalPos);
   }

   void UpdateConfirmingGrids(SkillAttribute skill,GridCoordinate currentCoord)
   {
      confirmingGrids.Clear();
      switch(skill.GetTargetType()) {
         case SkillAttribute.TargetType.Single:
            confirmingGrids.Add(firstCastConfirmedGrid);
            break;
         case SkillAttribute.TargetType.Directional:
            for(int i = 0; i < 4; i++) {
               List<Vector2Int> _directionalGrids = skill.RotatedTargetTiles(i);
               if(_directionalGrids.Contains(new Vector2Int(firstCastConfirmedGrid.row - currentCoord.row, firstCastConfirmedGrid.col - currentCoord.col))) {
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
   }

   void ClearConfirmingGrids() => confirmingGrids.Clear();

   void TrimOutNonDamageTarget(List<GridCoordinate> targets,List<TurnBasedActorType> typesToExclude)
   {
      List<GridCoordinate> gridsToExclude = new List<GridCoordinate>();
      foreach (var target in targets) {
         if(!battleMap.actorsCoord.ContainsKey(target))
            continue;
         TurnBasedActor actor = battleMap.actorsCoord[target];
         // foreach (TurnBasedActorType excludeType in typesToExclude) {
         //    if(actor.turnBasedActorType == excludeType)
         //       targets.Remove(target);
         // }
         //
         if (typesToExclude.Contains(actor.turnBasedActorType))
         {
            Debug.Log("Excluded grid "+target+" to damage as it is a "+actor.turnBasedActorType);
            gridsToExclude.Add(target);
         }
      }

      foreach (GridCoordinate gridToExclude in gridsToExclude) {
         targets.Remove(gridToExclude);
      }
   }
   void ResetMonsterControllerState()
   {
      //Debug.Log("reset state");
      isAskingPlayerMovementInput = false;
      gotWalkableGridPos = false;
      isAskingPlayerFirstCastConfirm = false;
      gotFirstCastConfirm = false;
      gotSecondCastConfirm = false;
      isTargetChanged = false;
      potentialCastingGrids.Clear();
      confirmingGrids.Clear();
   }

//===============================================================================================================================
//Verification
//===============================================================================================================================
   bool IsWalkableGrid(Vector3 worldPos)
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

   bool IsValidCastGrid(Vector3 inputGridPos)
   {
      GridCoordinate inputGridCoord = battleMap.GetGridCoordByWorldPos(inputGridPos);
      return potentialCastingGrids.Contains(inputGridCoord);
   }

   bool IsClickingOnConfirmingGrids(GridCoordinate clickedGrid) => confirmingGrids.Contains(clickedGrid);

   bool PositionIsReachable(Vector3 position)
   {
      navMeshAgent.CalculatePath(position, path);
      if (path.status == NavMeshPathStatus.PathPartial ||
          path.status == NavMeshPathStatus.PathInvalid)
         return false;
      return true;
   }
   
   bool HasReachedPosition(Vector3 position,float tolerance = 0.5f) => (transform.position - position).magnitude < tolerance;
   
   
//NOTE: For Development Testing Only   
   Vector3 GetOneGridForAIMovement(GridCoordinate center,int range)
   {
      List<GridCoordinate> walkableGrids = battleMap.GetWalkableGridsInRange(center,range);

      if (walkableGrids.Count == 0) {
         Debug.Log("No possible move");
         return transform.position;
      }
      float maxScore = 0;
      GridCoordinate targetCoord =walkableGrids[0];
      foreach (var grid in walkableGrids)
      {
         float evaluationScore = EvaluateMoveSetHeuristic(grid, battleMap.gridMoveSets[grid.row, grid.col]);
         Debug.Log("Scores at grid "+grid+ " with move "+battleMap.gridMoveSets[grid.row, grid.col]+" ="+evaluationScore);
         if (evaluationScore > maxScore) {
            maxScore = evaluationScore;
            targetCoord = grid;
         }
      }
      
      return battleMap.GetGridCenterOnNavmeshByCoord(targetCoord);
      //return battleMap.GetGridCenterOnNavmeshByWorldPos(randomPosition);
   }

   float EvaluateMoveSetHeuristic(GridCoordinate moveGrid,MoveSetOnGrid.MoveSetType moveSetType)
   {
      SkillAttribute selectedSkill = controlledMonster.GetSkillFromMoveSet(moveSetType);
      switch (moveSetType)
      {
         case MoveSetOnGrid.MoveSetType.Melee:
         case MoveSetOnGrid.MoveSetType.AOE:
         case MoveSetOnGrid.MoveSetType.Directional:
         case MoveSetOnGrid.MoveSetType.UltimateSkill:

            List<GridCoordinate> potentialTargets= selectedSkill.GetPotentialCastingPoint(moveGrid,battleMap.GetNumOfRow(),battleMap.GetNumOfCol());
            float accumulatedDamage = 0;
            foreach (var grid in potentialTargets) {
               if(!battleMap.actorsCoord.ContainsKey(grid))
                  continue;

               if (battleMap.actorsCoord[grid].turnBasedActorType == TurnBasedActorType.FriendlyControllableMonster ||
                   battleMap.actorsCoord[grid].turnBasedActorType == TurnBasedActorType.FriendlyUncontrollableMonster) {
                  accumulatedDamage += selectedSkill.Damage; 
               }
            }
            return accumulatedDamage;
         
         case MoveSetOnGrid.MoveSetType.Defense:
         case MoveSetOnGrid.MoveSetType.Heal:
            
            SkillAttribute skillMelee = controlledMonster.GetSkillFromMoveSet( MoveSetOnGrid.MoveSetType.Melee);
            SkillAttribute skillRanged = controlledMonster.GetSkillFromMoveSet( MoveSetOnGrid.MoveSetType.Directional);
            SkillAttribute skillAOE = controlledMonster.GetSkillFromMoveSet( MoveSetOnGrid.MoveSetType.AOE);
            
            float averageDamage = (skillMelee.Damage + skillRanged.Damage + skillAOE.Damage)/3;
            averageDamage *= Random.Range(0.2f, 0.5f);
            float healthPercentage = controlledMonster.currentHP / controlledMonster.maxHp;
            if (healthPercentage > 0.5) {
               averageDamage *= (1 + healthPercentage); //prefer heal when <0.5hp , prefer def when >0.5hp
            }
            return averageDamage;

      }
      return 0;
   } 
   
}
