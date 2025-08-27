using System.Collections.Generic;
using B83.Win32;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DynamicFileManager : MonoBehaviour
{
    private static DynamicFileManager instance;
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
        UnityDragAndDropHook.InstallHook();
        UnityDragAndDropHook.OnDroppedFiles += OnFiles;
    }
    void OnDisable()
    {
        UnityDragAndDropHook.UninstallHook();
    }

    void OnDestroy()
    {
        UnityDragAndDropHook.UninstallHook();
    }

    void OnFiles(List<string> aFiles, POINT aPos)
    {
        FileBrowserSet.LoadFiles(aFiles.ToArray());
        SceneLoader.LoadMusicLobby();
    }
}