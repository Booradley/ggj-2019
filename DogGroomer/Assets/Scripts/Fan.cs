using UnityEngine;

public class Fan : MonoBehaviour
{
    [SerializeField]
    private Vector3 _fanSpeed;

	private void Update()
    {
        transform.Rotate(_fanSpeed);
	}
}