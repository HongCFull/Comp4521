using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//TODO: Maybe it would be better if it is changed to a scriptableObjectSingleton ?
public class LevelConstructionInfoBuffer : MonoBehaviour
{
    public static LevelConstructionInfoBuffer Instance { get; private set; }
    private List<TurnBasedActorSpawningSetting> actorSpawningInfos = new List<TurnBasedActorSpawningSetting>();
    public BattleTerrainSetting battleTerrainSettingInfo { get; private set;}
    
    void Start()
    {
        if (Instance) {
            Destroy(this);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this);
    }

    /// <summary>
    ///  Get the actorSpawningInfos and clear the memory after accessing 
    /// </summary>
    /// <returns> a list of spawning information of the actor  </returns>
    public List<TurnBasedActorSpawningSetting> ConsumeTurnBasedActorSpawningInfos()
    {
        List<TurnBasedActorSpawningSetting> temp = new List<TurnBasedActorSpawningSetting>(actorSpawningInfos);
        actorSpawningInfos.Clear();
        return temp;
    }
    public void AssignBattleTerrainSetting(BattleTerrainSetting terrainSetting) => battleTerrainSettingInfo=terrainSetting;
    public void AddActorSpawningInfo(TurnBasedActorSpawningSetting setting) => actorSpawningInfos.Add(setting);
    public void AddActorSpawningInfo(List<TurnBasedActorSpawningSetting> infos) => actorSpawningInfos.AddRange(infos);

}
