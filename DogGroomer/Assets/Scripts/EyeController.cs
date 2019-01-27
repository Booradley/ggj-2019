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
        _initialLeftEyeRotation = _leftEye.rotation;
        _initialRightEyeRotation = _rightEye.rotation;

        StartCoroutine(EyeSequence());
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

    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponentInParent<Throwable>() == null)
            return;

        if (_target == null || Vector3.Distance(_leftEye.position, other.transform.position) < Vector3.Distance(_leftEye.position, _target.position))
        {
            SetTarget(other.transform);
        }
    }

    private IEnumerator EyeSequence()
    {
        while (true)
        {
            if (_target != null)
            {
                _leftEye.LookAt(_target);
                _rightEye.LookAt(_target);
                _rightEye.Rotate(new Vector3(0f, 180f, 0f), Space.Self);

                Debug.Log(_target.name);
            }
            else
            {
                _leftEye.rotation = _initialLeftEyeRotation;
                _rightEye.rotation = _initialRightEyeRotation;

                Debug.Log("None");
            }

            yield return null;
        }
    }

    private void SetTarget(Transform target)
    {
        _target = target;
    }
}
