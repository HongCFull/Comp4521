using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class MonsterAnimator : MonoBehaviour
{
    private Animator animator;
    private int meleeTriggerID;
    private int rangedTriggerID;
    private int areaTriggerID;
    private int hurtTriggerID;
    private int ultimateSkillTriggerID;
    private int buffTriggerID;
    private int movingBoolID;
    private int deadBoolID;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        meleeTriggerID = Animator.StringToHash("Melee");
        rangedTriggerID = Animator.StringToHash("Ranged");
        areaTriggerID = Animator.StringToHash("Area");
        hurtTriggerID = Animator.StringToHash("Hurt");
        ultimateSkillTriggerID = Animator.StringToHash("UltimateSkill");
        buffTriggerID = Animator.StringToHash("Buff");
        movingBoolID = Animator.StringToHash("Moving");
        deadBoolID = Animator.StringToHash("Dead");
    }

    public void SetMeleeTrigger() => animator.SetTrigger(meleeTriggerID);
    public void SetRangedTrigger()=>animator.SetTrigger(rangedTriggerID);
    public void SetAreaTrigger()=>animator.SetTrigger(areaTriggerID);
    public void SetHurtTrigger()=>animator.SetTrigger(hurtTriggerID);
    public void SetUltimateSkillTrigger() => animator.SetTrigger(ultimateSkillTriggerID);
    public void SetBuffTrigger() => animator.SetTrigger(buffTriggerID);
    public void SetMovingBool(bool isMoving)=>animator.SetBool(movingBoolID,isMoving);
    public void SetDeadBool(bool isDead)=>animator.SetBool(deadBoolID,isDead);
    

}
