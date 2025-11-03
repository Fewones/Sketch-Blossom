using UnityEngine;

public class ConsoleTest : MonoBehaviour
{
    void Start()
    {
        Debug.LogError("🔴 CONSOLE TEST SCRIPT IS RUNNING!");
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.LogWarning("🟡 MOUSE CLICKED!");
        }
    }
}
