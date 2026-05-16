#if UNITY_INCLUDE_TESTS
using NUnit.Framework;

public sealed class LaneModelPhase1Tests
{
    [Test]
    public void TryMoveLeft_WhenAtLeftBound_ReturnsFalse()
    {
        LaneModel laneModel = new LaneModel(3, 0);

        bool result = laneModel.TryMoveLeft();

        Assert.IsFalse(result);
        Assert.AreEqual(0, laneModel.CurrentLaneIndex);
    }

    [Test]
    public void TryMoveRight_WhenInsideBounds_ChangesLane()
    {
        LaneModel laneModel = new LaneModel(3, 1);

        bool result = laneModel.TryMoveRight();

        Assert.IsTrue(result);
        Assert.AreEqual(2, laneModel.CurrentLaneIndex);
    }
}
#endif
