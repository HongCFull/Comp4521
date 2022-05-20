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
    
    [Tooltip("No need to assign scene loading")]
    [SerializeField] private UnityEvent OnFinishSending;
    
    [SerializeField] private int combatSceneIndex = 2;

    [ContextMenu("Send LV info to buffer")]
    public void SendLevelConstructionInfoToBuffer()
    {
        LevelConstructionInfoBuffer.Instance.AddActorSpawningInfo(levelSetting.GetMonsterSpawningInfos());
        SendUserMonsterSpawningInfoToBuffer();
        OnFinishSending.Invoke();
        LoadSceneManager.Instance.LoadSceneWithLoadSceneBG(combatSceneIndex);
    }

    private void SendUserMonsterSpawningInfoToBuffer()
    {
        foreach (UserMonsterSpawnSetting spawnSetting in userMonsterSpawnSettings)
        {
            EvolutionChain monsterEvolutionChain = spawnSetting.monsterEvolutionChain;
            MonsterInfo monsterInfo = monsterEvolutionChain.GetMonsterInfoAtCurrentStage();
            
            TurnBasedActorSpawningSetting setting = TurnBasedActorSpawningSetting.ConstructTurnBasedActorSpawningSetting
            (TurnBasedActorType.FriendlyControllableMonster, monsterInfo.GetTurnBasedActorPrefab(), spawnSetting.spawningCoord,spawnSetting.orientationSetting , monsterEvolutionChain.MonsterLevel);
            
            LevelConstructionInfoBuffer.Instance.AddActorSpawningInfo(setting);
        }
    }
    

}