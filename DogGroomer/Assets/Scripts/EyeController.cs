using System;
using System.Collections;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class EyeController : MonoBehaviour
{
    [SerializeField]
    private Transform _leftEye;

    [SerializeField]
    private Transform _rightEye;

    private Transform _target;
    private Quaternion _initialLeftEyeRotation;
    private Quaternion _initialRightEyeRotation;

    private void Start()
    {
        _initialLeftEyeRotation = _leftEye.localRotation;
        _initialRightEyeRotation = _rightEye.localRotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<Throwable>() == null)
            return;

        if (_target == null)
        {
            SetTarget(other.transform);
        }
        else if (other.transform != _target)
        {
            if (Vector3.Distance(_leftEye.position, other.transform.position) < Vector3.Distance(_leftEye.position, _target.position))
            {
                SetTarget(other.transform);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform == _target)
        {
            _target = null;
        }
    }

    private Quaternion _leftEyeTargetRotation;
    private Quaternion _rightEyeTargetRotation;

    private void Update()
    {
        if (_target != null)
        {
            Quaternion initialLeftEyeRotation = _leftEye.localRotation;
            Quaternion initialRightEyeRotation = _rightEye.localRotation;

            _leftEye.LookAt(_target);
            _rightEye.LookAt(_target);
            _rightEye.Rotate(new Vector3(0f, 180f, 180f), Space.Self);

            _leftEyeTargetRotation = _leftEye.localRotation;
            _rightEyeTargetRotation = _rightEye.localRotation;

            _leftEye.localRotation = initialLeftEyeRotation;
            _rightEye.localRotation = initialRightEyeRotation;
        }
        else
        {
            _leftEyeTargetRotation = _initialLeftEyeRotation;
            _rightEyeTargetRotation = _initialRightEyeRotation;
        }

        _leftEye.localRotation = Quaternion.Lerp(_leftEye.localRotation, _leftEyeTargetRotation, 0.2f);
        _rightEye.localRotation = Quaternion.Lerp(_rightEye.localRotation, _rightEyeTargetRotation, 0.2f);
    }

    private void SetTarget(Transform target)
    {
        _target = target;
    }
}
