using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public partial class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpd, mouseSen, jumpSpd, gravity;
    [SerializeField] private string[] animationStates;

    private NetworkFPSPlayer Player => this.GetComponent<NetworkFPSPlayer>();
    private PlayerAvatar Avatar => this.GetComponent<PlayerAvatar>();
    private TickScheduler TickScheduler => this.Player.TickScheduler;
    private PhysicsController PhysicsController => this.GetComponent<PhysicsController>();

    private float currentJump;
    [SerializeField] private bool inAir;

    protected partial void Awake();
    protected partial void Update();
    protected partial void TickUpdate();
}
