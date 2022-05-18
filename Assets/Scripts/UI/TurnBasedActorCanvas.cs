using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms.DataVisualization.Charting;
using UnityEngine;

public class TurnBasedActorCanvas : MonoBehaviour
{
    [SerializeField] private HealthBar friendlyHealthBar;
    [SerializeField] private HealthBar adversarialHealthBar;
    public HealthBar activeHealthBar { get; private set; } = null;
    
    //Can only activate once 
    public void EnableHealthBarByActorType(TurnBasedActorType type)
    {
        if (activeHealthBar)
            return;
        
        switch (type)
        {
            case TurnBasedActorType.FriendlyControllableMonster:
            case TurnBasedActorType.FriendlyUncontrollableMonster:
                friendlyHealthBar.gameObject.SetActive(true);
                activeHealthBar = friendlyHealthBar;
                break;

            case TurnBasedActorType.EnemyMonster:
                adversarialHealthBar.gameObject.SetActive(true);
                activeHealthBar = adversarialHealthBar;
                break;
            
            case TurnBasedActorType.Other:
                break;
        }
    }
}
