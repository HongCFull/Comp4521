using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelConstructionBuffer : MonoBehaviour
{
    public static LevelConstructionBuffer Instance { get; private set; }
    private List<TurnBasedActorSpawningInfo> actorSpawningInfos = new List<TurnBasedActorSpawningInfo>();
    
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
    public List<TurnBasedActorSpawningInfo> PopTurnBasedActorSpawningInfos()
    {
        List<TurnBasedActorSpawningInfo> temp = new List<TurnBasedActorSpawningInfo>(actorSpawningInfos);
        actorSpawningInfos.Clear();
        return temp;
    }
    
    public void AddActorSpawningInfo(TurnBasedActorSpawningInfo info) => actorSpawningInfos.Add(info);
    public void AddActorSpawningInfo(List<TurnBasedActorSpawningInfo> infos) => actorSpawningInfos.AddRange(infos);

}
