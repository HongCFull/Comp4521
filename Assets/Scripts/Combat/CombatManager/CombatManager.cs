using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.Mathematics;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera followCamera;
    public BattleTerrain battleTerrain;
    
    public static CombatManager Instance { get; private set; }
    private const float ActionTimeLimit = 20f;  // Used to prevent the battle for stuck at an actor forever 
    private const float TurnSmoothingTime = 0.5f;
    
    private List<TurnBasedActor> registeredActors;
    private Queue<TurnBasedActor> turnOrder;

    private int allyCounts = 0;
    private int enemyCounts = 0;
    
    public bool IsBattling { get; private set; } = true;  //for testing

    private void Start()
    {
        //TODO: Need to make sure the instance is not accessed before created
        if (!Instance)
            Instance = this;
        
        registeredActors = new List<TurnBasedActor>();
        turnOrder = new Queue<TurnBasedActor>();
        InitializeLevel();
    }

//=================================================================================================================
//Public Methods
//=================================================================================================================

    public void StartBattle()
    {
        StartCoroutine(StartBattleCoroutine());
    }

//=================================================================================================================
// Private Methods
//=================================================================================================================
    
    void InitializeLevel()
    {
        ClearPreviousData();
        SpawnTurnBasedActors();
        SortRegisteredActorListBySpeed();
    }
    
    void ClearPreviousData()
    {
        allyCounts = 0;
        enemyCounts = 0;
        registeredActors.Clear();
        turnOrder.Clear();
    }
    
    void SpawnTurnBasedActors()
    {
        List<TurnBasedActorSpawningSetting> actorSpawningInfos = LevelConstructionInfoBuffer.Instance.ConsumeTurnBasedActorSpawningInfos();
        foreach (TurnBasedActorSpawningSetting actorSpawningInfo in actorSpawningInfos)
        {
            Vector3 spawnPos = battleTerrain.GetGridCenterPosByCoordinate(actorSpawningInfo.coordToSpawn);
            Quaternion lookRotation = actorSpawningInfo.orientationSetting.GetLookAtRotationByOrientation();
            TurnBasedActor turnBasedActor = Instantiate(actorSpawningInfo.turnBasedActor,spawnPos,lookRotation);
            turnBasedActor.turnBasedActorType = actorSpawningInfo.turnBasedActorType;
            
            RegisterNewTurnBasedActor(turnBasedActor);
            
            Monster monster = turnBasedActor as Monster;
            if(!monster) continue;

            TurnBasedActorType actorType = monster.InitializeActorAs(actorSpawningInfo.turnBasedActorType);
            monster.InitializeAttributesByLv(actorSpawningInfo.monsterLv);
            if (actorType  == TurnBasedActorType.FriendlyControllableMonster) {
                monster.GetMonsterController().EnablePlayerControl();
                allyCounts++;
            }else {
                enemyCounts++;
            }
        }
    }
    
    /// <summary>
    /// Add the input TurnBasedActor to the list in the right order 
    /// </summary>
    /// <param name="turnBasedActor">The TurnBasedActor to add</param>
    private void RegisterNewTurnBasedActor(TurnBasedActor turnBasedActor)
    {
        registeredActors.Add(turnBasedActor);
        SortRegisteredActorListBySpeed();
    }

    IEnumerator StartBattleCoroutine()
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
