using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ProjectResolutionSetting", menuName = "ScriptableObjects/ProjectResolutionSettingScriptableObject", order = 1)]
public class ProjectResolutionSetting : ScriptableObject
{
    [SerializeField] float defaultScreenWidth;
    [SerializeField] float defaultScreenHeight;

    public float GetDefaultScreenWidth() => defaultScreenWidth;
    public float GetDefaultScreenHeight() => defaultScreenHeight;

}
