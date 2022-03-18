using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TurnBasedActor : MonoBehaviour
{
    /// <summary>
    /// The speed of this turn based actor. Actor with higher speed has a higher priority to execute the action   
    /// </summary>
    public float Speed { get; private set; }
    
    /// <summary>
    /// Whether this actor has finished the actions in this turn.
    /// It should be set to true manually when the actor has finished all actions
    /// </summary>
    public bool HasExecutedActions { get; protected set;}
    
    /// <summary>
    /// Register this turn based actor in the combat manager. Should be done once an actor only
    /// </summary>
    [ContextMenu("register actor in the combat manager list")]
    protected void RegisterInCombatManager()=> CombatManager.Instance.RegisterNewTurnBasedActor(this);
    
    /// <summary>
    /// Called when the turn of this actor started
    /// </summary>
    public virtual void OnActorTurnStart()=> HasExecutedActions = false;
    
    /// <summary>
    /// Called when the turn of this actor ended 
    /// </summary>
    public virtual void OnActorTurnEnd() => HasExecutedActions = true;
    
    /// <summary>
    /// The sequential actions that this actor needs to execute
    /// </summary>
    protected abstract IEnumerator StartActionsCoroutine();

    /// <summary>
    /// The actor has to call this function to initialize the speed
    /// </summary>
    /// <param name="speed"></param>
    protected void UpdateTurnBasedActorSpeed(float speed) => Speed = speed;
}
