using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class previewAnimationControl : MonoBehaviour
{
    [SerializeField] private GameObject BeePreview;
    [SerializeField] private GameObject BatPreview;
    [SerializeField] private GameObject BombPreview;
    [SerializeField] private GameObject BudPreview;
    [SerializeField] private GameObject ChickPreview;
    [SerializeField] private GameObject DragonSparkPreview;
    [SerializeField] private GameObject EggletPreview;
    [SerializeField] private GameObject GhostPreview;
    [SerializeField] private GameObject MushroomPreview;
    [SerializeField] private GameObject SeedPreview;
    [SerializeField] private GameObject ShellPreview;
    [SerializeField] private GameObject SnakeletPreview;
    [SerializeField] private GameObject SunBlossomPreview;
    [SerializeField] private GameObject TargetDummyPreview;
    [SerializeField] private GameObject WolfPupPreview;

    public void activatePreview(MonsterType monsterType)
    {
        resetPreview();

        switch(monsterType)
        {
            case MonsterType.Bat: BatPreview.SetActive(true); break;
            case MonsterType.Bee: BeePreview.SetActive(true); break;
            case MonsterType.Bomb: BombPreview.SetActive(true); break;
            case MonsterType.Bud: BudPreview.SetActive(true); break;
            case MonsterType.Chick: ChickPreview.SetActive(true); break;
            case MonsterType.Egglet: EggletPreview.SetActive(true); break;
            case MonsterType.Ghost: GhostPreview.SetActive(true); break;
            case MonsterType.Mushroom: MushroomPreview.SetActive(true); break;
            case MonsterType.Seed: SeedPreview.SetActive(true); break;
            case MonsterType.Shell: ShellPreview.SetActive(true); break;
            case MonsterType.Snakelet: SnakeletPreview.SetActive(true); break;
            case MonsterType.DragonSpark: DragonSparkPreview.SetActive(true); break;
            case MonsterType.SunBlossom: SunBlossomPreview.SetActive(true); break;
            case MonsterType.TargetDummy: TargetDummyPreview.SetActive(true); break;
            case MonsterType.WolfPup: WolfPupPreview.SetActive(true); break;
        }
    }

    public void resetPreview()
    {
        BatPreview.SetActive(false);
        BeePreview.SetActive(false);
        BombPreview.SetActive(false);
        BudPreview.SetActive(false);
        ChickPreview.SetActive(false);
        EggletPreview.SetActive(false);
        GhostPreview.SetActive(false);
        MushroomPreview.SetActive(false);
        SeedPreview.SetActive(false);
        ShellPreview.SetActive(false);
        SnakeletPreview.SetActive(false);
        DragonSparkPreview.SetActive(false);
        SunBlossomPreview.SetActive(false);
        TargetDummyPreview.SetActive(false);
        WolfPupPreview.SetActive(false);
    }
}
