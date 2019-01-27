using UnityEngine;
using UnityEngine.SceneManagement;
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
        SceneManager.LoadScene("Main");
    }
}