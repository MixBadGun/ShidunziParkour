using System.Collections.Generic;
using System.IO;
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
        string beat_path = $"{Application.persistentDataPath}/music/{BeatmapInfo.beatmap_name}/data.sdz";
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
            }
            if (data[0].Trim() == "mass")
            {
                Level.text = data[1].Trim();
                continue;
            }
            if (data[0].Trim() == "difficulty")
            {
                levelImage.sprite = LevelPresents[(int)BeatmapManager.GetDifficulty(data[1].Trim())];
            }
        }
        description.text = $"曲师：{author}\n谱师：{mapper}";

        string record_path = $"{Application.persistentDataPath}/record/{BeatmapInfo.beatmap_name}.dat";
        while (recordItemFather.transform.childCount > 0)
        {
            DestroyImmediate(recordItemFather.transform.GetChild(0).gameObject);
        }
        if (File.Exists(record_path))
        {
            var data_list = JsonConvert.DeserializeObject<List<BeatmapManager.BeatmapResult>>(File.ReadAllText(record_path));
            foreach (BeatmapManager.BeatmapResult result in data_list)
            {
                GameObject newItem = Instantiate(recordItem, recordItemFather.transform);
                newItem.SetActive(true);
                newItem.GetComponent<RecordItem>().beatmapResult = result;
            }
        }

        string ad_data_path = $"{Application.persistentDataPath}/music/{BeatmapInfo.beatmap_name}/ad.dat";

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
                adbar.GetComponent<Adbar>().imagePath = $"{Application.persistentDataPath}/music/{BeatmapInfo.beatmap_name}/" + adInfo.imagePath;
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
