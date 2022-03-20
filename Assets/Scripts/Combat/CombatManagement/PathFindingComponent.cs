using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;


[RequireComponent(typeof(NavMeshAgent))]
public class PathFindingComponent : MonoBehaviour
{
   private NavMeshAgent navMeshAgent;
   private NavMeshPath path;
   private WaitForSeconds waitForSeconds;
   private void Awake() {
      navMeshAgent = gameObject.GetComponent<NavMeshAgent>();
      path = new NavMeshPath();
      waitForSeconds = new WaitForSeconds(0.2f);
   }
   
   public IEnumerator MoveToPositionCoroutine(Vector3 position)
   {
      if(!PositionIsReachable(position))
         yield break;
      
      navMeshAgent.destination = position;
      while (!HasReachedPosition(position)) {
         yield return waitForSeconds;
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

//NOTE: For Development Testing Only   
   public Vector3 GetRandomReachablePosition(float walkRadius)
   {
      Vector3 randomPosition = transform.position + Random.insideUnitSphere * walkRadius;
      NavMeshHit hit;
      NavMesh.SamplePosition(randomPosition, out hit, walkRadius,1);
      return hit.position;
   }
   
}
