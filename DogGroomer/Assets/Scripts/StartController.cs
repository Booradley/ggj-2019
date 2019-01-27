using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class StartController : MonoBehaviour
{
    [SerializeField]
    private HoverButton _startButton;

    private void Start()
    {
        _startButton.onButtonUp.AddListener(OnStart);
    }

    private void OnStart(Hand hand)
    {
        _startButton.onButtonUp.RemoveAllListeners();

        StartCoroutine(StartSequence());
    }

    private IEnumerator StartSequence()
    {
        SteamVR_Fade.Start(Color.white, 1.0f);

        yield return new WaitForSeconds(1.0f);

        SceneManager.LoadScene("Main");
    }
}