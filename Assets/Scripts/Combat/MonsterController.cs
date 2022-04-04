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
   /// <summary>
   /// Set to false by default. Call EnablePlayerControl to enable the player control
   /// </summary>
   [HideInInspector] public bool enablePlayerControl = false;
   [HideInInspector] public Monster monster;

   private const float RayCastDistance = 200f;
   private Camera mainCamera;
   private NavMeshAgent navMeshAgent;
   private NavMeshPath path;
   
   private Vector3 gridPosition;
   private bool isAskingPlayerInput = false;
   private bool gotAvailableGridPos = false;
   
   private void Awake() {
      path = new NavMeshPath();
      
      navMeshAgent = gameObject.GetComponent<NavMeshAgent>();
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

      //Get the player/AI input
      yield return StartCoroutine(MoveToPositionCoroutine(gridPosition));
      while (!HasReachedPosition(gridPosition)) {
         yield return null;
      }
      ResetMonsterControllerState();
   }

   void HandlePlayerScreenTouchInput()
   {
#if UNITY_EDITOR
      if (!Input.GetMouseButtonDown(0))
         return;
      
      Vector3 screenToWorldPoint = mainCamera.ScreenToWorldPoint(Input.mousePosition);
      Vector3 direction = mainCamera.transform.rotation*Vector3.forward*100f;
      Ray ray = new Ray(screenToWorldPoint, direction);
      RaycastHit hit ;
      Vector3 worldPos = Vector3.zero;
      
      Debug.DrawRay(ray.origin,ray.direction,Color.cyan,3f);

      bool hitSomeThing = Physics.Raycast(ray, out hit, RayCastDistance);
      if (!hitSomeThing)
         return;
      
      if (hit.transform.gameObject.layer.Equals(LayerMask.NameToLayer("TurnBasedGrid"))) 
         worldPos = hit.transform.position;
     
#elif UNITY_ANDROID

#endif
      
      if (IsValidGrid(worldPos)) {
         gridPosition = GetNearestGridWorldPos(worldPos);
         gotAvailableGridPos = true;
      }
   }
   
   void AskForAIMovementInput()
   {
      Vector3 randPos = GetRandomReachablePosition(6);
      gridPosition = GetNearestGridWorldPos(randPos);
   }

   IEnumerator AskForPlayerMovementInput()
   {
      isAskingPlayerInput = true;   //enable the update method to process player input 
      while (!gotAvailableGridPos) {   //Wait until the player clicked a valid grid 
         //Debug.Log("Waiting a valid pos");
         yield return null;
      }
   }
   
   IEnumerator MoveToPositionCoroutine(Vector3 position)
   {
      Debug.Log("Moving to "+position);
      if (!PositionIsReachable(position)) {
         Debug.Log("Pos not reachable!");
         yield break;
      }
      
      navMeshAgent.destination = position;
      while (!HasReachedPosition(position)) {
         yield return new WaitForSeconds(0.2f);;
      }
   }

   private bool PositionIsReachable(Vector3 position)
   {
      navMeshAgent.CalculatePath(position, path);
      if (path.status == NavMeshPathStatus.PathPartial ||
          path.status == NavMeshPathStatus.PathInvalid)
         return false;
      return true;
   }
   
   private bool HasReachedPosition(Vector3 position,float tolerance = 0.5f) => (transform.position - position).magnitude < tolerance;
   private bool HasReachedPosition(Transform destTransform,float tolerance = 0.5f) => (transform.position - destTransform.position).magnitude < tolerance;

   private bool IsValidGrid(Vector3 worldPos) => true;

   private Vector3 GetNearestGridWorldPos(Vector3 pos)
   {
      NavMeshHit hit;
      NavMesh.SamplePosition(pos, out hit, 5f, NavMesh.AllAreas);
      return hit.position;
   }

   private void ResetMonsterControllerState()
   {
      isAskingPlayerInput = false;
      gotAvailableGridPos = false;
   }
   
//NOTE: For Development Testing Only   
   public Vector3 GetRandomReachablePosition(float walkRadius)
   {
      Vector3 randomPosition = transform.position + Random.insideUnitSphere * walkRadius;
      NavMeshHit hit;
      NavMesh.SamplePosition(randomPosition, out hit, walkRadius, NavMesh.AllAreas);
      return hit.position;
   }
   
}
