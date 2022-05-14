using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public struct TurnBasedActorSpawningSetting
{
    public TurnBasedActorType turnBasedActorType;
    public TurnBasedActor turnBasedActor;
    public TileCoordinate coordToSpawn;
    
    [Tooltip("For Monster Actor Only. Ignore it for non monster actor")]
    public int monsterLv;   //For Monster Only

    /// <summary>
    /// Construct and return a new spawning setting according to the input   
    /// </summary>
    public static TurnBasedActorSpawningSetting ConstructTurnBasedActorSpawningSetting
    (TurnBasedActorType turnBasedActorType,TurnBasedActor turnBasedActor, TileCoordinate tileCoord, int monsterLv=0)
    {
        TurnBasedActorSpawningSetting setting = new TurnBasedActorSpawningSetting();
        setting.turnBasedActorType = turnBasedActorType;
        setting.turnBasedActor = turnBasedActor;
        setting.coordToSpawn = tileCoord;
        setting.monsterLv = monsterLv;
        return setting;
    }
}


[Serializable]
public struct UserMonsterSpawnSetting
{
    public EvolutionChain monsterEvolutionChain;
    public TileCoordinate spawningCoord;
}

