using UnityEngine;
using UnityEngine.SceneManagement;
public class Combat : MonoBehaviour
{
    public void DrawButton()
    {
        // Load the game scene
        SceneManager.LoadScene("DrawingScene");
    }
}
