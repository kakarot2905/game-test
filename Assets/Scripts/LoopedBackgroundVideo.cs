using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

[RequireComponent(typeof(RawImage))]
[RequireComponent(typeof(VideoPlayer))]
public class LoopedBackgroundVideo : MonoBehaviour
{
    private VideoPlayer videoPlayer;
    private RawImage rawImage;
    private RenderTexture renderTexture;

    [Header("Settings")]
    public Color startColor = Color.black; // Hide the static white image
    public bool useAudio = false;          // Disable audio for smoother loop if not needed

    void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        rawImage = GetComponent<RawImage>();

        // 1. Hide the RawImage initially (prevent white static flash)
        rawImage.color = startColor;

        // 2. Configure Video Player for performance
        videoPlayer.playOnAwake = false; // We play manually after prepare
        videoPlayer.isLooping = true;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        
        // Critical for smooth loops:
        videoPlayer.skipOnDrop = true; // Skip frames to keep time
        videoPlayer.waitForFirstFrame = true; // Wait until ready

        // Disable audio if not needed (audio sync often causes loop glitches)
        if (!useAudio)
        {
            videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        }

        // Prepare
        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.Prepare();
    }

    void OnVideoPrepared(VideoPlayer vp)
    {
        // Create RenderTexture
        if (renderTexture == null)
        {
            renderTexture = new RenderTexture((int)vp.width, (int)vp.height, 0);
            renderTexture.name = "BackgroundVideo_RT";
        }
        
        // Assign
        vp.targetTexture = renderTexture;
        rawImage.texture = renderTexture;
        
        // Play
        vp.Play();
        
        // Reveal Image immediately? Or wait a tiny bit?
        // Let's reveal it now. The 'waitForFirstFrame' setting helps ensure it's not empty.
        rawImage.color = Color.white; 
        
        Debug.Log($"[BackgroundVideo] Playing loop: {vp.width}x{vp.height}");
    }

    void OnDestroy()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }
    }
}
