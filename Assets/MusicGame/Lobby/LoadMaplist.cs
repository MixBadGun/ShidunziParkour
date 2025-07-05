using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using kcp2k;
using Mirror;
using Mirror.Discovery;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static LevelDisplayer;

public class LoadMaplist : MonoBehaviour
{
    public static LoadMaplist instance;
    public struct AnBeatmapInfo
    {
        public string path;
        public string title;
        public string author;
        public string mapper;
        public float BPM;
        public int level;
        public Difficulty difficulty;
    }

    private string dataFolder;
    private Dictionary<string, List<AnBeatmapInfo>> beatmapInfos = new();

    public GameObject SingleItem;
    public GameObject MapList;
    public GameObject EmptyInfo;
    public GameObject t_DisplayPanel;
    public NetworkManager networkManager;
    public TMP_InputField networkIP;
    public TMP_InputField networkPort;
    public TMP_Dropdown networkDropdown;
    public static GameObject DisplayPanel;

    static bool isDeleteState = false;
    public Text deleteStateButtonText;

    private void Awake()
    {
        UpdateOldRecord();

        instance = this;
        DisplayPanel = t_DisplayPanel;
        dataFolder = $"{Application.persistentDataPath}/music";
        if (!Directory.Exists(dataFolder))
        {
            Directory.CreateDirectory(dataFolder);
        }
        string[] subfolderPaths = Directory.GetDirectories(dataFolder, "*", SearchOption.TopDirectoryOnly);
        foreach (string path in subfolderPaths)
        {
            if (!File.Exists($"{path}/data.sdz"))
            {
                continue;
            }
            string folderName = Path.GetFileName(path);
            string beat_path = $"{path}/data.sdz";
            AnBeatmapInfo info = new()
            {
                path = folderName,
                level = 0,
                difficulty = Difficulty.NONE
            };
            foreach (string line in File.ReadAllText(beat_path).Split("\n"))
            {
                string[] data = line.Split("=");
                if (data[0].Trim() == "title")
                {
                    info.title = data[1].Trim();
                    continue;
                }
                if (data[0].Trim() == "bpm")
                {
                    info.BPM = float.Parse(data[1].Trim());
                    continue;
                }
                if (data[0].Trim() == "author")
                {
                    info.author = data[1].Trim();
                    continue;
                }
                if (data[0].Trim() == "mapper")
                {
                    info.mapper = data[1].Trim();
                    continue;
                }
                if (data[0].Trim() == "level")
                {
                    info.level = (int)(float.Parse(data[1].Trim()) / 15 * 100000);
                    continue;
                }
                if (data[0].Trim() == "mass")
                {
                    info.level = int.Parse(data[1].Trim());
                    continue;
                }
                if (data[0].Trim() == "difficulty")
                {
                    info.difficulty = BeatmapManager.GetDifficulty(data[1].Trim());
                    continue;
                }
            }
            string identify_key = info.title + info.author;
            if (!beatmapInfos.ContainsKey(identify_key))
            {
                beatmapInfos.Add(identify_key, new());
            }
            beatmapInfos[identify_key].Add(info);
            static int sortComparison(AnBeatmapInfo x, AnBeatmapInfo y) => x.level.CompareTo(y.level);
            beatmapInfos[identify_key].Sort(sortComparison);
        }
        bool init = false;
        foreach (List<AnBeatmapInfo> infos in beatmapInfos.Values)
        {
            GameObject item;
            var info = infos[0];
            if (!init)
            {
                init = true;
            }
            item = Instantiate(SingleItem, MapList.transform);
            item.SetActive(true);
            item.GetComponent<SingleBeatmapInfo>().beatmapInfos = infos;
            item.GetComponent<SingleBeatmapInfo>().diff_index = 0;
            item.GetComponent<SingleBeatmapInfo>().LoadBeatmapInfo();
            if (File.Exists($"{dataFolder}/{info.path}/special_bar.png"))
            {
                byte[] fileData = File.ReadAllBytes($"{dataFolder}/{info.path}/special_bar.png");
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(fileData);
                item.GetComponent<SingleBeatmapInfo>().SetBackground(texture, 1);
            }
            else if (File.Exists($"{dataFolder}/{info.path}/left_special_bar.png"))
            {
                byte[] fileData = File.ReadAllBytes($"{dataFolder}/{info.path}/left_special_bar.png");
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(fileData);
                item.GetComponent<SingleBeatmapInfo>().SetBackground(texture, 1, SingleBeatmapInfo.Margin_Type.LEFT);
            }
            else if (File.Exists($"{dataFolder}/{info.path}/right_special_bar.png"))
            {
                byte[] fileData = File.ReadAllBytes($"{dataFolder}/{info.path}/right_special_bar.png");
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(fileData);
                item.GetComponent<SingleBeatmapInfo>().SetBackground(texture, 1, SingleBeatmapInfo.Margin_Type.RIGHT);
            }
            else if (File.Exists($"{dataFolder}/{info.path}/bg.png"))
            {
                byte[] fileData = File.ReadAllBytes($"{dataFolder}/{info.path}/bg.png");
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(fileData);
                item.GetComponent<SingleBeatmapInfo>().SetBackground(texture);
            }
        }
        // 无谱面则隐藏
        if (!init)
        {
            EmptyInfo.SetActive(true);
        }
    }

