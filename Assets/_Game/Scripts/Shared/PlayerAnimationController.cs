using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;
using UnityEngine.Playables;
using Kigor.Rigging;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private string[] animationStates;
    [SerializeField] private List<RiggingConstraint> riggingConstraints;
    private Transform aimTargetPoint;
    private Animator animator;

    private int currentStateIndex;
    private PlayableGraph playableGraph;

    private RigBuilder rigBuilder => this.GetComponent<RigBuilder>();

    private void Awake()
    {
        this.animator = this.GetComponent<Animator>();
        this.animator.speed = 0;
        this.animator.enabled = true;
    }


    public void ChangeAnimationState(int stateIndex)
    {
        if (this.currentStateIndex == stateIndex) return;
        this.currentStateIndex = stateIndex;
        this.animator.Play(this.animationStates[this.currentStateIndex]);
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
        this.riggingConstraints.ForEach(x => x.SolveIK());
        this.animator.speed = 0;
    }
}
