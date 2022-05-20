using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMonsterStatistic", menuName = "ScriptableObjects/NewMonsterStatistic", order = 1)]
public class MonsterInfo : ScriptableObject
{
    [Header("The Evolution Chain This Monster Belongs To")]
    [SerializeField] private EvolutionChain evolutionChain;
    [SerializeField] private bool isUnlocked;
    [SerializeField] private Monster monsterPrefab;
    [SerializeField] private Sprite monsterPreviewImage;

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
    
    [Header("Range Category")]
    [SerializeField] private int movementRange;

    [Header("Skills Attributes")]
    [SerializeField] private SkillAttribute skillMelee;
    [SerializeField] private SkillAttribute skillRanged;
    [SerializeField] private SkillAttribute skillArea;
    [SerializeField] private SkillAttribute skillUltimate;
    [SerializeField] private SkillAttribute skillDefense;
    [SerializeField] private SkillAttribute skillHeal;

    public ElementType ElementType => elementType;
    public int MovementRange => movementRange;

    public bool IsUnlocked => evolutionChain.IsUnlocked;
//TODO: GameBalance, for prototyping only:
    public float GetHealthPointAtLv(int lv) => baseHealthPoint + Mathf.Pow(GetLevelRatio(lv),hpGrowthRate);
    public float GetAttackAtLv(int lv) => baseAttack + Mathf.Pow(GetLevelRatio(lv),attackGrowthRate);
    public float GetDefenseAtLv(int lv) => baseDefense + Mathf.Pow(GetLevelRatio(lv),defenseGrowthRate);
    public float GetSpeedAtLv(int lv) => baseSpeed + Mathf.Pow(GetLevelRatio(lv),speedGrowthRate);
    private float GetLevelRatio(int lv) => lv > 1 ? Mathf.Sqrt(lv) + 1 : 1;

    public SkillAttribute GetSkillAttribute(MoveSetOnGrid.MoveSetType moveSet) {
        switch (moveSet) {
            case MoveSetOnGrid.MoveSetType.Melee:
                return(skillMelee);

            case MoveSetOnGrid.MoveSetType.Directional:
                return(skillRanged);
            
            case MoveSetOnGrid.MoveSetType.AOE:
                return(skillArea);
            
            case MoveSetOnGrid.MoveSetType.UltimateSkill:
                return(skillUltimate);
            
            case MoveSetOnGrid.MoveSetType.Defense:
                return(skillDefense);
            
            case MoveSetOnGrid.MoveSetType.Heal:
                return(skillHeal);
        }
        return null;
    }
    public TurnBasedActor GetTurnBasedActorPrefab() => monsterPrefab;

    public Sprite GetPreviewImage() => monsterPreviewImage;
}
