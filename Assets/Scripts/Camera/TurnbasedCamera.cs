using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class TurnbasedCamera : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    
    
    public CinemachineVirtualCamera GetTurnBasedVirtualCamera() => virtualCamera;
}
