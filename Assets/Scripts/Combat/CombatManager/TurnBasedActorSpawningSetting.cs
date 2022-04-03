using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public struct TurnBasedActorSpawningSetting
{
    public TurnBasedActorType turnBasedActorType;
    public TurnBasedActorInfo turnBasedActorInfo;
    public Vector3 positionToSpawn;
    
    [Tooltip("For Monster Actor Only. Ignore it for non monster actor")]
    public int monsterLv;   //For Monster Only

    /// <summary>
    /// Construct and return a new spawning setting according to the input   
    /// </summary>
    public static TurnBasedActorSpawningSetting ConstructTurnBasedActorSpawningSetting
    (TurnBasedActorType turnBasedActorType,TurnBasedActorInfo turnBasedActorInfo, Vector3 positionToSpawn, int monsterLv=0)
    {
        TurnBasedActorSpawningSetting setting = new TurnBasedActorSpawningSetting();
        setting.turnBasedActorType = turnBasedActorType;
        setting.turnBasedActorInfo = turnBasedActorInfo;
        setting.positionToSpawn = positionToSpawn;
        setting.monsterLv = monsterLv;
        return setting;
    }
}
