using System.Collections;
using UnityEngine;

public enum TurnBasedActorType
{
    FriendlyControllableMonster,
    FriendlyUncontrollableMonster,
    EnemyMonster,
    Other
}


public abstract class TurnBasedActor : MonoBehaviour
{
    [SerializeField] protected TurnBasedActorCanvas turnBasedActorCanvas;
    
    [HideInInspector] public TurnBasedActorType turnBasedActorType;
    [HideInInspector] public CombatManager combatManager;
    [HideInInspector] public BattleMap battleMap;
    
    public abstract TurnBasedActorType InitializeActorAs(TurnBasedActorType type);
    
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
    // The actor has to call this function to initialize the speed
    protected void UpdateTurnBasedActorSpeed(float speed) => Speed = speed;
    protected void SetHasExecutedActions() => HasExecutedActions = true;

}
