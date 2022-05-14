using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class LevelConstructionInfoSender : MonoBehaviour
{
    [SerializeField] private BattleTerrainSetting battleTerrainSettingInfo;
    [SerializeField] private List<TurnBasedActorSpawningSetting> enemyActorSpawningInfos;
    [SerializeField] private List<EvolutionChain> userMonsterChain;
    [SerializeField] private UnityEvent OnFinishSending;
    
    [ContextMenu("Send LV info to buffer")]
    public void SendLevelConstructionInfoToBuffer()
    {
        LevelConstructionInfoBuffer.Instance.AddActorSpawningInfo(enemyActorSpawningInfos);
        LevelConstructionInfoBuffer.Instance.AssignBattleTerrainSetting(battleTerrainSettingInfo);
        
        SendUserMonsterSpawningInfoToBuffer();
        OnFinishSending.Invoke();
    }

    private void SendUserMonsterSpawningInfoToBuffer()
    {
        foreach (EvolutionChain evolutionChain in userMonsterChain) {
            MonsterInfo monsterInfo = evolutionChain.GetMonsterInfoAtCurrentStage();
            Vector3 spawnPos = new Vector3(Random.Range(0, 10f), 0, Random.Range(0, 10f));
            TurnBasedActorSpawningSetting setting =TurnBasedActorSpawningSetting.ConstructTurnBasedActorSpawningSetting(
                TurnBasedActorType.FriendlyMonster, monsterInfo, spawnPos, evolutionChain.MonsterLevel);
            
            LevelConstructionInfoBuffer.Instance.AddActorSpawningInfo(setting);
        }
    }
    

}