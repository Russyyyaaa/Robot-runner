using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class BatteryPickupAdapter : MonoBehaviour, ITriggerActor
{
    [SerializeField] private bool _useConfigAmount = true;
    [SerializeField] private float _restoreAmount = 25f;

    public bool IsSingleUse => true;
    public bool ConsumeOnApply => true;

    public void ApplyTo(RunSessionController sessionController)
    {
        if (sessionController == null)
        {
            throw new ArgumentNullException(nameof(sessionController));
        }

        if (_useConfigAmount)
        {
            sessionController.RestoreDefaultBatteryAmount();
            return;
        }

        sessionController.RestoreBattery(_restoreAmount);
    }
}
