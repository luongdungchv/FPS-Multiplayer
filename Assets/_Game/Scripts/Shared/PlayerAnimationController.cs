using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;
using UnityEngine.Playables;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private string[] animationStates;
    [SerializeField] private MultiAimConstraint rightHandAim, chestAim;
    private Transform aimTargetPoint;
    private Animator animator;

    private int currentStateIndex;
    private PlayableGraph playableGraph;

    private RigBuilder rigBuilder => this.GetComponent<RigBuilder>();

    private void Awake()
    {
        this.animator = this.GetComponent<Animator>();
        this.animator.speed = 0;

        var aimTargetObj = new GameObject("AimTarget");
        this.aimTargetPoint = aimTargetObj.transform;
        
        var weightedPoint = new WeightedTransform(this.aimTargetPoint, 1);
        WeightedTransformArray sources = new WeightedTransformArray();
        
        sources.Add(weightedPoint);
        this.rightHandAim.data.sourceObjects = sources;
        this.chestAim.data.sourceObjects = sources;
        this.rigBuilder.Build();

        this.rigBuilder.graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
        this.rigBuilder.enabled = false;
    }

    private void OnEnable()
    {
        // this.rigBuilder.enabled = false;
        // playableGraph = PlayableGraph.Create(gameObject.name + ".HumanAnim");
        // playableGraph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
        //
        // // Create main playable
        // var mainPlayable = AnimatorControllerPlayable.Create(playableGraph, this.animator.runtimeAnimatorController);
        // var mainOutput = AnimationPlayableOutput.Create(playableGraph, "MainOutput", this.animator);
        // mainOutput.SetSourcePlayable(mainPlayable);
        
        // Debug.Log(rigBuilder.Build(playableGraph));
        // this.animator.playableGraph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
        //this.rigBuilder.graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
    }

    private void LateUpdate()
    {
        //this.playableGraph.Evaluate(Time.deltaTime);
        // this.animator.speed = 1;
        // this.animator.Update(Time.deltaTime);
        // this.animator.speed = 0;

        var directionIndicator = this.GetComponent<PlayerAvatar>().DirectionIndicator;
        this.aimTargetPoint.position = directionIndicator.transform.position + directionIndicator.forward * 1000;

        this.rigBuilder.graph.Evaluate(Time.deltaTime);
    }

    private void Update()
    {
        this.animator.speed = 1;
        this.animator.Update(Time.deltaTime);
        this.animator.speed = 0;
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
        this.animator.speed = 0;
        
    }
}
