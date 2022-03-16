using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [SerializeField] private List<TurnBasedActor> turnBasedActors;
    public static CombatManager Instance { get; private set; }
    private const float ActionTimeLimit = 20f;
    private const float TurnSmoothingTime = 0.2f;

    private void Awake()
    {
        //TODO: Need to make sure the instance is not accessed before created
        if (!Instance)
            Instance = this;
        turnBasedActors = new List<TurnBasedActor>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            HandleActorsInThisTurn();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="turnBasedActor"></param>
    public void RegisterNewTurnBasedActor(TurnBasedActor turnBasedActor)=> turnBasedActors.Add(turnBasedActor);

    public void HandleActorsInThisTurn()
    {
        turnBasedActors.Sort((x, y) => y.Speed.CompareTo(x.Speed)); //descending sort according to speed 
        StartCoroutine(ProcessAllActorsOnQueue());
    }

    IEnumerator ProcessAllActorsOnQueue()
    {
        WaitForSeconds turnSmoothingTime = new WaitForSeconds(TurnSmoothingTime);
        foreach (TurnBasedActor turnBasedActor in turnBasedActors) {
            yield return StartCoroutine(ProcessActor(turnBasedActor));
            yield return turnSmoothingTime;
            //turnBasedActors.Remove(turnBasedActor);
        }
    }

    IEnumerator ProcessActor(TurnBasedActor turnBasedActor)
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


}
