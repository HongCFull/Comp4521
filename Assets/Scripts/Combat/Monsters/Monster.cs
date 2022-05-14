using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


[RequireComponent(typeof(MonsterController))]
public class Monster : TurnBasedActor, IClickable
{
    [SerializeField] private MonsterInfo monsterInfo;
    public MonsterController monsterController;
    private float maxHp;
    private float currentHP;
    private float attack;
    private float defense;
    private float speed;
    private int level;
    private Animator animator;

    protected virtual void Awake()
    {
        monsterController.monster = this;
        
        //TODO: Update the battle attribute of this instance according to the level
        UpdateTurnBasedActorSpeed(speed);
        animator = GetComponent<Animator>();
    }

    protected virtual void Start()
    {
    }
    
//=================================================================================================================
//Public Methods
//=================================================================================================================
    public void InitializeAttributesByLv(int lv)
    {
        maxHp = monsterInfo.GetHealthPointAtLv(lv);
        attack = monsterInfo.GetAttackAtLv(lv);
        defense = monsterInfo.GetDefenseAtLv(lv);
        speed = monsterInfo.GetSpeedAtLv(lv);
        currentHP = maxHp;
        UpdateTurnBasedActorSpeed(speed);
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
        
        StartCoroutine(StartActionsCoroutine());
    }
    
    public override void OnActorTurnEnd()
    {
    }

    public MonsterController GetMonsterController() => monsterController;
    
    
//=================================================================================================================
//Protected Methods
//=================================================================================================================
    
    //EXAMPLE OF Move -> Attack -> End
    protected override IEnumerator StartActionsCoroutine()
    {
        yield return StartCoroutine(monsterController.ProcessMonsterMovementCoroutine());
        
        Debug.Log("playing attack animation for 2sec");
        animator.SetTrigger("Melee");
        
        yield return new WaitForSeconds(2f);
        SetHasExecutedActions();  
    }
    
//=================================================================================================================
//Private Methods
//=================================================================================================================
    
}
