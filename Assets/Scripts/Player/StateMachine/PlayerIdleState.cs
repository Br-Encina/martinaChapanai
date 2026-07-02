public class PlayerIdleState : PlayerBaseState
{
    public PlayerIdleState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base(currentContext, playerStateFactory)
    {
    }

    public override void EnterState()
    {
        Ctx.Animator.SetBool(Ctx.IsRunningHash, false);
        Ctx.ApliedMovementX = 0f;
        Ctx.ApliedMovementZ = 0f;
    }

    public override void UpdateState()
    {
        CheckSwichStates();
    }

    public override void ExitState()
    {
    }

    public override void CheckSwichStates()
    {
        if (Ctx.IsMovementPressed)
        {
            SwitchState(Factory.Run());
        }
    }

    public override void InitializeSubState()
    {
    }
}
