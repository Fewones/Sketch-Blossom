using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;
    public Vector2[] playerSpawnPoints = {new Vector2(-6.49f, -2.1f), new Vector2(-6.03f, -0.47f), 
                                          new Vector2(-7.31f, -3.81f), new Vector2(-0.23f, -3.26f), 
                                          new Vector2(-6.7f, 1.16f), Vector2.zero};

    public Vector2[] backgroundSpawnPoints = {new Vector3(6.48f, 0.93f), new Vector3(-0.32f, 6.48f), new Vector3(-4.07f, 8.8f)};
    
    public bool facingLeft = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(Instance);
        } else
        {
            Destroy(this);
        }
    }


}
