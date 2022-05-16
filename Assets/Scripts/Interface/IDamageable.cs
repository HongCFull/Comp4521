using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    public abstract void OnDamageTaken(int damage);
    public abstract void OnDeath();
}
