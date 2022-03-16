using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMonsterStatistic", menuName = "ScriptableObjects/NewMonsterStatistic", order = 1)]
public class MonsterStatistic : ScriptableObject
{
    [SerializeField] private ElementType elementType;
    [SerializeField] private float maxHealthPoint;
    [SerializeField] private float attack;
    [SerializeField] private float defense;
    [SerializeField] private int movementRange;
    [SerializeField] private int level;
    [SerializeField] private float experience;
    [SerializeField] private float speed;

    public ElementType GetElementType() => elementType;
    public float GetMaxHealthPoint() => maxHealthPoint;
    public float GetAttack() => attack;
    public float GetDefense() => defense;
    public int GetMovementRange() => movementRange;
    public float GetSpeed() => speed;
    public int GetMonsterLevel() => level;
    public float GetMonsterExperience() => experience;
    public float GainExperience(float exp) => experience += exp;
}
