public sealed class RunSessionStateModel
{
    private GameState _currentState = GameState.Menu;

    public GameState CurrentState => _currentState;
    public bool IsRunning => _currentState == GameState.Running;

    public bool TryStartRun()
    {
        if (_currentState == GameState.Running)
        {
            return false;
        }

        _currentState = GameState.Running;
        return true;
    }

    public bool TryEndRun()
    {
        if (_currentState != GameState.Running)
        {
            return false;
        }

        _currentState = GameState.GameOver;
        return true;
    }

    public bool TryReturnToMenu()
    {
        if (_currentState == GameState.Menu)
        {
            return false;
        }

        _currentState = GameState.Menu;
        return true;
    }
}
