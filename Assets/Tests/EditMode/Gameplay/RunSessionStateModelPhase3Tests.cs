#if UNITY_INCLUDE_TESTS
using NUnit.Framework;

public sealed class RunSessionStateModelPhase3Tests
{
    [Test]
    public void TryStartRun_FromMenu_SwitchesToRunning()
    {
        RunSessionStateModel stateModel = new RunSessionStateModel();

        bool started = stateModel.TryStartRun();

        Assert.IsTrue(started);
        Assert.AreEqual(GameState.Running, stateModel.CurrentState);
    }

    [Test]
    public void TryEndRun_FromRunning_SwitchesToGameOver()
    {
        RunSessionStateModel stateModel = new RunSessionStateModel();
        stateModel.TryStartRun();

        bool ended = stateModel.TryEndRun();

        Assert.IsTrue(ended);
        Assert.AreEqual(GameState.GameOver, stateModel.CurrentState);
    }

    [Test]
    public void TryEndRun_WhenAlreadyGameOver_ReturnsFalse()
    {
        RunSessionStateModel stateModel = new RunSessionStateModel();
        stateModel.TryStartRun();
        stateModel.TryEndRun();

        bool endedAgain = stateModel.TryEndRun();

        Assert.IsFalse(endedAgain);
        Assert.AreEqual(GameState.GameOver, stateModel.CurrentState);
    }

    [Test]
    public void TryEndRun_FromMenu_ReturnsFalse()
    {
        RunSessionStateModel stateModel = new RunSessionStateModel();

        bool ended = stateModel.TryEndRun();

        Assert.IsFalse(ended);
        Assert.AreEqual(GameState.Menu, stateModel.CurrentState);
    }
}
#endif
