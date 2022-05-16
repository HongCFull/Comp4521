using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class MonsterInventoryController : MonoBehaviour
{
    [SerializeField] private GridLayoutGroup monsterInventoryLayoutGroup;
    [SerializeField] private GameObject monsterInventoryPrefab;
    [SerializeField] private MonsterInfoManager monsterInfoManager;
    [SerializeField] private GameObject previewObjectPannel;
    [SerializeField] private GameObject previewTextPannel;
    [SerializeField] private GameObject previewTitlePannel;
    [SerializeField] private GameObject batAnimation;

    void Start()
    {
        refreshInventory();
    }

    private void refreshInventory()
    {
        GameObject temp;
        foreach (MonsterType monsterType in Enum.GetValues(typeof(MonsterType)))
        {
            temp = Instantiate(monsterInventoryPrefab, monsterInventoryLayoutGroup.transform);
            MonsterInfo monsterInfo = monsterInfoManager.GetMonsterInfoWithType(monsterType);
            //set image
            temp.transform.GetChild(0).GetComponent<Button>().image.sprite = monsterInfo.GetPreviewImage();
            if(!monsterInfo.IsUnlocked)
            {
                temp.transform.GetComponent<Image>().color = new Color32(162,162,162,255);
                temp.transform.GetChild(0).GetComponent<Button>().image.color = new Color32(162,162,162,255);
            }
            //set name
            temp.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = monsterType.ToString();
            //set preview listener
            MonsterType tempType = monsterType;
            temp.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() => { createPreview(monsterInfo, tempType); });
            
        }


    }

    private void createPreview(MonsterInfo monsterInfo, MonsterType monsterType)
    {
        if(previewObjectPannel.transform.childCount > 0)
        {
            Destroy(previewObjectPannel.transform.GetChild(0).gameObject);
        }
        GameObject monsterPreview = Instantiate(getAnimation(monsterType) , previewObjectPannel.transform);
        //set monster name
        previewTitlePannel.transform.GetComponent<TextMeshProUGUI>().text = monsterType.ToString();
        //set offset of each monster
        monsterPreview.transform.localPosition = new Vector3(0.0f, previewOffset(monsterType), 0.0f);
        //set element text
        previewTextPannel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = monsterInfo.ElementType.ToString();
        //set attack text
        previewTextPannel.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = monsterInfo.GetAttackAtLv(0).ToString();
        //set defence text
        previewTextPannel.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = monsterInfo.GetDefenseAtLv(0).ToString();
        //set speed text
        previewTextPannel.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = monsterInfo.GetSpeedAtLv(0).ToString();
        //set attack range text
        previewTextPannel.transform.GetChild(4).GetComponent<TextMeshProUGUI>().text = monsterInfo.MovementRange.ToString();
    }   

    private float previewOffset(MonsterType monsterType)
    {
        switch(monsterType)
        {
            case MonsterType.Bat: return 0.15f;
            case MonsterType.Bee: return 0.3f;
            case MonsterType.Bomb: return 0.1f;
            case MonsterType.Bud: return 0.76f;
            case MonsterType.Chick: return 0.8f;
            case MonsterType.DragonSpark: return 0.45f;
            case MonsterType.Egglet: return 0.5f;
            case MonsterType.Ghost: return 0.23f;
            case MonsterType.Mushroom: return 0.75f;
            case MonsterType.Seed: return 0.8f;
            case MonsterType.Shell: return 0.75f;
            case MonsterType.Snakelet: return 0.7f;
            case MonsterType.SunBlossom: return 0.8f;
            case MonsterType.TargetDummy: return 0.3f;
            case MonsterType.WolfPup: return 0.5f;
        }
        return 0;
    }

    private GameObject getAnimation(MonsterType monsterType)
    {
        switch(monsterType)
        {
            case MonsterType.Bat: return batAnimation;
            /*
            case MonsterType.Bee: return 0.3f;
            case MonsterType.Bomb: return 0.1f;
            case MonsterType.Bud: return 0.76f;
            case MonsterType.Chick: return 0.8f;
            case MonsterType.DragonSpark: return 0.45f;
            case MonsterType.Egglet: return 0.5f;
            case MonsterType.Ghost: return 0.23f;
            case MonsterType.Mushroom: return 0.75f;
            case MonsterType.Seed: return 0.8f;
            case MonsterType.Shell: return 0.75f;
            case MonsterType.Snakelet: return 0.7f;
            case MonsterType.SunBlossom: return 0.8f;
            case MonsterType.TargetDummy: return 0.3f;
            case MonsterType.WolfPup: return 0.5f;
            */
        }
        return null;
    }
}
