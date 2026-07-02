using UnityEngine;

public class PlayerCrouchState : PlayerBaseState
{
    public PlayerCrouchState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
        : base(currentContext, playerStateFactory) { }

    public override void EnterState()
    {
        Ctx.IsCrouching = true;
        Ctx.Animator.SetBool(Ctx.IsCrouchingHash, true);
        Ctx.Animator.SetBool(Ctx.IsRunningHash, false);

        // Achicar el CharacterController para que el player quepa detrás de paredes bajas
        Ctx.CharacterController.height = Ctx.CrouchingCCHeight;
        Ctx.CharacterController.center = Vector3.up * (Ctx.CrouchingCCHeight / 2f);
    }

    public override void UpdateState()
    {
        bool isMoving = Ctx.IsMovementPressed;

        Ctx.Animator.SetBool(Ctx.IsCrouchingHash, isMoving);
        Ctx.Animator.SetBool("isCrouchWalking", isMoving);

        if (isMoving)
        {
            Vector3 inputDirection = new Vector3(Ctx.CurrentMovementInput.x, 0f, Ctx.CurrentMovementInput.y);
            Quaternion isoRotation = Quaternion.Euler(0f, Ctx.IsometricAngle, 0f);
            Vector3 isometricDirection = isoRotation * inputDirection;
            Ctx.ApliedMovementX = isometricDirection.x * Ctx.CrouchMoveSpeed;
            Ctx.ApliedMovementZ = isometricDirection.z * Ctx.CrouchMoveSpeed;
        }
        else
        {
            Ctx.ApliedMovementX = 0f;
            Ctx.ApliedMovementZ = 0f;
        }

        CheckSwichStates();
    }

    public override void ExitState()
    {
        Ctx.IsCrouching = false;
        Ctx.Animator.SetBool(Ctx.IsCrouchingHash, false);

        // Restaurar el CharacterController a la altura normal
        Ctx.CharacterController.height = Ctx.StandingCCHeight;
        Ctx.CharacterController.center = Vector3.up * (Ctx.StandingCCHeight / 2f);
    }

    public override void CheckSwichStates()
    {
        // Soltar Crouch → volver a Grounded (que decidirá si está Idle o corriendo)
        if (!Ctx.IsCrouchPressed)
        {
            SwitchState(Factory.Grounded());
        }
    }

    public override void InitializeSubState() { }
}
