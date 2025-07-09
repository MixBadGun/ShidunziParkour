using System.Collections.Generic;
using B83.Win32;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DynamicFileManager : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(this);
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