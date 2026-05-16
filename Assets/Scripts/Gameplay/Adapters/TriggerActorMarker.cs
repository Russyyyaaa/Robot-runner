using UnityEngine;

[DisallowMultipleComponent]
public sealed class TriggerActorMarker : MonoBehaviour
{
    [SerializeField] private TriggerActorMarkerKind _kind = TriggerActorMarkerKind.None;

    public TriggerActorMarkerKind Kind => _kind;
}

public enum TriggerActorMarkerKind
{
    None = 0,
    Trap = 1,
    Booster = 2,
    Battery = 3
}
