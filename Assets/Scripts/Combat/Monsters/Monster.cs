using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(PathFindingComponent))]
public abstract class Monster : TurnBasedActor, IClickable
{
    [SerializeField] private MonsterStatistic monsterStatistic;
    
    private PathFindingComponent pathFindingComponent;
    private bool enablePlayerControl = false;   //dynamically set 
    private float maxHp;
    private float currentHP;
    private float attack;
    private float defense;
    private float speed;
    private int level;

    protected virtual void Awake()
    {
        pathFindingComponent = GetComponent<PathFindingComponent>();
        //TODO: Update the battle attribute of this instance according to the level
        UpdateTurnBasedActorSpeed(speed);
    }

    protected virtual void Start()
    {
    }
//=================================================================================================================
//Public Methods
//=================================================================================================================
    public void InitializeAttributesByLv(int lv)
    {
        maxHp = monsterStatistic.GetHealthPointAtLv(lv);
        attack = monsterStatistic.GetAttackAtLv(lv);
        defense = monsterStatistic.GetDefenseAtLv(lv);
        speed = monsterStatistic.GetSpeedAtLv(lv);
        currentHP = maxHp;
    }
    
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
    
    
//=================================================================================================================
//Protected Methods
//=================================================================================================================

    //EXAMPLE OF Move -> Attack -> End
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
