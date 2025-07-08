using UnityEngine;

public class MusicCommandSet : MonoBehaviour
{
    public AudioSource MusicPlayer;
    public GameObject PauseUI;

    public void Pause()
    {
        Time.timeScale = 0;
        MusicPlayer.Pause();
        PauseUI.SetActive(true);
    }

    public void Resume()
    {
        Time.timeScale = 1;
        MusicPlayer.UnPause();
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
