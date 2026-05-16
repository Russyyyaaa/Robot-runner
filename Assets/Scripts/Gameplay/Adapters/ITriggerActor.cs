public interface ITriggerActor
{
    bool IsSingleUse { get; }
    bool ConsumeOnApply { get; }

    void ApplyTo(RunSessionController sessionController);
}
