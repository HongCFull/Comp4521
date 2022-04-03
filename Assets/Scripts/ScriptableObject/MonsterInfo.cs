using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMonsterStatistic", menuName = "ScriptableObjects/NewMonsterStatistic", order = 1)]
public class MonsterInfo : TurnBasedActorInfo
{
    [Header("The Evolution Chain This Monster Belongs To")]
    [SerializeField] private EvolutionChain evolutionChain;
    [SerializeField] private bool isUnlocked;
    [SerializeField] private Monster monsterPrefab;

    [Header("ElementType")]
    [SerializeField] private ElementType elementType;
    
    [Header("Health point")]
    [SerializeField] private float baseHealthPoint;
    [SerializeField] private float hpGrowthRate;

    [Header("Attack Category")]
    [SerializeField] private float baseAttack;
    [SerializeField] private float attackGrowthRate;
    
    [Header("Defense Category")]
    [SerializeField] private float baseDefense;
    [SerializeField] private float defenseGrowthRate;

    [Header("Speed Category")]
    [SerializeField] private float baseSpeed;
    [SerializeField] private float speedGrowthRate;
    [SerializeField] private int movementRange;

    
    public bool IsUnlocked => isUnlocked;
    public ElementType ElementType => elementType;
    public int MovementRange => movementRange;
    

//TODO: GameBalance, for prototyping only:
    public float GetHealthPointAtLv(int lv) => baseHealthPoint + Mathf.Pow(GetLevelRatio(lv),hpGrowthRate);
    public float GetAttackAtLv(int lv) => baseAttack + Mathf.Pow(GetLevelRatio(lv),attackGrowthRate);
    public float GetDefenseAtLv(int lv) => baseDefense + Mathf.Pow(GetLevelRatio(lv),defenseGrowthRate);
    public float GetSpeedAtLv(int lv) => baseSpeed + Mathf.Pow(GetLevelRatio(lv),speedGrowthRate);
    private float GetLevelRatio(int lv) => lv > 1 ? Mathf.Sqrt(lv) + 1 : 1;

    public override TurnBasedActor GetTurnBasedActorPrefab() => monsterPrefab;
}
