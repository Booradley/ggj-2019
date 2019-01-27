using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField]
    private List<FurController> _furControllers;

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            Application.Quit();
            return;
        }

        if (Input.GetKeyUp(KeyCode.F1))
        {
            for (int i = 0; i < _furControllers.Count; i++)
            {
                _furControllers[i].Reset();
            }
        }
    }
}