    void Update()
    {
        if (!isDeleteState)
        {
            deleteStateButtonText.text = "删除谱面";
        }
        else
        {
            deleteStateButtonText.text = "取消删除";
        }
    }

    public void OpenDisplayPanel()
    {
        networkManager.GetComponent<NetworkDis>().ClearUrls();
        networkDropdown.ClearOptions();
        networkManager.GetComponent<NetworkDiscovery>().StartDiscovery();
        DisplayPanel.SetActive(true);
        DisplayPanel.GetComponent<DisplayPanel>().LoadPanel();
    }

    public static void TurnOffDisplayPanel()
    {
        DisplayPanel.SetActive(false);
    }

    public static void GameStart()
    {
        BeatmapInfo.record_identity = "";
        SceneManager.LoadScene("MusicGame");
    }

    public void SyncGameStart(bool host)
    {
        BeatmapInfo.record_identity = "";
        if (!int.TryParse(networkPort.text, out int port))
        {
            port = 4782;
        }
        if (networkIP.text.Length <= 0)
        {
            networkManager.networkAddress = "localhost";
        }
        else
        {
            networkManager.networkAddress = networkIP.text;
        }
        networkManager.GetComponent<KcpTransport>().port = (ushort)port;
        if (!host)
        {
            NetworkClient.Shutdown();
            networkManager.GetComponent<NetworkDiscovery>().StopDiscovery();
            networkManager.StartClient();
        }
        else
        {
            NetworkServer.Shutdown();
            networkManager.StartHost();
        }
    }

    public static bool IsDeleting()
    {
        return isDeleteState;
    }

    public static void ChangeDeleteState()
    {
        isDeleteState = !isDeleteState;
    }

    public void UpdateIpValue()
    {
        string[] address = networkDropdown.options[networkDropdown.value].text.Split(":");
        networkIP.text = address[0];
        networkPort.text = address[1];
    }

    // 将旧版记录转换为新版记录
    void UpdateOldRecord()
    {
        string path = $"{Application.persistentDataPath}/record/";
        if (!Directory.Exists(path))
        {
            return;
        }

        DirectoryInfo dirInfo = new(path);
        foreach (FileInfo file in dirInfo.GetFiles())
        {
            if (file.Name.Split(".").Last() == "dat")
            {
                ConvertRecord(Path.Join(file.DirectoryName, file.Name), file.Name.Split(".")[0]);
            }
        }
    }

    void ConvertRecord(string record_path, string target)
    {
        if (File.Exists(record_path))
        {
            var data_list = JsonConvert.DeserializeObject<List<BeatmapManager.BeatmapResult>>(File.ReadAllText(record_path));
            for (int i = 0; i < data_list.Count; i++)
            {
                var result = data_list[i];
                result.achieveTime *= TimeSpan.TicksPerSecond;
                var achieveTime = result.achieveTime;
                string path = $"{Application.persistentDataPath}/record/{target}";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                string save_target = $"{path}/{achieveTime}.dat";
                var jsonData = JsonConvert.SerializeObject(result, Formatting.Indented);
                File.WriteAllText(save_target, jsonData);
            }
            File.Delete(record_path);
        }
    }
}
