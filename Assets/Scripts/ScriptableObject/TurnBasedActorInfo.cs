using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TurnBasedActorInfo : ScriptableObject
{
    public abstract TurnBasedActor GetTurnBasedActorPrefab();
}
