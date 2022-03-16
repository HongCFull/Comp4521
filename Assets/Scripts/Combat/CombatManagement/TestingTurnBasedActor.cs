using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class TestingTurnBasedActor : TurnBasedActor
{
    private NavMeshAgent navMeshAgent;
    private bool enablePlayerControl = false;
    [SerializeField] private Transform destination;
    [SerializeField] private float maxActionDuration = 10f;
    [SerializeField] private MonsterStatistic monsterStatistic;
    
    private void Start()
    {
        navMeshAgent = gameObject.GetComponent<NavMeshAgent>();
        UpdateTurnBasedActorSpeed(monsterStatistic.GetSpeed());
        RegisterInCombatManager();
    }
    
    private void Update()
    {
        if (!enablePlayerControl)
            return;
        
        //if(Input.GetKeyDown(KeyCode.E))
        //    navMeshAgent.Move(destination.position);
    }
    //BUG:putting this function in base class doesn't work
    private void RegisterInCombatManager()=> CombatManager.Instance.RegisterNewTurnBasedActor(this);
    
    protected override void UpdateTurnBasedActorSpeed(float speed) => Speed = speed;

    public override void OnActorTurnStart()
    {
        Debug.Log("on "+gameObject.name+"'s turn now");
        enablePlayerControl = true;
        HasExecutedActions = false;
        StartCoroutine(ExecuteTurnBasedActions());
    }

    public override void OnActorTurnEnd()
    {
        Debug.Log("end "+gameObject.name+"'s turn now");
        enablePlayerControl = false;
    }

    public override IEnumerator ExecuteTurnBasedActions()
    {
        navMeshAgent.destination = (destination.position);

        float timer = 0f;
        while (!HasReachedPosition(destination.position)) {
            timer += Time.deltaTime;
            if (timer > maxActionDuration) 
                yield break;
            yield return null;
        }
        
        Debug.Log("playing attack animation for 2sec");
        yield return new WaitForSeconds(2f);
        HasExecutedActions = true;
    }

    bool HasReachedPosition(Vector3 pos)
    {
        float dist = (transform.position - pos).magnitude;
        Debug.Log("dist = "+dist);
        return dist<3f;
    }
}
