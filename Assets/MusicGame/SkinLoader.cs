using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class SkinLoader : MonoBehaviour
{
    public Texture2D track_texture;
    public Texture2D obstacle_texture;
    public Texture2D side_texture;
    public Texture2D side_floor_texture;

    string dataFolder;
    // Start is called before the first frame update
    void Start()
    {
        dataFolder = $"{Application.persistentDataPath}/skin";

        // DataStorager.settings.skinPath = "Black and White";
        if(DataStorager.settings.skinPath == ""){
            return;
        }
        string skin_path = $"{dataFolder}/{DataStorager.settings.skinPath}";
        if(!Directory.Exists(skin_path)){
            return;
        }

        // 加载皮肤
        if(File.Exists($"{skin_path}/track.png")){
            byte[] fileData = File.ReadAllBytes($"{skin_path}/track.png");
            track_texture.LoadImage(fileData);
        }

        if(File.Exists($"{skin_path}/obstacle.png")){
            byte[] fileData = File.ReadAllBytes($"{skin_path}/obstacle.png");
            obstacle_texture.LoadImage(fileData);
        }

        if(File.Exists($"{skin_path}/side_building.png")){
            byte[] fileData = File.ReadAllBytes($"{skin_path}/side_building.png");
            side_texture.LoadImage(fileData);
        }

        if(File.Exists($"{skin_path}/side_floor.png")){
            byte[] fileData = File.ReadAllBytes($"{skin_path}/side_floor.png");
            side_floor_texture.LoadImage(fileData);
        }

        if(File.Exists($"{skin_path}/crash.wav")){
            StartCoroutine(LoadBoomSound($"file://{skin_path}/crash.wav",AudioType.WAV));
        }

        if(File.Exists($"{skin_path}/golden.wav")){
            StartCoroutine(LoadBestSound($"file://{skin_path}/golden.wav",AudioType.WAV));
        }
    }

    IEnumerator LoadBoomSound(string path, AudioType audioType)
    {
        using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path, audioType);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(www.error);
        }
        else
        {
            AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
            SoundManager.boom_sound = clip;
        }
    }

    IEnumerator LoadBestSound(string path, AudioType audioType)
    {
        using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path, audioType);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(www.error);
        }
        else
        {
            AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
            SoundManager.best_sound = clip;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
