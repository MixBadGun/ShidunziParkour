using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using static LoadMaplist;
using static SteamMyUGC;

public class SteamUploadPanel : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField title;
    [SerializeField]
    private TMP_InputField description;
    [SerializeField]
    private TMP_InputField mapper;
    [SerializeField]
    private TMP_InputField author;
    [SerializeField]
    private DifficultyToggle difficultyToggleTemplate;
    [SerializeField]
    private Transform difficultyToggleParent;
    Dictionary<AnBeatmapInfo, DifficultyToggle> selectedDifficulties = new();

    public void LoadUploadPanel()
    {
        selectedDifficulties.Clear();
        title.enabled = false;
        title.text = BeatmapInfo.LoadBeatmapInfos[0].title;
        mapper.enabled = false;
        mapper.text = BeatmapInfo.LoadBeatmapInfos[0].mapper;
        author.enabled = false;
        author.text = BeatmapInfo.LoadBeatmapInfos[0].author;
        while (difficultyToggleParent.childCount > 0)
        {
            DestroyImmediate(difficultyToggleParent.GetChild(0).gameObject);
        }
        foreach (AnBeatmapInfo beatmapInfo in BeatmapInfo.LoadBeatmapInfos)
        {
            var diffToggle = Instantiate(difficultyToggleTemplate, difficultyToggleParent);
            diffToggle.gameObject.SetActive(true);
            if (string.IsNullOrEmpty(beatmapInfo.refer_path))
            {
                diffToggle.SetLabelAndToggle($"{beatmapInfo.difficulty} {beatmapInfo.level}", true);
            }
            else
            {
                diffToggle.SetLabelAndToggle($"{beatmapInfo.difficulty} {beatmapInfo.level}");
            }
            selectedDifficulties.Add(beatmapInfo, diffToggle);
        }
    }

    public void EnterUpload()
    {
        string dataFolder = $"{Application.persistentDataPath}/music";
        string previewImagePath = $"{dataFolder}/{selectedDifficulties.First().Key.path}/bg.png";
        if (!File.Exists(previewImagePath))
        {
            previewImagePath = $"{dataFolder}/{selectedDifficulties.First().Key.refer_path}/bg.png";
        }
        UGCData uGCData = new()
        {
            Title = title.text,
            Description = description.text,
            previewImagePath = previewImagePath,
            beatmapPathNames = selectedDifficulties.Where(kv => kv.Value.IsOn()).Select(kv => kv.Key.path).ToArray(),
            tags = new string[] { "Beatmap" }
        };
        SteamMyUGC.UploadItem(uGCData);
        MessageSystem.SendInfo("上传中...");
        TurnOffPanel();
    }

    public void OpenURL(string url)
    {
        Application.OpenURL(url);
    }

    public void TurnOffPanel()
    {
        gameObject.SetActive(false);
    }
}
