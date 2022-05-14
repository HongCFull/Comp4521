using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;


public class LevelConstructionInfoSender : MonoBehaviour
{
    [SerializeField] private LevelSetting levelSetting;
    [SerializeField] private List<UserMonsterSpawnSetting> userMonsterSpawnSettings;
    [SerializeField] private UnityEvent OnFinishSending;
    
    [ContextMenu("Send LV info to buffer")]
    public void SendLevelConstructionInfoToBuffer()
    {
        LevelConstructionInfoBuffer.Instance.AddActorSpawningInfo(levelSetting.GetMonsterSpawningInfos());
        SendUserMonsterSpawningInfoToBuffer();
        OnFinishSending.Invoke();
    }

    private void SendUserMonsterSpawningInfoToBuffer()
    {
        foreach (UserMonsterSpawnSetting spawnSetting in userMonsterSpawnSettings)
        {
            EvolutionChain monsterEvolutionChain = spawnSetting.monsterEvolutionChain;
            MonsterInfo monsterInfo = monsterEvolutionChain.GetMonsterInfoAtCurrentStage();
            
            TurnBasedActorSpawningSetting setting = TurnBasedActorSpawningSetting.ConstructTurnBasedActorSpawningSetting
            (TurnBasedActorType.FriendlyMonster, monsterInfo.GetTurnBasedActorPrefab(), spawnSetting.spawningCoord, monsterEvolutionChain.MonsterLevel);
            
            LevelConstructionInfoBuffer.Instance.AddActorSpawningInfo(setting);
        }
    }
    

}