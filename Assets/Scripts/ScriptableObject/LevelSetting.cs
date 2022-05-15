using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLevelSetting", menuName = "ScriptableObjects/NewLevelSetting", order = 1)]
public class LevelSetting : ScriptableObject
{
    [SerializeField] private int chapter;
    [SerializeField] private int level;
    [SerializeField] private List<TurnBasedActorSpawningSetting> monsterSpawningInfos;
    public List<TurnBasedActorSpawningSetting> GetMonsterSpawningInfos() => monsterSpawningInfos;
    
    
}
