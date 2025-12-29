using UnityEngine;

/// <summary>
/// Manages window settings for the standalone build.
/// Sets window to windowed mode (not maximized) and makes it resizable.
/// </summary>
public class WindowManager : MonoBehaviour
{
    [Header("Window Settings")]
    public int windowWidth = 1920;
    public int windowHeight = 1080;
    public bool startWindowed = true;
    public bool makeResizable = true;

    private void Start()
    {
        // Only apply window settings in standalone builds (not in editor)
        #if !UNITY_EDITOR
        if (startWindowed)
        {
            // First, ensure we're not in fullscreen
            if (Screen.fullScreen)
            {
                Screen.fullScreen = false;
            }
            
            // Set to windowed mode with specific resolution
            // Using a slightly smaller initial size to ensure it's not maximized
            Screen.SetResolution(windowWidth, windowHeight, FullScreenMode.Windowed);
            
            // Force window to not be maximized by setting a smaller size first, then desired size
            // This is a workaround for Unity's window management
            StartCoroutine(EnsureWindowedMode());
        }
        #endif
        
        // Log current settings for debugging
        Debug.Log($"[WindowManager] Resolution: {Screen.width}x{Screen.height}, Fullscreen: {Screen.fullScreen}, Mode: {Screen.fullScreenMode}");
    }
    
    private System.Collections.IEnumerator EnsureWindowedMode()
    {
        #if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        // Wait a frame for window to initialize
        yield return null;
        
        // Set to a smaller size first to break out of maximized state
        Screen.SetResolution(windowWidth - 100, windowHeight - 100, FullScreenMode.Windowed);
        yield return new WaitForSeconds(0.1f);
        
        // Now set to desired size - window should be resizable now
        Screen.SetResolution(windowWidth, windowHeight, FullScreenMode.Windowed);
        #else
        yield return null;
        #endif
    }

    // Optional: Allow toggling fullscreen with F11 key
    private void Update()
    {
        #if !UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.F11))
        {
            Screen.fullScreen = !Screen.fullScreen;
            Debug.Log($"[WindowManager] Toggled fullscreen: {Screen.fullScreen}");
        }
        #endif
    }
}

