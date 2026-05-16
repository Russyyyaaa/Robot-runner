using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class TrapZoneAdapter : MonoBehaviour, ITriggerActor
{
    [SerializeField] private bool _useConfigAmount = true;
    [SerializeField] private float _damageAmount = 20f;

    public bool IsSingleUse => true;
    public bool ConsumeOnApply => false;

    public void ApplyTo(RunSessionController sessionController)
    {
        if (sessionController == null)
        {
            throw new ArgumentNullException(nameof(sessionController));
        }

        if (_useConfigAmount)
        {
            sessionController.ApplyDefaultTrapDamage();
            return;
        }

        sessionController.ApplyTrapDamage(_damageAmount);
    }
}
