using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Kigor.Rigging;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private string[] animationStates;
    [SerializeField] private AimConstraint rightHandConstraint;
    [SerializeField] private CustomTwoBonesIK leftHandConstraint;
    private Transform aimTargetPoint;
    private Animator animator;

    private int currentStateIndex;
    private PlayableGraph playableGraph;


    private void Awake()
    {
        this.animator = this.GetComponent<Animator>();
        this.animator.speed = 0;
        this.animator.enabled = true;
    }

    private void LateUpdate()
    {
        // var directionIndicator = this.GetComponent<PlayerAvatar>().DirectionIndicator;
        // var targetPos = directionIndicator.position + directionIndicator.forward * 1000;
    }

    public void ChangeAnimationState(int stateIndex, float normalizedTime = 0)
    {
        this.animator.speed = 1;
        if (this.currentStateIndex == stateIndex) return;
        this.currentStateIndex = stateIndex;
        this.animator.Play(this.animationStates[this.currentStateIndex], -1, normalizedTime);
        this.animator.speed = 0;
    }

    public float GetCurrentStateTime()
    {
        var stateInfo = this.animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.normalizedTime;
    }

    public void UpdateAnimation(float deltaTime)
    {
        this.animator.speed = 1;
        this.animator.Update(deltaTime);

        // this.rightHandConstraint.SolveIK();
        // this.leftHandConstraint.SolveIK();
        
        this.animator.speed = 0;
    }

    public void ManuallyUpdateIK()
    {
        this.rightHandConstraint.SolveIK();
        this.leftHandConstraint.SolveIK();
    }
    
}
