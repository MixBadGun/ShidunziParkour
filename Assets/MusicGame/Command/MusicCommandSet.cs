using UnityEngine;
using UnityEngine.Video;

public class MusicCommandSet : MonoBehaviour
{
    public AudioSource MusicPlayer;
    public GameObject PauseUI;
    public VideoPlayer videoPlayer;

    public void Pause()
    {
        Time.timeScale = 0;
        MusicPlayer.Pause();
        if (BeatmapManager.IsVideoPlaying())
        {
            videoPlayer.Pause();
        }
        PauseUI.SetActive(true);
    }

    public void Resume()
    {
        Time.timeScale = 1;
        MusicPlayer.UnPause();
        if (BeatmapManager.IsVideoPlaying())
        {
            videoPlayer.Play();
        }
        PauseUI.SetActive(false);
    }

    void Update()
    {
        KeyCode[] escKeys = DataStorager.keysettings.esc;
        foreach( KeyCode key in escKeys ){
            if(Input.GetKeyDown(key)){
                Pause();
            }
        }
    }
}
