using System;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "newBattleTerrainSetting",menuName = "ScriptableObjects/CreateBattleTerrainSetting")]
public class BattleTerrainSetting : ScriptableObject
{
    [SerializeField] private string terrainName;
    [SerializeField] public BattleTerrain battleTerrain;
    [SerializeField] private Vector3 terrainSpawnPoint;

    public string GetTerrainName() => terrainName;
    public BattleTerrain GetBattleTerrain() => battleTerrain;
    public Vector3 GetTerrainSpawnPoint() => terrainSpawnPoint;
}
