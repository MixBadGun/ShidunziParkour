using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class SkinLoader : MonoBehaviour
{
    public Texture2D track_texture;
    public Texture2D obstacle_texture;
    public Texture2D main_obstacle_texture;
    public Texture2D golden_obstacle_texture;
    public Texture2D show_obstacle_texture;
    public Texture2D[] bandeng_texture;
    public Texture2D side_edge_texture;
    public Texture2D side_floor_texture;
    public Texture2D fence_texture;

    string dataFolder;

    void Awake()
    {
        LoadSkin("Default Classic", true);
        LoadSkin(DataStorager.settings.skinPath);
    }

    void LoadSkin(string select_skin_path, bool skip_beatmap_skin = false)
    {
        dataFolder = $"{Application.persistentDataPath}/skin";

        string skin_path = $"{dataFolder}/{select_skin_path}";

        bool isCustomSkin = false;

        if (!skip_beatmap_skin && BeatmapInfo.musicMode == true){
            if (!DataStorager.settings.notBeatmapSkin)
            {
                string beatmap_skin_path = $"{Application.persistentDataPath}/music/{BeatmapInfo.anBeatmapInfo.path}/skin";
                string sec_beatmap_skin_path = $"{Application.persistentDataPath}/music/{BeatmapInfo.anBeatmapInfo.refer_path}/skin";
                if (Directory.Exists(beatmap_skin_path))
                {
                    skin_path = beatmap_skin_path;
                    isCustomSkin = true;
                }
                else if (Directory.Exists(sec_beatmap_skin_path))
                {
                    skin_path = sec_beatmap_skin_path;
                    isCustomSkin = true;
                }
            }
        }

        if (!isCustomSkin)
            {
                if (select_skin_path == "")
                {
                    return;
                }
                if (!Directory.Exists(skin_path))
                {
                    return;
                }
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

        if(File.Exists($"{skin_path}/main_obstacle.png")){
            byte[] fileData = File.ReadAllBytes($"{skin_path}/main_obstacle.png");
            main_obstacle_texture.LoadImage(fileData);
        }

        if(File.Exists($"{skin_path}/golden_obstacle.png")){
            byte[] fileData = File.ReadAllBytes($"{skin_path}/golden_obstacle.png");
            golden_obstacle_texture.LoadImage(fileData);
        }

        if(File.Exists($"{skin_path}/show_obstacle.png")){
            byte[] fileData = File.ReadAllBytes($"{skin_path}/show_obstacle.png");
            show_obstacle_texture.LoadImage(fileData);
        }

        int max_bandeng_skin_index = -1;

        while(true){
            if(File.Exists($"{skin_path}/side_building_{max_bandeng_skin_index + 1}.png")){
                max_bandeng_skin_index += 1;
            } else {
                break;
            }
        }

        for(int i = 0;i < bandeng_texture.Count();i++){
            if(max_bandeng_skin_index < 0){
                if(File.Exists($"{skin_path}/side_building.png")){
                    byte[] fileData = File.ReadAllBytes($"{skin_path}/side_building.png");
                    bandeng_texture[i].LoadImage(fileData);
                }
                continue;
            }
            int h = i % (max_bandeng_skin_index + 1);
            byte[] fileData2 = File.ReadAllBytes($"{skin_path}/side_building_{h}.png");
            bandeng_texture[i].LoadImage(fileData2);
        }

        if(File.Exists($"{skin_path}/side_edge.png")){
            byte[] fileData = File.ReadAllBytes($"{skin_path}/side_edge.png");
            side_edge_texture.LoadImage(fileData);
        }

        if(File.Exists($"{skin_path}/side_floor.png")){
            byte[] fileData = File.ReadAllBytes($"{skin_path}/side_floor.png");
            side_floor_texture.LoadImage(fileData);
        }
        
        if(File.Exists($"{skin_path}/fence.png")){
            byte[] fileData = File.ReadAllBytes($"{skin_path}/fence.png");
            fence_texture.LoadImage(fileData);
        }

        if (File.Exists($"{skin_path}/crash.wav"))
        {
            StartCoroutine(LoadBoomSound($"file://{skin_path}/crash.wav", AudioType.WAV));
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
            foreach (var sound in FindObjectsOfType<CrashSound>()) {
                sound.UpdateSound();
            }
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
            foreach (var sound in FindObjectsOfType<BestSound>()) {
                sound.UpdateSound();
            }
        }
    }
}
