using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // ==== FRAME RATE & PERFORMANCE SETTINGS ====
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 120;

        // Add other initializations here later
    }
}
