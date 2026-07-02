using UnityEngine;

public class PlayerRunState : PlayerBaseState
{
    public PlayerRunState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base(currentContext, playerStateFactory)
    {
    }

    public override void EnterState()
    {
        Ctx.Animator.SetBool(Ctx.IsRunningHash, true);
    }

    public override void UpdateState()
    {
        // Tomamos el input crudo (X = horizontal, Y = vertical del teclado)
        Vector3 inputDirection = new Vector3(Ctx.CurrentMovementInput.x, 0f, Ctx.CurrentMovementInput.y);

        // Lo rotamos para que coincida con los ejes visuales de la cámara isométrica
        Quaternion isoRotation = Quaternion.Euler(0f, Ctx.IsometricAngle, 0f);
        Vector3 isometricDirection = isoRotation * inputDirection;

        Ctx.ApliedMovementX = isometricDirection.x * Ctx.MoveSpeed;
        Ctx.ApliedMovementZ = isometricDirection.z * Ctx.MoveSpeed;

        CheckSwichStates();
    }

    public override void ExitState()
    {
    }

    public override void CheckSwichStates()
    {
        if (!Ctx.IsMovementPressed)
        {
            SwitchState(Factory.Idle());
        }
    }

    public override void InitializeSubState()
    {
    }
}
