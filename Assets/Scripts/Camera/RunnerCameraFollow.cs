using UnityEngine;

public sealed class RunnerCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private Vector3 _offset = new Vector3(2.4f, 8.2f, -11.5f);
    [SerializeField] private float _followLerpSpeed = 10f;
    [SerializeField] private bool _lockRotation = true;
    [SerializeField] private Vector3 _fixedEulerAngles = new Vector3(35f, -12f, 2f);

    public void SetTarget(Transform target)
    {
        _target = target;

        if (_target == null)
        {
            return;
        }

        transform.position = _target.position + _offset;

        if (_lockRotation)
        {
            transform.rotation = Quaternion.Euler(_fixedEulerAngles);
        }
    }

    private void LateUpdate()
    {
        if (_target == null)
        {
            return;
        }

        Vector3 targetPosition = _target.position + _offset;
        float lerpFactor = _followLerpSpeed * Time.deltaTime;
        transform.position = Vector3.Lerp(transform.position, targetPosition, lerpFactor);

        if (_lockRotation)
        {
            transform.rotation = Quaternion.Euler(_fixedEulerAngles);
        }
    }
}
