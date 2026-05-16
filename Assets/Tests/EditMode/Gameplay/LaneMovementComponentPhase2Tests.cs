#if UNITY_INCLUDE_TESTS
using NUnit.Framework;

public sealed class LaneMovementComponentPhase2Tests
{
    [Test]
    public void TryMoveLeft_WhenTransitionActive_ReturnsFalse()
    {
        LaneMovementComponent laneMovementComponent = new LaneMovementComponent();
        laneMovementComponent.Initialize(0f);

        bool firstMove = laneMovementComponent.TryMoveLeft(0f);
        bool secondMove = laneMovementComponent.TryMoveLeft(0f);

        Assert.IsTrue(firstMove);
        Assert.IsFalse(secondMove);
    }

    [Test]
    public void TryMoveRight_WhenAtRightBoundary_ReturnsFalse()
    {
        LaneMovementComponent laneMovementComponent = new LaneMovementComponent();
        laneMovementComponent.Initialize(0f);

        laneMovementComponent.TryMoveRight(0f);
        laneMovementComponent.Tick(0f, 0.3f);
        laneMovementComponent.TryMoveRight(1.7f);
        laneMovementComponent.Tick(1.7f, 0.3f);

        bool result = laneMovementComponent.TryMoveRight(1.7f);

        Assert.IsFalse(result);
    }
}
#endif
