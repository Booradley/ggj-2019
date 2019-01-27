using System;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class LazySusan : MonoBehaviour
{
    [SerializeField]
    private Interactable _interactable;

    private Quaternion _initialRotation;

    private void Start()
    {
        _initialRotation = transform.rotation;
    }

    public void Reset()
    {
        transform.rotation = _initialRotation;
    }
}