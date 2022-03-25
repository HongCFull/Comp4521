using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public struct TurnBasedActorSpawningInfo
{
    public TurnBasedActorType turnBasedActorType;
    public TurnBasedActor turnBasedActor;
    public Vector3 positionToSpawn;
    
    [Tooltip("For Monster Actor Only. Ignore it for non monster actor")]
    public int lvToSpawn;   //For Monster Only
}


public enum TurnBasedActorType
{
    FriendlyMonster,
    EnemyMonster,
    Other
}
