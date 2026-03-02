using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;

    public AudioClip[] playlist;
    AudioSource source;
    int currentTrack = 0;
    
    [Header("Settings")]
    public float switchTime = 60f; // switch after 60 seconds

    // Override State
    bool isOverridden = false;
    AudioClip storedClip;
    float storedTime;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        source = GetComponent<AudioSource>();
    }

    void Start()
    {
        PlayTrack();
    }

    void Update()
    {
        // Debug heartbeat every 5 seconds
        if (Time.frameCount % 300 == 0) 
             Debug.Log($"[MusicManager] Heartbeat | Time: {source.time:F1}/{switchTime} | Playing: {source.isPlaying} | Overridden: {isOverridden}");

        if (isOverridden) return;

        // Switch if song ends naturally OR if it exceeds the input time limit
        if (!source.isPlaying || source.time >= switchTime)
        {
            Debug.Log($"[MusicManager] Switch Triggered! Time: {source.time:F1}/{switchTime}");
            NextTrack();
        }
    }

    void PlayTrack()
    {
        source.clip = playlist[currentTrack];
        source.Play();
    }

    void NextTrack()
    {
        currentTrack++;
        if (currentTrack >= playlist.Length)
            currentTrack = 0;

        Debug.Log($"[MusicManager] Track finished. Playing next: {currentTrack} ({playlist[currentTrack].name})");
        PlayTrack();
    }

    // ===================================
    // Override System (For OP Mode etc.)
    // ===================================
    public void PlayOverrideMusic(AudioClip clip)
    {
        if (clip == null) return;

        // Store current state
        if (!isOverridden)
        {
            storedClip = source.clip;
            storedTime = source.time;
        }

        isOverridden = true;
        source.clip = clip;
        source.loop = true; // Loop the override music (e.g. Boss/OP theme)
        source.time = 0;
        source.Play();
        
        Debug.Log($"[MusicManager] Override started: {clip.name}");
    }

    public void StopOverrideMusic()
    {
        if (!isOverridden) return;

        isOverridden = false;
        
        // Restore previous track
        if (storedClip != null)
        {
            source.loop = false; // Disable loop so playlist logic works
            source.clip = storedClip;
            source.time = storedTime;
            source.Play();
            Debug.Log($"[MusicManager] Resuming track: {storedClip.name} at {storedTime}s");
        }
        else
        {
            // If nothing stored, just play current playlist track
            PlayTrack();
        }
    }
}
