using System.Collections;
using UnityEngine;

public enum MonsterType
{
    FRIENDLY,
    ENEMY
}

[RequireComponent(typeof(PathFindingComponent))]
public abstract class Monster : TurnBasedActor, IClickable
{
    [Header("Reference Components")]
    [SerializeField] private MonsterStatistic monsterStatistic;

    public MonsterType MonsterType { get; private set; }
    private PathFindingComponent pathFindingComponent;
    private bool enablePlayerControl = false;   //dynamically set 
    
    
    protected virtual void Awake()
    {
        //Debug.Log("Monster::Awake",this);
        pathFindingComponent = GetComponent<PathFindingComponent>();
        UpdateTurnBasedActorSpeed(monsterStatistic.GetSpeed());

    }

    protected virtual void Start()
    {
    }
//=================================================================================================================
//Public Methods
//=================================================================================================================

    public void OnClickDown()
    {
        //TODO: Show statistic of this monster 
    }

    public void OnClickRelease()
    {
        //TODO: Hide the statistic of this monster
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

    public void EnablePlayerControl()=> enablePlayerControl = true;
    
    public void SetMonsterType(MonsterType monsterType)=> MonsterType = monsterType;
    
//=================================================================================================================
//Protected Methods
//=================================================================================================================

    protected override IEnumerator StartActionsCoroutine()
    {
        Vector3 randPos = pathFindingComponent.GetRandomReachablePosition(5f);

        yield return StartCoroutine(pathFindingComponent.MoveToPositionCoroutine(randPos));
        
        Debug.Log("playing attack animation for 2sec");
        
        yield return new WaitForSeconds(2f);
        SetHasExecutedActions();  
    }
    
//=================================================================================================================
//Private Methods
//=================================================================================================================
    
}
