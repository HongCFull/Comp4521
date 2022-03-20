using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [SerializeField] private List<TurnBasedActor> registeredActors;
    [SerializeField] private Queue<TurnBasedActor> turnOrder;
    [SerializeField] private CinemachineVirtualCamera followCamera;
    
    public static CombatManager Instance { get; private set; }
    
    private const float ActionTimeLimit = 20f;  // Used to prevent the battle for stuck at an actor forever 
    private const float TurnSmoothingTime = 0.2f;

    public bool IsBattling { get; private set; } = true;    //for testing

    private void Awake()
    {
        //TODO: Need to make sure the instance is not accessed before created
        if (!Instance)
            Instance = this;
        
        registeredActors = new List<TurnBasedActor>();
        turnOrder = new Queue<TurnBasedActor>();
    }
    
//=================================================================================================================
//Public Methods
//=================================================================================================================

    public void InitializeLevel()
    {
        
    }
    
    public void StartBattle()=> StartCoroutine(StartBattleCoroutine());
    
    /// <summary>
    /// Add the input TurnBasedActor to the list in the right order 
    /// </summary>
    /// <param name="turnBasedActor">The TurnBasedActor to add</param>
    public void RegisterNewTurnBasedActor(TurnBasedActor turnBasedActor)
    {
        registeredActors.Add(turnBasedActor); 
        SortRegisteredActorListBySpeed();
    }
    
    
//=================================================================================================================
// Private Methods
//=================================================================================================================
    IEnumerator  StartBattleCoroutine()
    {
        while (IsBattling) {
            AssignActorsToTurnProcessingQueue();
            yield return StartCoroutine(ProcessTheTurnQueueCoroutine());
        }
    }

    IEnumerator ProcessTheTurnQueueCoroutine()
    {
        WaitForSeconds turnSmoothingTime = new WaitForSeconds(TurnSmoothingTime);
        while (turnOrder.Count>0) {
            TurnBasedActor actor = turnOrder.Dequeue();
            if(!actor)  continue;   //the actor can be destroyed before it acts in this turn
           
            SetCameraFocusOnActiveActor(actor.transform);
            yield return StartCoroutine(ProcessTurnBasedActorCoroutine(actor));
            yield return turnSmoothingTime;
        }
    }

    IEnumerator ProcessTurnBasedActorCoroutine(TurnBasedActor turnBasedActor)
    {
        if(!turnBasedActor)
            yield break;
        
        turnBasedActor.OnActorTurnStart();

        float timer = 0f;
        while (!turnBasedActor.HasExecutedActions) {
            yield return null;
            timer += Time.deltaTime;
            if(timer>=ActionTimeLimit)
                yield break;
        }
        turnBasedActor.OnActorTurnEnd();
    }

    void AssignActorsToTurnProcessingQueue()
    {
        foreach (TurnBasedActor actor in registeredActors) {
            turnOrder.Enqueue(actor);
        }
    }
    
    void SetCameraFocusOnActiveActor(Transform actorTransform) => followCamera.Follow = actorTransform;
    
    void SortRegisteredActorListBySpeed()=> registeredActors.Sort((x, y) => y.Speed.CompareTo(x.Speed));

    void ClearTurnBasedActorList() => registeredActors.Clear();
}
