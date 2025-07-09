using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DisplayPanel : MonoBehaviour
{
    public TMP_Text Level;
    public TMP_Text title;
    public TMP_Text description;
    public GameObject recordItem;
    public GameObject recordItemFather;
    public Image levelImage;
    public Sprite[] LevelPresents;
    public GameObject AdbarList;
    public GameObject AdbarTemplate;

    public void LoadPanel()
    {
        string beat_path = $"{Application.persistentDataPath}/music/{BeatmapInfo.anBeatmapInfo.path}/data.sdz";
        string author = "";
        string mapper = "";

        foreach (string line in File.ReadAllText(beat_path).Split("\n"))
        {
            string[] data = line.Split("=");
            if (data[0].Trim() == "title")
            {
                title.text = data[1].Trim();
                continue;
            }
            if (data[0].Trim() == "author")
            {
                author = data[1].Trim();
                continue;
            }
            if (data[0].Trim() == "mapper")
            {
                mapper = data[1].Trim();
                continue;
            }
            if (data[0].Trim() == "level")
            {
                Level.text = ((int)(float.Parse(data[1].Trim()) / 15 * 100000)).ToString();
                continue;
            }
            if (data[0].Trim() == "mass")
            {
                Level.text = data[1].Trim();
                continue;
            }
            if (data[0].Trim() == "difficulty")
            {
                levelImage.sprite = LevelPresents[(int)BeatmapManager.GetDifficulty(data[1].Trim())];
                continue;
            }
            if (data[0].Trim() == "[Data]")
            {
                break;
            }
        }
        description.text = $"曲师：{author}\n谱师：{mapper}";

        while (recordItemFather.transform.childCount > 0)
        {
            DestroyImmediate(recordItemFather.transform.GetChild(0).gameObject);
        }

        // 读取记录
        string new_path = $"{Application.persistentDataPath}/record/{BeatmapInfo.anBeatmapInfo.path}/";
        if (Directory.Exists(new_path))
        {
            DirectoryInfo dirInfo = new(new_path);
            foreach (FileInfo file in dirInfo.GetFiles())
            {
                if (file.Name.Split(".").Last() == "dat")
                {
                    var data = JsonConvert.DeserializeObject<BeatmapManager.BeatmapResult>(File.ReadAllText(Path.Join(file.DirectoryName, file.Name)));
                    GameObject newItem = Instantiate(recordItem, recordItemFather.transform);
                    newItem.SetActive(true);
                    newItem.GetComponent<RecordItem>().beatmapResult = data;
                    newItem.GetComponent<RecordItem>().record_identity = file.Name;
                    if (data.play_records == null)
                    {
                        newItem.GetComponent<RecordItem>().viewRecordButton.SetActive(false);
                    }
                }
            }
        }

        string ad_data_path = $"{Application.persistentDataPath}/music/{BeatmapInfo.anBeatmapInfo.path}/ad.dat";
        string load_path = BeatmapInfo.anBeatmapInfo.path;
        if (!File.Exists(ad_data_path))
        {
            ad_data_path = $"{Application.persistentDataPath}/music/{BeatmapInfo.anBeatmapInfo.refer_path}/ad.dat";
            load_path = BeatmapInfo.anBeatmapInfo.refer_path;
        }

        if (File.Exists(ad_data_path))
        {
            while (AdbarList.transform.childCount > 0)
            {
                DestroyImmediate(AdbarList.transform.GetChild(0).gameObject);
            }
            AdbarList.SetActive(true);
            List<AdInfo> ad_list = JsonConvert.DeserializeObject<List<AdInfo>>(File.ReadAllText(ad_data_path));
            for (int i = 0; i < ad_list.Count; i++)
            {
                AdInfo adInfo = ad_list[i];
                GameObject adbar = Instantiate(AdbarTemplate, AdbarList.transform);
                adbar.GetComponent<Adbar>().targetURL = adInfo.targetURL;
                adbar.GetComponent<Adbar>().additionalText = adInfo.additionalText;
                adbar.GetComponent<Adbar>().imagePath = $"{Application.persistentDataPath}/music/{load_path}/" + adInfo.imagePath;
                adbar.SetActive(true);
            }
        }
        else
        {
            AdbarList.SetActive(false);
        }
    }

    struct AdInfo
    {
        public string imagePath;
        public string additionalText;
        public string targetURL;
    }
}
