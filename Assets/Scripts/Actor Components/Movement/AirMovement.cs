using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateMachines;
using AirMovementStates;

public class AirMovement : Movement
{
    [Header("Aerial Movement")]

    [SerializeField] private float movementSpeed;
    [SerializeField] private bool canLandOnGround;

    private AirMovementStateMachine movementStateMachine;

    public bool CanLandOnGround { get => canLandOnGround; set => canLandOnGround = value; }

    public bool IsGravityEnabled { get; private set; }

    public override MovementStateMachine MovementStateMachine { get => movementStateMachine; }

    protected override void Awake()
    {
        base.Awake();
        movementStateMachine = new AirMovementStateMachine();
    }

    protected override void Start()
    {
        base.Start();
        MovementStateMachine.ChangeState(new AirborneState(this));
    }

    public override void UpdateMovement(Vector2 direction)
    {
        CachedMovementDirection = direction;
        if (MovementStateMachine.CurrState is AirborneState airborneState)
        {
            airborneState.UpdateHorizontalMovement(direction.x);
            airborneState.UpdateVerticalMovement(direction.y);
        }
    }

    public override void SetMovementMode(MovementSpeedData.Mode mode)
    {
        if (MovementStateMachine.CurrState is AirborneState)
        {
            MovementMode = mode;
        }
    }

    public void ToggleGravity(bool isEnabled)
    {
        IsGravityEnabled = isEnabled;
        rigidbody2d.gravityScale = isEnabled ? 5f : 0f;
        if (isEnabled)
        {
            MovementStateMachine.ChangeState(new FallingState(this));
        }
    }
}
