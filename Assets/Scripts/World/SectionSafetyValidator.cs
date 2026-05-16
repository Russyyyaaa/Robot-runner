public sealed class SectionSafetyValidator
{
    public bool IsSectionSafe(RoadSectionDefinition sectionDefinition)
    {
        return sectionDefinition.Prefab != null && sectionDefinition.IsSafeByDesign;
    }
}
