using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public struct TurnBasedActorSpawningSetting
{
    public TurnBasedActorType turnBasedActorType;
    public TurnBasedActor turnBasedActor;
    public GridCoordinate coordToSpawn;
    public OrientationSetting orientationSetting;
    
    [Tooltip("For Monster Actor Only. Ignore it for non monster actor")]
    public int monsterLv;   //For Monster Only

    /// <summary>
    /// Construct and return a new spawning setting according to the input   
    /// </summary>
    public static TurnBasedActorSpawningSetting ConstructTurnBasedActorSpawningSetting
    (TurnBasedActorType turnBasedActorType,TurnBasedActor turnBasedActor, GridCoordinate gridCoord,OrientationSetting orientation, int monsterLv=0)
    {
        TurnBasedActorSpawningSetting setting = new TurnBasedActorSpawningSetting();
        setting.turnBasedActorType = turnBasedActorType;
        setting.turnBasedActor = turnBasedActor;
        setting.coordToSpawn = gridCoord;
        setting.orientationSetting = orientation;
        setting.monsterLv = monsterLv;
        return setting;
    }
}


[Serializable]
public struct UserMonsterSpawnSetting
{
    public EvolutionChain monsterEvolutionChain;
    public GridCoordinate spawningCoord;
    public OrientationSetting orientationSetting;

}


[Serializable]
public struct OrientationSetting
{
    public enum Orientation
    {
        PositiveX,
        PositiveZ,
        NegativeX,
        NegativeZ
    }

    public Orientation orientation;
    public Quaternion GetLookAtRotationByOrientation()
    {
        switch (orientation)
        {
            case Orientation.PositiveX:
                return Quaternion.LookRotation(Vector3.right);
            case Orientation.PositiveZ:
                return Quaternion.LookRotation(Vector3.forward);
            case Orientation.NegativeX:
                return Quaternion.LookRotation(Vector3.left);
            case Orientation.NegativeZ:
                return Quaternion.LookRotation(Vector3.back);
        }
        return Quaternion.LookRotation(Vector3.forward);
    }
}
