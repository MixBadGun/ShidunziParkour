using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System;
public class DataManager
{
    public static string dataFolder = $"{Application.persistentDataPath}/save";

    public static bool IsDataed(string filename)
    {
        string path = $"{dataFolder}/{filename}.dat";
        return File.Exists(path);
    }

    public static void Save<T>(string filename, T data)
    {
        string path = $"{dataFolder}/{filename}.dat";
        if (!Directory.Exists(dataFolder))
        {
            Directory.CreateDirectory(dataFolder);
        }
        var jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);

        File.WriteAllText(path, jsonData);
    }

    public static T Load<T>(string filename)
    {
        string path = $"{dataFolder}/{filename}.dat";
        if (!File.Exists(path))
        {
            return default;
        }
        var jsonData = JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
        return jsonData;
    }

    public struct Item
    {
        public string name;
        public int count;
    }

    public struct ConnectionInfo
    {
        public string ip;
        public ushort port;
        public string playerID;
    }

    public struct DunziSettings
    {
        public float SoundVolume;
        public float MusicVolume;
        public bool hasMotionBlur;
        public int CustomMaxLife;
        public bool notShake;
        public float MusicGameSpeed;
        public bool isAutoPlay;
        public bool notVibrate;
        public int offsetMs;
        public bool notBoomFX;
        public bool notBeatmapSkin;
        public bool relaxMod;
        public bool cinemaMod;
        public string skinPath;
        public bool fpsDisplay;
        public int fpsLimit;
        public bool hideKeyDisplay;
        public bool fixedCameraMod;
        public bool useGamepad;
        public TouchControlMode touchControlMode;
    }

    public struct KeySettings
    {
        public KeyCode[] esc;
        public KeyCode[] left;
        public KeyCode[] right;
        public KeyCode[] up;
        public KeyCode[] down;
        public KeyCode[] pad1;
        public KeyCode[] pad2;
        public KeyCode[] pad3;
    }

    public struct HistoryRecord
    {
        public bool isOldPlayer;
        public bool isHaveNewSkin;
    }

    public enum TouchControlMode
    {
        TAP,
        SLIDE
    }
}
