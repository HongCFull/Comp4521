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

//TODO:
    public bool RequireUserInputForSkill(MoveSetOnGrid.MoveSetType moveSetType)
    {
        switch (moveSetType)
        {
            case MoveSetOnGrid.MoveSetType.Melee:
                return true;    //skill attribute->needInput
            case MoveSetOnGrid.MoveSetType.Directional:
                return true;
            case MoveSetOnGrid.MoveSetType.AOE:
                return true;
            case MoveSetOnGrid.MoveSetType.UltimateSkill:
                return true;
        }
        return false;
    } 

//TODO:
    public void CastSkillAtGrid(MoveSetOnGrid.MoveSetType moveSet,GridCoordinate gridCoord)
    {
        List<GridCoordinate> gridsToDamage = new List<GridCoordinate>();
        switch (moveSet)
        {
            case MoveSetOnGrid.MoveSetType.Melee:
                Debug.Log("Performing SkillA");
                break;
            
            case MoveSetOnGrid.MoveSetType.Directional:
                Debug.Log("Performing SkillB");
                break;
            
            case MoveSetOnGrid.MoveSetType.AOE:
                Debug.Log("Performing SkillC");
                break;
            
            case MoveSetOnGrid.MoveSetType.UltimateSkill:
                Debug.Log("Performing SkillD");
                break;
            
            case MoveSetOnGrid.MoveSetType.Defense:
                Debug.Log("Performing Defense");
                break;
            
            case MoveSetOnGrid.MoveSetType.Heal:
                Debug.Log("Performing Heal");
                break;
        }
    public SkillAttribute GetSkillFromMoveSet(MoveSetOnGrid.MoveSetType moveSet) {
        return monsterInfo.GetSkillAttribute(moveSet);
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
