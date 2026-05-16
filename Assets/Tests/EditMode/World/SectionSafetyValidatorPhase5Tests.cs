#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using UnityEngine;

public sealed class SectionSafetyValidatorPhase5Tests
{
    private GameObject _safeObject;
    private GameObject _unsafeObject;

    [SetUp]
    public void SetUp()
    {
        _safeObject = new GameObject("safe");
        _unsafeObject = new GameObject("unsafe");
    }

    [TearDown]
    public void TearDown()
    {
        if (_safeObject != null)
        {
            Object.DestroyImmediate(_safeObject);
        }

        if (_unsafeObject != null)
        {
            Object.DestroyImmediate(_unsafeObject);
        }
    }

    [Test]
    public void IsSectionSafe_ReturnsConfiguredSafetyFlag()
    {
        SectionSafetyValidator validator = new SectionSafetyValidator();
        RoadSectionDefinition safe = new RoadSectionDefinition(_safeObject, true);
        RoadSectionDefinition unsafeSection = new RoadSectionDefinition(_unsafeObject, false);

        Assert.IsTrue(validator.IsSectionSafe(safe));
        Assert.IsFalse(validator.IsSectionSafe(unsafeSection));
    }
}
#endif
