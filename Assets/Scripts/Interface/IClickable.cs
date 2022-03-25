using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Responsible for handling the event after clicking on a non-UI game objects  
/// </summary>
public interface IClickable
{
    public abstract void OnClickDown();
    public abstract void OnClickRelease();
    //public abstract void OnDrag();
}
