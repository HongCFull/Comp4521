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
    
    public float maxHp {get; private set;}
    public float currentHP {get; private set;}
    public float attack {get; private set;}
    public float defense {get; private set;}
    public float speed {get; private set;}
    public int movementRange {get; private set;}
    public ElementType elementType {get; private set;}
    public int level {get; private set;}
    
    private MonsterAnimator monsterAnimator;
    private List<GridCoordinate> cachedGridsToDamage = new List<GridCoordinate>();
    private SkillAttribute cachedSkill;
    protected virtual void Awake()
    {
        //TODO: Update the battle attribute of this instance according to the level
        UpdateTurnBasedActorSpeed(speed);
        monsterAnimator = GetComponent<MonsterAnimator>();
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
            monsterController.EnablePlayerControl();
        }
        
        turnBasedActorCanvas.EnableHealthBarByActorType(type);
        return type;
    }
    
    public void InitializeAttributesByLv(int lv)
    {
        maxHp = monsterInfo.GetHealthPointAtLv(lv);
        currentHP = maxHp;
        attack = monsterInfo.GetAttackAtLv(lv);
        defense = monsterInfo.GetDefenseAtLv(lv);
        speed = monsterInfo.GetSpeedAtLv(lv);

        movementRange = monsterInfo.MovementRange;
        elementType = monsterInfo.ElementType;

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

    public void HealByPercentage(float percentage)
    {
        float hpToHeal = maxHp * percentage/100;
        currentHP = Mathf.Clamp(currentHP + hpToHeal, 0, maxHp);
        turnBasedActorCanvas.activeHealthBar.SetFillByPercentage(currentHP/maxHp);
    }
    
    public void OnDamageTaken(float damage)
    {
        currentHP -= damage;
        float UIFillPercentage = Mathf.Clamp(currentHP / maxHp, 0, 1);
        turnBasedActorCanvas.activeHealthBar.SetFillByPercentage(UIFillPercentage);
        Debug.Log(name+" taken dmg "+damage);
        if (currentHP <= 0) {
            OnDeath();
            return;
        }
        monsterAnimator.SetHurtTrigger();
    }

    public void OnDeath()
    {
        Debug.Log("Monster:"+gameObject.name+" dead");
        monsterAnimator.SetDeadBool(true);
        combatManager.ReportDeath(this);
        Destroy(this.gameObject,2f);
    }

    public MonsterController GetMonsterController() => monsterController;

    public int GetMonsterMovementRange() => monsterInfo.MovementRange;

    public bool RequireUserInputForSkill(MoveSetOnGrid.MoveSetType moveSetType)
    {
        switch (moveSetType)
        {
            case MoveSetOnGrid.MoveSetType.Melee:
            case MoveSetOnGrid.MoveSetType.Directional:
            case MoveSetOnGrid.MoveSetType.AOE:
            case MoveSetOnGrid.MoveSetType.UltimateSkill:
                return monsterInfo.GetSkillAttribute(moveSetType).RequireUserInput;
        }
        return false;
    } 

//TODO:
    public void PerformSkillToGrids(MoveSetOnGrid.MoveSetType moveSet, List<GridCoordinate> gridsToCastSkill)
    {
        Debug.Log("cached grids to damage : ");
        cachedGridsToDamage = new List<GridCoordinate>(gridsToCastSkill);
        foreach (var VARIABLE in cachedGridsToDamage)
        {
            Debug.Log("recorded "+VARIABLE+" to damage");
        }
        cachedSkill = GetSkillFromMoveSet(moveSet);
        switch (moveSet)
        {
            case MoveSetOnGrid.MoveSetType.Melee:
                Debug.Log("Melee");
                monsterAnimator.SetMeleeTrigger();
                battleMap.DealDamageToGrids(cachedGridsToDamage, cachedSkill.Damage);                break;

            case MoveSetOnGrid.MoveSetType.Directional:
                Debug.Log("Directional");
                monsterAnimator.SetRangedTrigger();
                battleMap.DealDamageToGrids(cachedGridsToDamage, cachedSkill.Damage);                break;

            case MoveSetOnGrid.MoveSetType.AOE:
                Debug.Log("AOE");
                monsterAnimator.SetAreaTrigger();
                battleMap.DealDamageToGrids(cachedGridsToDamage, cachedSkill.Damage);                break;

            case MoveSetOnGrid.MoveSetType.UltimateSkill:
                Debug.Log("ULT");
                monsterAnimator.SetUltimateSkillTrigger();
                battleMap.DealDamageToGrids(cachedGridsToDamage, cachedSkill.Damage);                break;

            case MoveSetOnGrid.MoveSetType.Defense:
                Debug.Log("Def");
                monsterAnimator.SetBuffTrigger();
                break;

            case MoveSetOnGrid.MoveSetType.Heal:
                Debug.Log("Heal");
                HealByPercentage(25f);
                monsterAnimator.SetBuffTrigger();
                break;
        }
    }

    public SkillAttribute GetSkillFromMoveSet(MoveSetOnGrid.MoveSetType moveSet)=> monsterInfo.GetSkillAttribute(moveSet);

    /// <summary>
    /// Called by the animation event
    /// </summary>
    public void DealDamageCachedGrids()
    {
        //BattleMap.Instance.DealDamageToGrids(cachedGridsToDamage, cachedSkill.Damage);
    }
//=================================================================================================================
//Protected Methods
//=================================================================================================================
    
    //EXAMPLE OF Move -> Attack -> End
    protected override IEnumerator StartActionsCoroutine()
    {
        yield return StartCoroutine(monsterController.ProcessMonsterMovementCoroutine());

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
