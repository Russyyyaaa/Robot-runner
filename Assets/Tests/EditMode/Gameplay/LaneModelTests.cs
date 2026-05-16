#if UNITY_INCLUDE_TESTS
using NUnit.Framework;

public sealed class LaneModelTests
{
    [Test]
    public void TryMoveLeft_AtLeftBoundary_ReturnsFalse()
    {
        LaneModel laneModel = new LaneModel(3, 0);

        bool moved = laneModel.TryMoveLeft();

        Assert.IsFalse(moved);
        Assert.AreEqual(0, laneModel.CurrentLaneIndex);
    }

    [Test]
    public void TryMoveRight_AtRightBoundary_ReturnsFalse()
    {
        LaneModel laneModel = new LaneModel(3, 2);

        bool moved = laneModel.TryMoveRight();

        Assert.IsFalse(moved);
        Assert.AreEqual(2, laneModel.CurrentLaneIndex);
    }
}
#endif
