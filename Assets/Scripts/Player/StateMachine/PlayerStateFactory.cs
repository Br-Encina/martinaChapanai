using System.Collections.Generic;

enum PlayerStates
{
    Idle,
    Grounded,
    Run,
    Crouch
}

public class PlayerStateFactory
{
    private PlayerStateMachine _context;

    Dictionary<PlayerStates, PlayerBaseState> _states = new Dictionary<PlayerStates, PlayerBaseState>();

    public PlayerStateFactory(PlayerStateMachine currentContext)
    {
        _context = currentContext;
        _states[PlayerStates.Idle] = new PlayerIdleState(_context, this);
        _states[PlayerStates.Grounded] = new PlayerGrounededState(_context, this);
        _states[PlayerStates.Run] = new PlayerRunState(_context, this);
        _states[PlayerStates.Crouch] = new PlayerCrouchState(_context, this);
    }

    public PlayerBaseState Idle()
    {
        return _states[PlayerStates.Idle];
    }
    public PlayerBaseState Grounded()
    {
        return _states[PlayerStates.Grounded];
    }
    public PlayerBaseState Run()
    {
        return _states[PlayerStates.Run];
    }

    public PlayerBaseState Crouch() {
        return _states[PlayerStates.Crouch];
    }

}
