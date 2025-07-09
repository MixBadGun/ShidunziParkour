using Mirror.Discovery;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader
{
    public static void LoadMusicLobby()
    {
        GameObject manager = GameObject.Find("NetworkManager");
        if (manager)
        {
            manager.GetComponent<NetworkDiscovery>().StopDiscovery();
            Object.DestroyImmediate(manager);
        }
        manager = GameObject.Find("MultyScript");
        if (manager)
        {
            Object.DestroyImmediate(manager);
        }
        BeatmapInfo.musicMode = true;
        SceneManager.LoadScene("MusicLobby");
    }

    public static void LoadRunning()
    {
        BeatmapInfo.musicMode = false;
        SceneManager.LoadScene("Running");
    }
}
