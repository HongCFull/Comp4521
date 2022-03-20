using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(PathFindingComponent))]
public abstract class Monster : TurnBasedActor, IClickable
{
    [Header("Player Control")]
    [Tooltip("Whether the player can control this actor")]
    [SerializeField] private bool enablePlayerControl = false;
    
    [Header("Reference Components")]
    [SerializeField] private MonsterStatistic monsterStatistic;
    
    private PathFindingComponent pathFindingComponent;

    protected virtual void Awake()
    {
        //Debug.Log("Monster::Awake",this);
        pathFindingComponent = GetComponent<PathFindingComponent>();
    }

    //TODO: may have to use awake as Monster will be spawned by using Instantiate<T>, not sure if necessary 
    protected virtual void Start()
    {
        //Debug.Log("Monster::Start",this);
        UpdateTurnBasedActorSpeed(monsterStatistic.GetSpeed());
        RegisterInCombatManager();
    }

    //TODO: Show statistic of this monster 
    public void OnClickDown()
    {
        
    }

    //TODO: Hide the statistic of this monster
    public void OnClickRelease()
    {
        
    }
    
    public override void OnActorTurnStart()
    {
        base.OnActorTurnStart();

        ResetHasExecutedActions();
        StartCoroutine(StartActionsCoroutine());
    }

    public override void OnActorTurnEnd()
    {
    }
    
    protected override IEnumerator StartActionsCoroutine()
    {
        Vector3 randPos = pathFindingComponent.GetRandomReachablePosition(5f);

        yield return StartCoroutine(pathFindingComponent.MoveToPositionCoroutine(randPos));
        
        Debug.Log("playing attack animation for 2sec");
        
        yield return new WaitForSeconds(2f);
        SetHasExecutedActions();  
    }
}
