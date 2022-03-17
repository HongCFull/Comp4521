using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [SerializeField] private List<TurnBasedActor> turnBasedActors;
    [SerializeField] private CinemachineVirtualCamera followCamera;
    public static CombatManager Instance { get; private set; }
    
    private const float ActionTimeLimit = 20f;
    private const float TurnSmoothingTime = 0.2f;

    public bool IsBattling { get; private set; } = true;    //for testing

    private void Awake()
    {
        //TODO: Need to make sure the instance is not accessed before created
        if (!Instance)
            Instance = this;
        turnBasedActors = new List<TurnBasedActor>();
    }
    
    /// <summary>
    /// Add the input TurnBasedActor to the list in the right order 
    /// </summary>
    /// <param name="turnBasedActor">The TurnBasedActor to add</param>
    public void RegisterNewTurnBasedActor(TurnBasedActor turnBasedActor)
    {
        turnBasedActors.Add(turnBasedActor); 
        SortTurnBasedActorList();
    }

    /// <summary>
    /// Add the input TurnBasedActor to the list in the right order 
    /// </summary>
    /// <param name="turnBasedActor">The TurnBasedActor to remove from the list</param>
    public void UnregisterTurnBasedActor(TurnBasedActor turnBasedActor)=> turnBasedActors.Remove(turnBasedActor);
    
    public void StartBattle()
    {
        StartCoroutine(StartBattleGameLoop());
    }

    IEnumerator StartBattleGameLoop()
    {
        WaitForSeconds turnSmoothingTime = new WaitForSeconds(TurnSmoothingTime);
        while (IsBattling) {
            foreach (TurnBasedActor turnBasedActor in turnBasedActors) {
                if(!turnBasedActor) continue;   //Prevent accessing null ref, as it may be deleted from the list elsewhere while iterating  
                
                SetCameraFocusOnActiveActor(turnBasedActor.transform);
                yield return StartCoroutine(ProcessActor(turnBasedActor));
                yield return turnSmoothingTime;
            }
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

    void SetCameraFocusOnActiveActor(Transform actorTransform) => followCamera.Follow = actorTransform;

    /// <summary>
    /// Sort the TurnBasedActor list by speed in a descending order 
    /// </summary>
    void SortTurnBasedActorList()=> turnBasedActors.Sort((x, y) => y.Speed.CompareTo(x.Speed));

    void ClearTurnBasedActorList() => turnBasedActors.Clear();
}
