using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterInfoManager : MonoBehaviour
{
    public static MonsterInfoManager Instance { get; private set; }
    
    [SerializeField] private MonsterInfo batInfo;
    [SerializeField] private MonsterInfo beeInfo;
    [SerializeField] private MonsterInfo bombInfo;
    [SerializeField] private MonsterInfo budInfo;
    [SerializeField] private MonsterInfo chickInfo;
    [SerializeField] private MonsterInfo dragonSparkInfo;
    [SerializeField] private MonsterInfo eggletInfo;
    [SerializeField] private MonsterInfo ghostInfo;
    [SerializeField] private MonsterInfo mushroomInfo;
    [SerializeField] private MonsterInfo seedInfo;
    [SerializeField] private MonsterInfo shellInfo;
    [SerializeField] private MonsterInfo snakeletInfo;
    [SerializeField] private MonsterInfo spiderInfo;
    [SerializeField] private MonsterInfo sunBlossomInfo;
    [SerializeField] private MonsterInfo targetDummyInfo;
    [SerializeField] private MonsterInfo wolfPupInfo;
    private void Awake()
    {
        if (!Instance)
            Instance = this;
    }

    public MonsterInfo GetMonsterInfoWithType(MonsterType monsterType)
    {
        switch (monsterType) {
            case MonsterType.Bat:
                return batInfo;
            case MonsterType.Bee:
                return beeInfo;
            case MonsterType.Bomb:
                return bombInfo;
            case MonsterType.Bud:
                return budInfo;
            case MonsterType.Chick:
                return chickInfo;
            case MonsterType.Egglet:
                return eggletInfo;
            case MonsterType.Ghost:
                return ghostInfo;
            case MonsterType.Mushroom:
                return mushroomInfo;
            case MonsterType.Seed:
                return seedInfo;
            case MonsterType.Shell:
                return shellInfo;
            case MonsterType.Snakelet:
                return snakeletInfo;
            case MonsterType.DragonSpark:
                return dragonSparkInfo;
            case MonsterType.SunBlossom:
                return sunBlossomInfo;
            case MonsterType.TargetDummy:
                return targetDummyInfo;
            case MonsterType.WolfPup:
                return wolfPupInfo;
        }
        return null;
    }
}
