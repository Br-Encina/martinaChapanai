using UnityEngine;

public class PlayerGrounededState : PlayerBaseState
{
    public PlayerGrounededState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base(currentContext, playerStateFactory)
    {
        IsRootState = true;
    }

    public override void EnterState()
    {
        InitializeSubState();
        Ctx.ApliedMovementY = Ctx.Gravity;
    }

    public override void UpdateState()
    {
        // Gravedad simple: si está en el piso, queda "pegado". Si no, acumula caída.
        if (Ctx.CharacterController.isGrounded)
        {
            Ctx.ApliedMovementY = Ctx.Gravity;
        }
        else
        {
            Ctx.ApliedMovementY += Ctx.Gravity * Time.deltaTime;
        }

        CheckSwichStates();
    }

    public override void ExitState()
    {
    }

    public override void CheckSwichStates()
    {
        if (Ctx.IsCrouchPressed)
        {
            SwitchState(Factory.Crouch());
            return;
        }
    }

    public override void InitializeSubState()
    {
        if (Ctx.IsMovementPressed)
        {
            SetSubState(Factory.Run());
        }
        else
        {
            SetSubState(Factory.Idle());
        }
    }
}
