using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


[RequireComponent(typeof(MonsterController))]
public class Monster : TurnBasedActor, IClickable, IDamageable
{
    [SerializeField] private MonsterInfo monsterInfo;
    [SerializeField] MonsterController monsterController;
    
    private float maxHp;
    private float currentHP;
    private float attack;
    private float defense;
    private float speed;
    private int level;
    private Animator animator;

    protected virtual void Awake()
    {
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
    public override TurnBasedActorType InitializeActorAs(TurnBasedActorType type)
    {
        if(type==TurnBasedActorType.FriendlyControllableMonster) {
            turnBasedActorCanvas.EnableHealthBarByActorType(type);
        }
        
        turnBasedActorCanvas.EnableHealthBarByActorType(type);
        return type;
    }
    
    public void InitializeAttributesByLv(int lv)
    {
        maxHp = monsterInfo.GetHealthPointAtLv(lv);
        attack = monsterInfo.GetAttackAtLv(lv);
        defense = monsterInfo.GetDefenseAtLv(lv);
        speed = monsterInfo.GetSpeedAtLv(lv);
        currentHP = maxHp;
        UpdateTurnBasedActorSpeed(speed);
    }

    public override void OnActorTurnStart()
    {
        base.OnActorTurnStart();
        
        StartCoroutine(StartActionsCoroutine());
    }
    
    public override void OnActorTurnEnd(){ }
    
    public void OnClickDown()
    {
        //TODO: Show statistic of this monster
    }

    public void OnClickRelease()
    {
        //TODO: Hide the statistic of this monster
    }
    
    public void OnDamageTaken(float damage)
    {
        currentHP -= damage;
        float UIFillPercentage = Mathf.Clamp(currentHP / maxHp, 0, 1);
        turnBasedActorCanvas.activeHealthBar.SetFillByPercentage(UIFillPercentage);
        
        if (currentHP <= 0) {
            OnDeath();
        }
    }

    public void OnDeath()
    {
        Debug.Log("Monster:"+gameObject.name+" dead");
    }

    public MonsterController GetMonsterController() => monsterController;

    public int GetMonsterMovementRange() => monsterInfo.MovementRange;

    public void PerformMoveSet(GridMoveSet moveSet)
    {
        switch (moveSet)
        {
            case GridMoveSet.MeleeAttack:
                Debug.Log("Performing SkillA");
                break;
            
            case GridMoveSet.LongRangeAttack:
                Debug.Log("Performing SkillB");
                break;
            
            case GridMoveSet.SpecialSkill:
                Debug.Log("Performing SkillC");
                break;
            
            case GridMoveSet.UltimateSkill:
                Debug.Log("Performing SkillD");
                break;
            
            case GridMoveSet.Defense:
                Debug.Log("Performing Defense");
                break;
            
            case GridMoveSet.Heal:
                Debug.Log("Performing Heal");
                break;
        }
    }
    
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
    
//=================================================================================================================
//For development 
//=================================================================================================================
    [ContextMenu("Deal 20 damage to this enemy")]
    public void Deal20DamageToThisEnemy() {
        OnDamageTaken(20f);
    }
}
