using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLevelSetting", menuName = "ScriptableObjects/NewLevelSetting", order = 1)]
public class LevelSetting : ScriptableObject
{
    [SerializeField] private List<TurnBasedActorSpawningSetting> monsterSpawningInfos;
}
