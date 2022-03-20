using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IClickable
{
    public abstract void OnClickDown();
    public abstract void OnClickRelease();
    //public abstract void OnDrag();
}
