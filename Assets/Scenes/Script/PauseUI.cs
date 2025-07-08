using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseUI : MonoBehaviour
{
    public GameObject PausePanel;
    public AudioSource BGM;
    public Move player;

    void Awake() {
        PausePanel.SetActive(false);
    }

    void Update()
    {
        KeyCode[] escKeys = DataStorager.keysettings.esc;
        foreach( KeyCode key in escKeys ){
            if(Input.GetKeyDown(key)){
                TogglePause();
            }
        }
    }

    public void TogglePause()
    {
        if (PausePanel.activeSelf)
        {
            Time.timeScale = 1;
            if (player.isAlive())
            {
                BGM.UnPause();
            }
        }
        else
        {
            Time.timeScale = 0;
            BGM.Pause();
        }
        PausePanel.SetActive(!PausePanel.activeSelf);
    }
}
