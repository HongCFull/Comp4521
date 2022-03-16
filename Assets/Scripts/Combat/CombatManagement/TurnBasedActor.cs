using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TurnBasedActor : MonoBehaviour
{
    public float Speed { get; protected set; }
    protected abstract void UpdateTurnBasedActorSpeed(float speed);
    public bool HasExecutedActions { get; protected set;}
    public abstract void OnActorTurnStart();
    public abstract void OnActorTurnEnd();
    public abstract IEnumerator ExecuteTurnBasedActions();
}
