

public abstract class PlayerBaseState
{ 
    private bool isRootState = false; 
    private PlayerStateMachine _ctx;
    private PlayerStateFactory _factory;
    private PlayerBaseState _currentSuperState;
    private PlayerBaseState _currentSubState;

    protected PlayerStateMachine Ctx { get {  return _ctx; } }
    protected PlayerStateFactory Factory { get { return _factory; } }
    protected bool IsRootState { get { return isRootState; } set { isRootState = value; } }
    public PlayerBaseState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    {
        _ctx = currentContext;
        _factory = playerStateFactory;  
    }
    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void ExitState();
    public abstract void CheckSwichStates();
    public abstract void InitializeSubState();

    public void UpdateStates()
    {
        UpdateState();
        if (_currentSubState != null)
        {
            _currentSubState.UpdateStates();
        }
        CheckSwichStates();
    }

    protected void SwitchState(PlayerBaseState newState)
    {
        ExitState();
        newState.EnterState();

        if (isRootState)
        {
            _ctx.CurrentState = newState;
        }
        else if (_currentSuperState != null)
        {
            _currentSuperState.SetSubState(newState);
        }


    }

    protected void SetSuperState(PlayerBaseState newSuperState)
    {
        // Implementation for setting the super state
        _currentSuperState = newSuperState;
    }

    protected void SetSubState(PlayerBaseState newSubState)
    {
        // Implementation for setting the sub state
        _currentSubState = newSubState;
        newSubState.SetSuperState(this);
    }

}
