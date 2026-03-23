using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;
    public Vector2[] playerSpawnPoints;
    public Vector2[] formerOffset;
    public Vector2[] nextOffset;
    public Vector2[] backgroundSpawnPoints;
    
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
