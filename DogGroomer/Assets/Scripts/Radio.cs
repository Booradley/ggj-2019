using UnityEngine;

public class Radio : MonoBehaviour
{
    private void Start()
    {
        GameObject.DontDestroyOnLoad(gameObject);
    }
}