using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using UnityEngine.Localization;

public class SteamOverlay : MonoBehaviour
{
    private static SteamOverlay instance;
    protected Callback<GameOverlayActivated_t> m_GameOverlayActivated;
    private bool loaded = false;
    void Awake()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        DontDestroyOnLoad(this.gameObject);
        instance = this;
    }
    void OnEnable()
    {
        m_GameOverlayActivated = Callback<GameOverlayActivated_t>.Create(OnGameOverlayActivated);
        if(loaded == false)
        {
            LoadLanguage();
            loaded = true;
        }
    }

    void LoadLanguage() {
        if (!SteamManager.Initialized)
        {
            return;
        }
        string userLanguage = SteamApps.GetCurrentGameLanguage();
        switch (userLanguage)
        {
            case "schinese":
                {
                    LanguageManager.LoadLanguage("zh-Hans");
                    break;
                }
            default:
                {
                    LanguageManager.LoadLanguage("en");
                    break;
                }
        }
    }

    private void OnGameOverlayActivated(GameOverlayActivated_t pCallback)
    {
        if (pCallback.m_bActive != 0)
        {
            Debug.Log("Steam Overlay has been activated");
            MusicCommandSet.TryPause();
            PauseUI.TryPause();
        }
    }
}
