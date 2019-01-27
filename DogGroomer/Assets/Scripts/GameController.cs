using System;
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

    private void Start()
    {
        _restartButton.onButtonUp.AddListener(OnRestart);
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