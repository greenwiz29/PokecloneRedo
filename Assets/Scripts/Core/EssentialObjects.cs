using UnityEngine;

public class EssentialObjects : MonoBehaviour
{
	void Awake()
	{
        DontDestroyOnLoad(gameObject);
	}
}
