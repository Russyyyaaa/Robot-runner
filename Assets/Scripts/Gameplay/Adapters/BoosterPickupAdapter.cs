using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class BoosterPickupAdapter : MonoBehaviour, ITriggerActor
{
    [SerializeField] private bool _useConfigValues = true;
    [SerializeField] private float _speedBonus = 3f;
    [SerializeField] private float _durationSeconds = 4f;

    public bool IsSingleUse => true;
    public bool ConsumeOnApply => true;

    public void ApplyTo(RunSessionController sessionController)
    {
        if (sessionController == null)
        {
            throw new ArgumentNullException(nameof(sessionController));
        }

        if (_useConfigValues)
        {
            sessionController.ActivateDefaultBooster();
            return;
        }

        sessionController.ActivateBooster(_speedBonus, _durationSeconds);
    }
}
