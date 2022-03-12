using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuTabTweener : MonoBehaviour
{
    [SerializeField] private ProjectResolutionSetting projectResolutionSetting;
    
    /// <summary>
    /// Move the Tabs Holder object inorder to see the corresponding tab
    /// </summary>
    /// <param name="index">The index of the tab in the main menu. 0 indicates the most left tab </param>
    public void TweenCurrentTabTo(int index) {
        LeanTween.moveLocalX(gameObject, -index * projectResolutionSetting.GetDefaultScreenWidth(),0.3f);    //Screen height is the horizontal length of the screen in landscape mode
    }
    
}
