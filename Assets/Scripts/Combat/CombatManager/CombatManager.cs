using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }
    private CombatManagerState state; 
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        state.OnStateUpdate();
    }
    
    
}
