using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public sealed class RunnerCollisionHandler : MonoBehaviour
{
    [SerializeField] private RunSessionController _runSessionController;
    [SerializeField] private float _trapHitToleranceX = 0.65f;
    [SerializeField] private float _trapHitToleranceZ = 0.75f;
    [SerializeField] private float _pickupBehindToleranceZ = 0.05f;
    [SerializeField] private float _minimumForwardDistanceToPickup = 0.02f;

    private Collider _runnerCollider;

    private void Awake()
    {
        if (_runSessionController == null)
        {
            _runSessionController = FindFirstObjectByType<RunSessionController>();
        }

        _runnerCollider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_runSessionController == null || _runSessionController.CurrentState != GameState.Running)
        {
            return;
        }

        if (_runSessionController.IsBoosterFlightActive)
        {
            return;
        }

        ITriggerActor triggerActor = other.GetComponentInParent<ITriggerActor>();
        if (triggerActor == null)
        {
            return;
        }

        if (IsPickupActor(triggerActor) && IsActorBehindRunner(other))
        {
            return;
        }

        if (triggerActor is TrapZoneAdapter && IsPreciseTrapHit(other) == false)
        {
            return;
        }

        triggerActor.ApplyTo(_runSessionController);
        if (triggerActor.IsSingleUse == false || triggerActor is MonoBehaviour behaviour == false)
        {
            return;
        }

        if (triggerActor.ConsumeOnApply)
        {
            behaviour.gameObject.SetActive(false);
            return;
        }

        DisableActorColliders(behaviour.gameObject);
    }

    private static void DisableActorColliders(GameObject actorObject)
    {
        if (actorObject == null)
        {
            return;
        }

        Collider[] colliders = actorObject.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider collider = colliders[i];
            if (collider == null)
            {
                continue;
            }

            collider.enabled = false;
        }
    }

    private bool IsPreciseTrapHit(Collider trapCollider)
    {
        if (trapCollider == null)
        {
            return false;
        }

        Vector3 runnerCenter = _runnerCollider != null ? _runnerCollider.bounds.center : transform.position;
        Vector3 trapClosestPoint = trapCollider.ClosestPoint(runnerCenter);

        float deltaX = Mathf.Abs(runnerCenter.x - trapClosestPoint.x);
        float deltaZ = Mathf.Abs(runnerCenter.z - trapClosestPoint.z);

        return deltaX <= Mathf.Max(0.05f, _trapHitToleranceX) && deltaZ <= Mathf.Max(0.05f, _trapHitToleranceZ);
    }

    private static bool IsPickupActor(ITriggerActor triggerActor)
    {
        return triggerActor is BatteryPickupAdapter || triggerActor is BoosterPickupAdapter;
    }

    private bool IsActorBehindRunner(Collider actorCollider)
    {
        if (actorCollider == null)
        {
            return false;
        }

        Vector3 runnerCenter = _runnerCollider != null ? _runnerCollider.bounds.center : transform.position;
        Vector3 closestPointToRunner = actorCollider.ClosestPoint(runnerCenter);
        Vector3 toActor = closestPointToRunner - runnerCenter;
        float forwardProjection = Vector3.Dot(transform.forward, toActor);
        float requiredForwardDistance = Mathf.Max(0.01f, _pickupBehindToleranceZ + _minimumForwardDistanceToPickup);
        return forwardProjection < -requiredForwardDistance;
    }
}
