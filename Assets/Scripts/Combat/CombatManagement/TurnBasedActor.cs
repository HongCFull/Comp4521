using System.Collections;
using UnityEngine;

public abstract class TurnBasedActor : MonoBehaviour
{
    // The speed of this turn based actor. Actor with higher speed has a higher priority to execute the action   
    public float Speed { get; private set; }
    
    // Whether this actor has finished the actions in this turn.
    // It should be set to true manually when the actor has finished all actions
    public bool HasExecutedActions { get; private set;}
    
    // Called when the turn of this actor started
    public virtual void OnActorTurnStart()=> HasExecutedActions = false;
    
    // Called when the turn of this actor ended 
    public virtual void OnActorTurnEnd() => HasExecutedActions = true;
    
    // The sequential actions that this actor needs to execute
    protected abstract IEnumerator StartActionsCoroutine();
    
    // Register this turn based actor in the combat manager. Should be done once an actor only
    [ContextMenu("register actor in the combat manager list")]
    protected void RegisterInCombatManager()=> CombatManager.Instance.RegisterNewTurnBasedActor(this);

    // The actor has to call this function to initialize the speed
    protected void UpdateTurnBasedActorSpeed(float speed) => Speed = speed;

    protected void SetHasExecutedActions() => HasExecutedActions = true;
    protected void ResetHasExecutedActions() => HasExecutedActions = false;

}
