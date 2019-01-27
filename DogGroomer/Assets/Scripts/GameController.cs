using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class GameController : MonoBehaviour
{
    [SerializeField]
    private List<FurController> _furControllers;

    [SerializeField]
    private LazySusan _lazySusan;

    [SerializeField]
    private List<Razor> _razors;

    [SerializeField]
    private HoverButton _restartButton;

    [SerializeField]
    private List<Interactable> _resetables;

    private Dictionary<Interactable, Quaternion> _initialRotations;
    private Dictionary<Interactable, Vector3> _initialPositions;

    private void Start()
    {
        _restartButton.onButtonUp.AddListener(OnRestart);

        _initialPositions = new Dictionary<Interactable, Vector3>();
        _initialRotations = new Dictionary<Interactable, Quaternion>();

        for (int i = 0; i < _resetables.Count; i++)
        {
            _initialPositions.Add(_resetables[i], _resetables[i].transform.position);
            _initialRotations.Add(_resetables[i], _resetables[i].transform.rotation);
        }
    }

    private void OnRestart(Hand hand)
    {
        Restart();
    }

    private void Restart()
    {
        _lazySusan.Reset();

        for (int i = 0; i < _furControllers.Count; i++)
        {
            _furControllers[i].Reset();
        }

        for (int i = 0; i < _razors.Count; i++)
        {
            _razors[i].Reset();
        }

        for (int i = 0; i < _resetables.Count; i++)
        {
            if (_resetables[i].attachedToHand != null)
            {
                _resetables[i].attachedToHand.DetachObject(gameObject);
                _resetables[i].attachedToHand.HoverUnlock(_resetables[i]);
            }

            _resetables[i].transform.position = _initialPositions[_resetables[i]];
            _resetables[i].transform.rotation = _initialRotations[_resetables[i]];
        }
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            Application.Quit();
            return;
        }

        if (Input.GetKeyUp(KeyCode.F1))
        {
            Restart();
        }
    }
}