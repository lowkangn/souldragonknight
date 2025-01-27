using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DragonPlayerController : PlayerController
{
    [SerializeField] private AirMovement movement;

    private InputAction moveAirHorizontalAction;
    private InputAction moveAirVerticalAction;
    private InputAction rangedAttackAction;
    private InputAction rangedAttackDownAction;
    private InputAction dodgeAction;
    private InputAction startInteractionAction;
    private InputAction stopInteractionAction;

    private HealthUI healthUI;
    private ConsumableResourceUI manaUI;

    private float horizontalMovementInput = 0f;
    private float verticalMovementInput = 0f;

    public override Movement Movement { get => movement; }

    protected override void Awake()
    {
        base.Awake();
        moveAirHorizontalAction = playerInput.actions["MoveAirHorizontal"];
        moveAirVerticalAction = playerInput.actions["MoveAirVertical"];
        rangedAttackAction = playerInput.actions["RangedAttack"];
        rangedAttackDownAction = playerInput.actions["RangedAttackDown"];
        dodgeAction = playerInput.actions["AirDodge"];
        startInteractionAction = playerInput.actions["InteractAir"];
        stopInteractionAction = playerInput.actions["InteractAirStop"];

        healthUI = FindObjectOfType<HealthUI>();
        manaUI = FindObjectOfType<ConsumableResourceUI>();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (combat.ActionStateMachine.CurrState == null || combat.ActionStateMachine.CurrState is CombatStates.AttackState)
        {
            movement.UpdateMovement(new Vector2(horizontalMovementInput, verticalMovementInput));
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if (photonView.IsMine)
        {
            Combat.Health.UpdateHealthEvent.AddListener(healthUI.UpdateDragonHealthUI);
            Combat.Resource.UpdateResourceEvent.AddListener(manaUI.UpdateManaUI);
            Combat.Resource.RegenerateResourceEvent.AddListener(manaUI.RegenerateManaUI);
            Combat.Resource.StopRegenResourceEvent.AddListener(manaUI.StopRegenManaUI);
            Combat.Resource.InsufficientResourceEvent.AddListener(manaUI.FlashManaWarning);
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (photonView.IsMine)
        {
            Combat.Health.UpdateHealthEvent.RemoveListener(healthUI.UpdateDragonHealthUI);
            Combat.Resource.UpdateResourceEvent.RemoveListener(manaUI.UpdateManaUI);
            Combat.Resource.RegenerateResourceEvent.RemoveListener(manaUI.RegenerateManaUI);
            Combat.Resource.StopRegenResourceEvent.RemoveListener(manaUI.StopRegenManaUI);
            Combat.Resource.InsufficientResourceEvent.RemoveListener(manaUI.FlashManaWarning);
        }
    }

    protected override void BindInputActionHandlers()
    {
        moveAirHorizontalAction.performed += HandleMoveAirHorizontalInput;
        moveAirVerticalAction.performed += HandleMoveAirVerticalInput;
        rangedAttackAction.performed += HandleRangedAttackInput;
        rangedAttackDownAction.performed += HandleRangedAttackDownInput;
        dodgeAction.performed += HandleDodgeInput;
        startInteractionAction.performed += HandleStartInteractionInput;
        stopInteractionAction.performed += HandleStopInteractionInput;
    }

    protected override void UnbindInputActionHandlers()
    {
        moveAirHorizontalAction.performed -= HandleMoveAirHorizontalInput;
        moveAirVerticalAction.performed -= HandleMoveAirVerticalInput;
        rangedAttackAction.performed -= HandleRangedAttackInput;
        rangedAttackDownAction.performed -= HandleRangedAttackDownInput;
        dodgeAction.performed -= HandleDodgeInput;
        startInteractionAction.performed -= HandleStartInteractionInput;
        stopInteractionAction.performed -= HandleStopInteractionInput;
    }

    private void HandleMoveAirHorizontalInput(InputAction.CallbackContext context)
    {
        horizontalMovementInput = context.ReadValue<float>();
    }

    private void HandleMoveAirVerticalInput(InputAction.CallbackContext context)
    {
        verticalMovementInput = context.ReadValue<float>();
    }

    private void HandleRangedAttackInput(InputAction.CallbackContext context)
    {
        if (movement.MovementStateMachine.CurrState is AirMovementStates.AirborneState)
        {
            Vector2 direction = movement.IsFacingRight ? Vector2.right : Vector2.left;
            combat.ExecuteCombatAbility(CombatAbilityIdentifier.ATTACK_RANGED, direction);
        }
    }

    private void HandleRangedAttackDownInput(InputAction.CallbackContext context)
    {
        if (movement.MovementStateMachine.CurrState is AirMovementStates.AirborneState)
        {
            combat.ExecuteCombatAbility(CombatAbilityIdentifier.ATTACK_RANGED, Vector2.down);
        }
    }

    private void HandleDodgeInput(InputAction.CallbackContext context)
    {
        if (movement.MovementStateMachine.CurrState is AirMovementStates.AirborneState)
        {
            movement.UpdateMovement(Vector2.zero);
            if (horizontalMovementInput < 0f && movement.IsFacingRight
                || horizontalMovementInput > 0f && !movement.IsFacingRight)
            {
                movement.FlipDirection(
                    movement.IsFacingRight ? Movement.Direction.LEFT : Movement.Direction.RIGHT);
            }

            Vector2 direction = new Vector2(horizontalMovementInput, verticalMovementInput);
            if (direction == Vector2.zero)
            {
                // if not moving, dodge upwards
                direction.y = 1f;
            }

            combat.ExecuteCombatAbility(CombatAbilityIdentifier.DODGE, direction);
        }
    }

    private void HandleStartInteractionInput(InputAction.CallbackContext context)
    {
        if (combat.ActionStateMachine.CurrState == null)
        {
            Interactable nearestInteractable = interactableDetector.GetNearestInteractable();
            if (nearestInteractable != null)
            {
                Interact(nearestInteractable);
            }
        }
    }

    private void HandleStopInteractionInput(InputAction.CallbackContext context)
    {
        InterruptInteraction();
    }

    protected override void HandleDeathEvent()
    {
        base.HandleDeathEvent();
        movement.ToggleGravity(true);
        PlayerManager.Instance.IncrementDeathCount();
    }

    protected override void HandleReviveStartEvent()
    {
        base.HandleReviveStartEvent();
        PlayerManager.Instance.DecrementDeathCount();
    }

    protected override void HandleReviveFinishEvent()
    {
        base.HandleReviveFinishEvent();
        movement.ToggleGravity(false);
        movement.TakeFlight();
    }
}
