using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveAndLoad : MonoBehaviour
{
    [SerializeField] private EvolutionChain monsterChainToSave;
    public bool isUnlocked;
    public int thisMonsterLv;
    void StoreInfo()
    {
        string monsterNameIsUnlockedString = monsterChainToSave.name + "IsUnlocked";
        string monsterNameLevel = monsterChainToSave.name + "Lv";
        if(monsterChainToSave.IsUnlocked)
            PlayerPrefs.SetInt(monsterNameIsUnlockedString,1);
        else
            PlayerPrefs.SetInt(monsterNameIsUnlockedString,0);
        
        PlayerPrefs.SetInt(monsterNameLevel,monsterChainToSave.MonsterLevel);
    }

    void PopInfo()
    {
        string monsterNameIsUnlockedString = monsterChainToSave.name + "IsUnlocked";
        string monsterNameLevel = monsterChainToSave.name + "Lv";
        
        int isUnlockedInt = PlayerPrefs.GetInt(monsterNameIsUnlockedString);
        if (isUnlockedInt == 1)
            isUnlocked = true;
        else
            isUnlocked = false;
        thisMonsterLv = PlayerPrefs.GetInt(monsterNameLevel,0);
    }
    
    
}
