using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;
using static InputStruct;
using static LevelDisplayer;

public class BeatmapManager : MonoBehaviour
{
    public static BeatmapManager Instance;

    float OnPlayingTime = 0;
    float BeforeTime = 3;
    float iniOffset = 3;
    float BPM = 0;
    float offset = DataStorager.settings.offsetMs / 1000;
    float videoOffset = 0;
    float MaxPoint = 0;
    float NowPoint = 0;
    float NegaPoint = 0;
    float MaxPlusPoint = 0;
    float NowPlusPoint = 0;
    float NegaPlusPoint = 0;
    int Combo = 0;
    int MaxCombo = 0;
    int FullCombo = 0;

    int placed_count = 0;

    public struct Point_Detail
    {
        public int perfect;
        public int great;
        public int miss;
        public int break_p;
        public int break_g;
        public int break_m;
    }

    Point_Detail point_detail;

    bool isPlaying = false;
    bool hasVideo = false;
    bool isVideoPlaying = false;
    bool isEnd = false;
    bool isSaved = false;
    bool isAutoPlay = DataStorager.settings.isAutoPlay;
    bool isPractcing = false;
    bool isViewingRecord = false;
    float distance = 10;
    public Player Player;
    public GameObject[] ObstacleList;
    public AudioSource MusicPlayer;
    public RawImage BackForVideo;
    public RawImage BackForImage;
    public RawImage BackForVideo2;
    public RawImage BackForImage2;
    public VideoPlayer videoPlayer;
    public GameObject ComboDisplay;
    public GameObject ResultCanvas;
    public GameObject AutoPlayImage;
    public GameObject CinemaImage;
    public GameObject PracticingImage;
    public GameObject RelaxModImage;
    public GameObject ViewRecordImage;
    public Animator ShowFrontVideo;
    public Animator MapInfo;
    public GameObject GetIntoButton;
    public GameObject PracticingObject;
    public GameObject noteParent;
    public ControlDisplay controlDisplay;

    // 谱面信息展示
    public RawImage DisplayInfoImage;
    public TMP_Text DisplayInfoText;
    public LevelDisplayer levelDisplayer;

    public MusicCamera camera;
    public LandGenerator landGenerator;
    public GameObject PauseUI;

    // 自动游玩变量
    bool last_record = false;
    float last_change_time;
    float should_change_time;
    bool ready_to_change_bpm = false;
    bool ready_to_change_hidden = false;
    bool ready_to_change_camera = false;
    List<float> should_change_bpm = new();
    List<float> should_change_bpm_time = new();
    List<float> should_change_hidden_time = new();
    List<CameraData> should_change_camera_data = new();
    List<float> should_change_camera_time = new();
    float autoShift = 0.0f;

    List<Play_Record> play_records = new();

    public void AddPlayRecord(MoveType moveType, KeyCode keyCode = KeyCode.None)
    {
        if (keyCode != KeyCode.None
            && !DataStorager.settings.hideKeyDisplay)
        {
            controlDisplay.TriggerKey(keyCode);
        }
        if (isViewingRecord)
            {
                return;
            }
        play_records.Add(new()
        {
            control_time = GetRealPlayingTime(),
            keyCode = keyCode,
            moveType = moveType
        });
    }

    string dataFolder;

    enum B_TYPE
    {
        BEAT_TYPE,
        BEST_BEAT_TYPE,
        GAINT_BEAT_TYPE,
        BPM_TYPE,
        HIDE_FRONT_TYPE,
        SHOW_BEAT_TYPE,
        CAMERA_TYPE,
        FINISH,
    }

    public class CameraData
    {
        public float cross_time = 0;
        public float z_angle = 0;
        public float y_angle = 0;
        public float x_angle = 0;
        public float z_pos = 0;
        public float y_pos = 0;
        public float x_pos = 0;
        public int ease_type = 1;

        public static CameraData operator *(double a, CameraData b)
        {
            CameraData r = new()
            {
                cross_time = b.cross_time,
                z_angle = (float)(b.z_angle * a),
                y_angle = (float)(b.y_angle * a),
                x_angle = (float)(b.x_angle * a),
                z_pos =   (float)(b.z_pos * a),
                y_pos =   (float)(b.y_pos * a),
                x_pos =   (float)(b.x_pos * a),
                ease_type = b.ease_type
            };
            return r;
        }

        public static CameraData operator +(CameraData a, CameraData b)
        {
            CameraData r = new()
            {
                cross_time = Math.Max(a.cross_time, b.cross_time),
                z_angle = a.z_angle + b.z_angle,
                y_angle = a.y_angle + b.y_angle,
                x_angle = a.x_angle + b.x_angle,
                z_pos =   a.z_pos   + b.z_pos  ,
                y_pos =   a.y_pos   + b.y_pos  ,
                x_pos =   a.x_pos   + b.x_pos  ,
                ease_type = b.ease_type
            };
            return r;
        }
    }

    struct SingleBeat
    {
        public B_TYPE type;
        public float beat_time;
        public float track;
        public int stack;
        public int rem_stack;
        public float size;
        public float y_offset;
        public float BPM;
        public CameraData camera_data;
    }

    private List<SingleBeat> remain_beats = new();
    private List<SingleBeat> auto_remain_beats = new();

    int CompareResult(float a, float b)
    {
        if (a < b) return -1;
        if (a > b) return 1;
        return 0;
    }

    public float getBPM()
    {
        return BPM;
    }

    float getValue(string value, float default_value = 0)
    {
        string trimed = value.Trim();
        if (trimed.Length == 0)
        {
            return default_value;
        }
        return float.Parse(value);
    }

    float getValueWithTry(string[] values, int index, float default_value = 0)
    {
        if (values.Count() > index)
        {
            return getValue(values[index], default_value);
        }
        return default_value;
    }

    public static Difficulty GetDifficulty(string difficulty_string)
    {
        return difficulty_string[0] switch
        {
            'S' => Difficulty.SO_POWERFUL,
            's' => Difficulty.SO_POWERFUL,
            'P' => Difficulty.POWERFUL,
            'p' => Difficulty.POWERFUL,
            'H' => Difficulty.HARD,
            'h' => Difficulty.HARD,
            'N' => Difficulty.NORMAL,
            'n' => Difficulty.NORMAL,
            'E' => Difficulty.EASY,
            'e' => Difficulty.EASY,
            'F' => Difficulty.FUN,
            'f' => Difficulty.FUN,
            _ => Difficulty.NONE,
        };
    }

    int getIntValue(string value, int default_value = 0)
    {
        string trimed = value.Trim();
        if (trimed.Length == 0)
        {
            return default_value;
        }
        return int.Parse(value);
    }

    void LoadResource(string beatmap_name)
    {
        if (File.Exists($"{dataFolder}/{beatmap_name}/music.wav"))
        {
            StartCoroutine(LoadMusic($"file://{dataFolder}/{beatmap_name}/music.wav", AudioType.WAV));
        }
        else if (File.Exists($"{dataFolder}/{beatmap_name}/music.mp3"))
        {
            StartCoroutine(LoadMusic($"file://{dataFolder}/{beatmap_name}/music.mp3", AudioType.MPEG));
        }
        else if (File.Exists($"{dataFolder}/{beatmap_name}/music.ogg"))
        {
            StartCoroutine(LoadMusic($"file://{dataFolder}/{beatmap_name}/music.ogg", AudioType.OGGVORBIS));
        }
        // 读取图片或视频
        if (File.Exists($"{dataFolder}/{beatmap_name}/bg.mp4"))
        {
            videoPlayer.targetTexture = (RenderTexture)BackForVideo.texture;
            videoPlayer.playOnAwake = false;
            videoPlayer.url = $"file://{dataFolder}/{beatmap_name}/bg.mp4";
            hasVideo = true;
        }
        else
        {
            BackForVideo.GameObject().SetActive(false);
        }
        if (File.Exists($"{dataFolder}/{beatmap_name}/bg.png"))
        {
            byte[] fileData = File.ReadAllBytes($"{dataFolder}/{beatmap_name}/bg.png");
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(fileData); // 自动调整纹理大小
            BackForImage.texture = texture;
            BackForImage2.texture = texture;
            BackForImage.GetComponent<AspectRatioFitter>().aspectRatio = (float)texture.width / texture.height;
            BackForImage2.GetComponent<AspectRatioFitter>().aspectRatio = (float)texture.width / texture.height;
            DisplayInfoImage.texture = texture;
            DisplayInfoImage.GetComponent<AspectRatioFitter>().aspectRatio = (float)texture.width / texture.height;
        }
    }

    public void LoadData(string beatmap_name)
    {
        // 读取谱面
        string path = $"{dataFolder}/{beatmap_name}/data.sdz";
        if (!Directory.Exists(dataFolder))
        {
            Directory.CreateDirectory(dataFolder);
        }
        float last_time = 0;

        List<SingleBeat> storage_beats = new();

        foreach (string line in File.ReadAllText(path).Split("\n"))
        {
            string[] data = line.Split("=");
            if (data[0].Trim() == "bpm")
            {
                BPM = getValue(data[1], 120);
                remain_beats.Add(
                    new SingleBeat()
                    {
                        type = B_TYPE.BPM_TYPE,
                        beat_time = 0,
                        BPM = getValue(data[1], 120)
                    }
                );
                continue;
            }
            if (data[0].Trim() == "offset")
            {
                offset = DataStorager.settings.offsetMs / 1000 + getValue(data[1]);
                continue;
            }
            if (data[0].Trim() == "bg_offset")
            {
                videoOffset = getValue(data[1]);
                continue;
            }
            if (data[0].Trim() == "title")
            {
                DisplayInfoText.text = data[1].Trim();
                continue;
            }
            if (data[0].Trim() == "level")
            {
                levelDisplayer.level = (int)(getValue(data[1]) / 15 * 100000);
                continue;
            }
            if (data[0].Trim() == "mass")
            {
                levelDisplayer.level = getIntValue(data[1]);
                continue;
            }
            if (data[0].Trim() == "difficulty")
            {
                levelDisplayer.difficulty = GetDifficulty(data[1].Trim());
                continue;
            }

            data = line.Split(",");
            // 石墩音符
            if ("SDX".Contains(data[0]) && data[0].Length > 0)
            {
                float slice_beat = getValue(data[3]) > 0 ? getValue(data[2]) / getValue(data[3]) : 0;
                float beat_time = last_time + (getValue(data[1]) + slice_beat) * (60 / BPM) + offset;
                int stack_count = getIntValue(data[5], 1);
                int rem_stack = 0;
                if (data.Count() >= 7)
                {
                    rem_stack = getIntValue(data[6], 0);
                }

                if (stack_count - rem_stack <= 0)
                {
                    continue;
                }

                float size = 1;
                if (data.Count() >= 8)
                {
                    size = getValue(data[7], 1);
                }
                float y_offset = 0;
                if (data.Count() >= 9)
                {
                    y_offset = getValue(data[8], 0);
                }

                B_TYPE type = 0;
                switch (data[0])
                {
                    case "S": type = B_TYPE.SHOW_BEAT_TYPE; break;
                    case "D": type = B_TYPE.BEAT_TYPE; break;
                    case "X": type = B_TYPE.BEST_BEAT_TYPE; break;
                }
                storage_beats.Add(
                    new SingleBeat()
                    {
                        type = type,
                        beat_time = beat_time,
                        track = getValue(data[4], 2),
                        stack = stack_count,
                        rem_stack = rem_stack,
                        size = size,
                        y_offset = y_offset
                    }
                );

                if ("DX".Contains(data[0]))
                {
                    MaxPoint += stack_count - rem_stack;
                    FullCombo += stack_count - rem_stack;
                    if (data[0] == "X")
                    {
                        MaxPlusPoint += stack_count - rem_stack;
                    }
                }
                continue;
            }
            if (data[0] == "H")
            {
                float slice_beat = getValue(data[3]) > 0 ? getValue(data[2]) / getValue(data[3]) : 0;
                float beat_time = last_time + (getValue(data[1]) + slice_beat) * (60 / BPM) + offset;
                storage_beats.Add(
                    new SingleBeat()
                    {
                        type = B_TYPE.HIDE_FRONT_TYPE,
                        beat_time = beat_time,
                    }
                );
                continue;
            }
            if (data[0] == "C")
            {
                float slice_beat = getValue(data[3]) > 0 ? getValue(data[2]) / getValue(data[3]) : 0;
                float beat_time = last_time + (getValue(data[1]) + slice_beat) * (60 / BPM) + offset;
                float camera_cross_slice_beat = getValue(data[7]) > 0 ? getValue(data[6]) / getValue(data[7]) : 0;
                CameraData cameraData = new()
                {
                    cross_time = (getValue(data[5], 0.5f) + camera_cross_slice_beat) * (60 / BPM),
                    z_angle = getValueWithTry(data, 8, 0),
                    y_angle = getValueWithTry(data, 9, 0),
                    x_angle = getValueWithTry(data, 10, 0),
                    z_pos = getValueWithTry(data, 11, 0),
                    y_pos = getValueWithTry(data, 12, 0),
                    x_pos = getValueWithTry(data, 13, 0),
                    ease_type = getIntValue(data[4], 1)
                };
                storage_beats.Add(
                    new SingleBeat()
                    {
                        type = B_TYPE.CAMERA_TYPE,
                        beat_time = beat_time,
                        camera_data = cameraData
                    }
                );
                continue;
            }
            // BPM 是刷新 storage_beats 并存入的标志。
            if (data[0] == "B")
            {
                float slice_beat = getValue(data[3]) > 0 ? getValue(data[2]) / getValue(data[3]) : 0;
                float beat_time = last_time + (getValue(data[1]) + slice_beat) * (60 / BPM) + offset;
                last_time = beat_time - offset;
                BPM = getValue(data[4], 120);
                storage_beats.Add(
                    new SingleBeat()
                    {
                        type = B_TYPE.BPM_TYPE,
                        beat_time = beat_time,
                        BPM = getValue(data[4], 120)
                    }
                );
                storage_beats.Sort((x, y) => CompareResult(x.beat_time, y.beat_time));
                remain_beats.AddRange(storage_beats);
                storage_beats.Clear();
                continue;
            }
        }
        // 最后再加一次。
        storage_beats.Sort((x, y) => CompareResult(x.beat_time, y.beat_time));
        remain_beats.AddRange(storage_beats);
        storage_beats.Clear();
    }

    IEnumerator LoadMusic(string path, AudioType audioType)
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
            MusicPlayer.clip = clip;
        }
    }

    // IEnumerator LoadVideo(string path)
    // {
    //     using UnityWebRequest www = UnityWebRequest.Get(path);
    //     yield return www.SendWebRequest();

    //     if (www.result == UnityWebRequest.Result.ConnectionError)
    //     {
    //         Debug.Log(www.error);
    //     }
    //     else
    //     {
    //         videoPlayer.url = path;
    //     }
    // }

    void LoadViewRecordData()
    {
        string record_file = $"{Application.persistentDataPath}/record/{BeatmapInfo.beatmap_name}/{BeatmapInfo.record_identity}";

        if (!File.Exists(record_file))
        {
            return;
        }

        play_records = JsonConvert.DeserializeObject<BeatmapResult>(File.ReadAllText(record_file)).play_records;

        if (play_records == null)
        {
            return;
        }

        Player.moveToIndex(2);
        while (play_records.Count > 0 && play_records[0].control_time < GetRealPlayingTime())
        {
            switch (play_records[0].moveType)
            {
                case MoveType.MOVE_LEFT: Player.moveLeft(); break;
                case MoveType.MOVE_RIGHT: Player.moveRight(); break;
            }
            play_records.RemoveAt(0);
        }
    }

    private void Awake()
    {
        Instance = this;

        Application.targetFrameRate = DataStorager.settings.fpsLimit > 0 ? DataStorager.settings.fpsLimit : 300;

        dataFolder = $"{Application.persistentDataPath}/music";
        LoadResource(BeatmapInfo.beatmap_name);

        if (BeatmapInfo.record_identity != "")
        {
            isViewingRecord = true;
            ViewRecordImage.SetActive(true);
        }

        LoadBeatmap();
        ComboDisplay.SetActive(false);
        ResultCanvas.SetActive(false);

        if (isViewingRecord)
        {
            isAutoPlay = false;
        }

        if (!isAutoPlay)
        {
            AutoPlayImage.SetActive(false);
        }
        if (!DataStorager.settings.relaxMod)
        {
            RelaxModImage.SetActive(false);
        }
        if (!DataStorager.settings.cinemaMod)
        {
            CinemaImage.SetActive(false);
        }
    }

    private void LoadBeatmap(float start_time = 0)
    {
        if (isViewingRecord)
        {
            LoadViewRecordData();
        }

        while (noteParent.transform.childCount > 0)
        {
            DestroyImmediate(noteParent.transform.GetChild(0).gameObject);
        }
        remain_beats.Clear();
        LoadData(BeatmapInfo.beatmap_name);
        remain_beats.Add(
            new SingleBeat()
            {
                type = B_TYPE.FINISH,
                track = 2
            }
        );

        for (int i = 0; i < remain_beats.Count; i++)
        {
            if (remain_beats[0].type == B_TYPE.FINISH)
            {
                break;
            }
            if (remain_beats[0].beat_time >= start_time)
            {
                break;
            }
            remain_beats.RemoveAt(0);
        }

        auto_remain_beats.Clear();
        auto_remain_beats.AddRange(remain_beats);
    }

    public void ReStart(float start_time)
    {
        ReloadFromTime(start_time);
        Time.timeScale = 1;
        MusicPlayer.time = start_time;
        MusicPlayer.Play();
    }

    public void ReloadFromTime(float start_time)
    {
        start_time = Math.Min(MusicPlayer.clip.length, Math.Abs(start_time));
        OnPlayingTime = start_time;
        Player.ChangePos();
        landGenerator.RespawnLand();
        LoadBeatmap(start_time);
        UpdateObstacle();
    }

    public void GetIntoPractice()
    {
        BeforeTime = 0;
        PracticingImage.SetActive(true);
        PracticingObject.SetActive(true);
        PracticingObject.GetComponent<PracticingBar>().SetFirstValue();
        isPractcing = true;
        MusicPlayer.Pause();
        Time.timeScale = 0;
        GetIntoButton.SetActive(false);
        PauseUI.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    B_TYPE[] detect_list = {
        B_TYPE.BEAT_TYPE,
        B_TYPE.BEST_BEAT_TYPE,
        B_TYPE.SHOW_BEAT_TYPE
    };


    bool Intersects(float a1, float b1, float a2, float b2)
    {
        return (a1 < b2) && (a2 < b1);
    }

    int[] toTouchTracks(float track, float size = 1)
    {
        List<int> move_tracks = new();
        float left_track = 2 * track - size;
        float right_track = 2 * track + size;
        for (int k = 1; k <= 3; k++)
        {
            if (Intersects(left_track, right_track, k * 2 - 1, k * 2 + 1))
            {
                move_tracks.Add(k);
            }
        }
        return move_tracks.ToArray();
    }

    public float GetPlayingTime()
    {
        return -BeforeTime + OnPlayingTime + iniOffset;
    }

    public float GetRealPlayingTime()
    {
        return -BeforeTime + OnPlayingTime;
    }

    public Point_Detail GetPointDetail()
    {
        return point_detail;
    }

    void UpdateObstacle()
    {
        while (detect_list.Contains(remain_beats[0].type) && remain_beats[0].beat_time - OnPlayingTime + BeforeTime < 5)
        {
            Vector3 place_pos;
            place_pos.z = (remain_beats[0].beat_time + iniOffset) * Player.GetVelocity();
            place_pos.x = (float)((remain_beats[0].track - 2) * 3);
            for (int i = remain_beats[0].rem_stack; i < remain_beats[0].stack; i++)
            {
                place_pos.y = remain_beats[0].y_offset + i * 2 * remain_beats[0].size;
                GameObject obs;
                switch (remain_beats[0].type)
                {
                    case B_TYPE.BEAT_TYPE:
                        {
                            obs = Instantiate(ObstacleList[0], noteParent.transform);
                            break;
                        }
                        ;
                    case B_TYPE.SHOW_BEAT_TYPE:
                        {
                            obs = Instantiate(ObstacleList[2], noteParent.transform);
                            obs.GetComponent<MusicObstacle>().setShowNote();
                            break;
                        }
                        ;
                    case B_TYPE.BEST_BEAT_TYPE:
                        {
                            obs = Instantiate(ObstacleList[1], noteParent.transform);
                            obs.GetComponent<MusicObstacle>().setBestNote();
                            break;
                        }
                        ;
                    default:
                        {
                            obs = Instantiate(ObstacleList[0], noteParent.transform);
                            break;
                        }
                }
                ;
                obs.GetComponent<MusicObstacle>().index = placed_count++;
                obs.GetComponent<MusicObstacle>().size = remain_beats[0].size;
                obs.GetComponent<MusicObstacle>().setNote();
                obs.GetComponent<MusicObstacle>().track = toTouchTracks(remain_beats[0].track, remain_beats[0].size);
                obs.transform.position = place_pos;
                obs.transform.localScale *= remain_beats[0].size;
                if (remain_beats[0].size != 1)
                {
                    obs.transform.localScale *= (float)4 / 3;
                }
                if (remain_beats[1].type == B_TYPE.FINISH
                    && !isPractcing)
                {
                    obs.GetComponent<MusicObstacle>().setLastNote();
                }
            }
            remain_beats.RemoveAt(0);
        }
        while (!detect_list.Contains(remain_beats[0].type) && remain_beats[0].type != B_TYPE.FINISH)
        {
            remain_beats.RemoveAt(0);
        }
    }

    void VideoHandle()
    {
        if (hasVideo && !isVideoPlaying)
        {
            if (-BeforeTime + OnPlayingTime >= videoOffset)
            {
                videoPlayer.Play();
                BackForVideo.GetComponent<AspectRatioFitter>().aspectRatio = (float)videoPlayer.width / videoPlayer.height;
                BackForVideo2.GetComponent<AspectRatioFitter>().aspectRatio = (float)videoPlayer.width / videoPlayer.height;
            }
        }
    }

    void MusicAndTimeAndSaveHandle()
    {
        if (BeforeTime > 0)
        {
            BeforeTime -= Time.fixedDeltaTime;
            return;
        }
        if (!isPlaying)
        {
            isPlaying = !isPlaying;
            MusicPlayer.Play();
        }
        if (isEnd && !isSaved)
        {
            ResultCanvas.SetActive(true);
            MapInfo.SetTrigger("ResultTrigger");
            if (!isAutoPlay
                && !DataStorager.settings.relaxMod
                // && !DataStorager.settings.fixedCameraMod
                && !DataStorager.settings.cinemaMod
                && !isPractcing
                && !(DateTime.Now.Day == 1 && DateTime.Now.Month == 4)
                && !isViewingRecord
            )
            {
                SaveResult();
            }
            isSaved = true;
        }
        if (MusicPlayer.isPlaying)
        {
            OnPlayingTime = MusicPlayer.time;
        }
        else
        {
            OnPlayingTime += Time.fixedDeltaTime;
        }
    }

    void FixedUpdate()
    {
        SyncplayHandle();
        UpdateObstacle();
        VideoHandle();
        MusicAndTimeAndSaveHandle();
    }

    public struct Play_Record
    {
        public float control_time;
        public KeyCode keyCode;
        public MoveType moveType;
    }

    public struct BeatmapResult
    {
        public int rating;
        public float achievement;
        public int maxCombo;
        public long achieveTime;
        public Point_Detail point_detail;
        public List<Play_Record> play_records;
    }

    enum Rating { SSSp, SSS, SSp, SS, Sp, S, AAA, AA, A, BBB, BB, B, C, D, F };

    void SyncplayHandle()
    {
        // 需要同步时间点处理的音符
        if (auto_remain_beats[0].type == B_TYPE.BPM_TYPE)
        {
            ready_to_change_bpm = true;
            should_change_bpm_time.Add(auto_remain_beats[0].beat_time);
            should_change_bpm.Add(auto_remain_beats[0].BPM);
            auto_remain_beats.RemoveAt(0);
        }
        if (auto_remain_beats[0].type == B_TYPE.HIDE_FRONT_TYPE)
        {
            ready_to_change_hidden = true;
            should_change_hidden_time.Add(auto_remain_beats[0].beat_time);
            auto_remain_beats.RemoveAt(0);
        }
        if (auto_remain_beats[0].type == B_TYPE.CAMERA_TYPE)
        {
            ready_to_change_camera = true;
            should_change_camera_time.Add(auto_remain_beats[0].beat_time);
            should_change_camera_data.Add(auto_remain_beats[0].camera_data);
            auto_remain_beats.RemoveAt(0);
        }
        if (ready_to_change_bpm)
        {
            if (OnPlayingTime - BeforeTime >= should_change_bpm_time[0])
            {
                BPM = should_change_bpm[0];
                should_change_bpm_time.RemoveAt(0);
                should_change_bpm.RemoveAt(0);
                if (should_change_bpm_time.Count <= 0)
                {
                    ready_to_change_bpm = false;
                }
            }
        }
        if (ready_to_change_hidden)
        {
            if (OnPlayingTime - BeforeTime >= should_change_hidden_time[0])
            {
                ShowFrontVideo.SetBool("ShowBool", !ShowFrontVideo.GetBool("ShowBool"));
                // Debug.Log(should_change_hidden_time.ToArray());
                should_change_hidden_time.RemoveAt(0);
                if (should_change_hidden_time.Count <= 0)
                {
                    ready_to_change_hidden = false;
                }
            }
        }
        if (ready_to_change_camera)
        {
            if (OnPlayingTime - BeforeTime >= should_change_camera_time[0])
            {
                camera.triggerTransform(should_change_camera_data[0]);
                should_change_camera_time.RemoveAt(0);
                should_change_camera_data.RemoveAt(0);
                if (should_change_camera_time.Count <= 0)
                {
                    ready_to_change_camera = false;
                }
            }
        }

        // 自动游玩判断
        if (isAutoPlay)
        {
            if (!detect_list.Contains(auto_remain_beats[0].type))
            {
                return;
            }
            // 先判断是不是需要大跳
            if ((auto_remain_beats[0].stack > 1 || auto_remain_beats[0].y_offset > 1) && Player.GetPos().y < 0.01f)
            {
                float jump_should_remain_time = (float)Math.Sqrt(Math.Pow(2, (int)Math.Log(auto_remain_beats[0].stack * auto_remain_beats[0].size + auto_remain_beats[0].y_offset, 2) + 1) * 2 / Player.GetGravity());
                if (Player.GetPos().z / Player.GetComponent<Player>().GetVelocity() + jump_should_remain_time - autoShift > auto_remain_beats[0].beat_time + iniOffset)
                {
                    int jump_times = (int)Math.Log(auto_remain_beats[0].stack * auto_remain_beats[0].size + auto_remain_beats[0].y_offset, 2);
                    for (int k = 0; k < jump_times; k++)
                    {
                        Player.moveUp(KeyCode.UpArrow);
                    }
                }
            }
            int[] should_tracks = toTouchTracks(auto_remain_beats[0].track, auto_remain_beats[0].size);
            if (!should_tracks.Contains(Player.GetNowTrack()) && (should_tracks.Count() > 0))
            {
                if (!last_record)
                {
                    last_change_time = OnPlayingTime - BeforeTime;
                    last_record = true;
                    float switch_time = (auto_remain_beats[0].beat_time - last_change_time) * 1 / 2;
                    if (switch_time < 0.25)
                    {
                        switch_time = 0;
                    }
                    should_change_time = last_change_time + switch_time;
                }
                if (OnPlayingTime - BeforeTime >= should_change_time)
                {
                    int should_move_times = should_tracks[0] - Player.GetNowTrack();
                    // 移动
                    if (should_move_times > 0)
                    {
                        for (int j = 0; j < should_move_times; j++)
                        {
                            Player.moveRight(KeyCode.RightArrow);
                        }
                    }
                    else
                    {
                        for (int j = 0; j < -should_move_times; j++)
                        {
                            Player.moveLeft(KeyCode.LeftArrow);
                        }
                    }
                }
            }
        }
        while (Player.GetPos().z >= (auto_remain_beats[0].beat_time + iniOffset - autoShift) * Player.GetVelocity()
            && detect_list.Contains(auto_remain_beats[0].type))
        {
            int[] should_tracks = toTouchTracks(auto_remain_beats[0].track, auto_remain_beats[0].size);

            // 补足 Auto 的痛（
            if (!should_tracks.Contains(Player.GetNowTrack()) && (should_tracks.Count() > 0) && isAutoPlay)
            {
                int should_move_times = should_tracks[0] - Player.GetNowTrack();
                // 移动
                if (should_move_times > 0)
                {
                    for (int j = 0; j < should_move_times; j++)
                    {
                        Player.moveRight(KeyCode.RightArrow);
                    }
                }
                else
                {
                    for (int j = 0; j < -should_move_times; j++)
                    {
                        Player.moveLeft(KeyCode.LeftArrow);
                    }
                }
            }

            if (((auto_remain_beats[0].stack > 1 && Player.GetPos().y > 0.1) || (Player.GetPos().y > 1 + auto_remain_beats[0].y_offset)) && isAutoPlay)
            {
                Player.moveDown(KeyCode.DownArrow);
            }

            // 设置跨越速度
            if (detect_list.Contains(auto_remain_beats[1].type))
            {
                Player.setCrossTime(auto_remain_beats[1].beat_time - auto_remain_beats[0].beat_time);
            }

            auto_remain_beats.RemoveAt(0);
            last_record = false;
        }

        // 观看记录模式
        if (isViewingRecord)
        {
            if(play_records == null || play_records.Count == 0)
            {
                return;
            }
            while (play_records.Count > 0 && play_records[0].control_time <= GetRealPlayingTime())
            {
                switch (play_records[0].moveType)
                {
                    case MoveType.MOVE_LEFT:
                        Player.moveLeft(play_records[0].keyCode);
                        break;
                    case MoveType.MOVE_RIGHT:
                        Player.moveRight(play_records[0].keyCode);
                        break;
                    case MoveType.MOVE_UP:
                        Player.moveUp(play_records[0].keyCode);
                        break;
                    case MoveType.MOVE_DOWN:
                        Player.moveDown(play_records[0].keyCode);
                        break;
                }
                AddPlayRecord(MoveType.MOVE_INDEX, play_records[0].keyCode);
                play_records.RemoveAt(0);
            }
        }
    }

    void SaveResult()
    {
        long achieveTime = DateTime.UtcNow.Ticks;

        string path = $"{Application.persistentDataPath}/record/{BeatmapInfo.beatmap_name}";
        string save_target = $"{path}/{achieveTime}.dat";
        if (!Directory.Exists($"{Application.persistentDataPath}/record/"))
        {
            Directory.CreateDirectory($"{Application.persistentDataPath}/record/");
        }

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        BeatmapResult data = new()
        {
            rating = GetRating(),
            achievement = GetProgress() * 100,
            maxCombo = MaxCombo,
            achieveTime = achieveTime,
            point_detail = point_detail,
            play_records = play_records
        };

        var jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(save_target, jsonData);
    }

    public float GetProgress()
    {
        float plus_point = MaxPlusPoint > 0 ? NowPlusPoint / MaxPlusPoint : 1;
        return (float)((NowPoint + NowPlusPoint * 4) / (MaxPoint + MaxPlusPoint * 4) + plus_point * 0.01);
    }

    public float GetNegaProgress()
    {
        float nega_plus_point = MaxPlusPoint > 0 ? NegaPlusPoint / MaxPlusPoint : 0;
        return (float)(1.01 - (NegaPlusPoint * 4 + NegaPoint) / (MaxPoint + MaxPlusPoint * 4) - nega_plus_point * 0.01);
    }

    public float GetNegativeProgress()
    {
        float plus_point = MaxPlusPoint > 0 ? NowPlusPoint / MaxPlusPoint : 1;
        return (float)((NowPoint + plus_point * 4) / (MaxPoint + MaxPlusPoint * 4) + plus_point * 0.01);
    }

    public void triggerEnd()
    {
        isEnd = true;
    }

    public enum M_TYPE
    {
        Perfect,
        Great,
        Miss,
        Break_P,
        Break_G,
        Break_M
    }

    public void AddNowPoint(M_TYPE mtype, bool hasPoint = true)
    {
        if (!hasPoint)
        {
            return;
        }
        float point = 0;
        switch (mtype)
        {
            case M_TYPE.Perfect:
                {
                    point = 1;
                    point_detail.perfect += 1;
                    break;
                }
            case M_TYPE.Great:
                {
                    point = 0.95f;
                    point_detail.great += 1;
                    break;
                }
            case M_TYPE.Miss:
                {
                    point_detail.miss += 1;
                    break;
                }
        }
        if (point > 0)
        {
            Combo += 1;
            ComboDisplay.SetActive(true);
            ComboDisplay.GetComponent<Animator>().SetTrigger("NewCombo");
        }
        NowPoint += point;
        NegaPoint += 1 - point;
        MaxCombo = Math.Max(Combo, MaxCombo);
    }

    public void AddNowBest(M_TYPE mtype, bool hasPoint = true)
    {
        if (!hasPoint)
        {
            return;
        }
        float point = 0;
        switch (mtype)
        {
            case M_TYPE.Break_P:
                {
                    point = 1;
                    point_detail.break_p += 1;
                    break;
                }
            case M_TYPE.Break_G:
                {
                    point = 0.95f;
                    point_detail.break_g += 1;
                    break;
                }
            case M_TYPE.Break_M:
                {
                    point_detail.break_m += 1;
                    break;
                }
        }
        NowPlusPoint += point;
        NegaPlusPoint += 1 - point;
    }

    public void Miss()
    {
        Combo = 0;
        ComboDisplay.SetActive(false);
    }

    public int GetCombo()
    {
        return Combo;
    }

    public int GetMaxCombo()
    {
        return MaxCombo;
    }

    public int GetFullCombo()
    {
        return FullCombo;
    }

    int CalcRating(float progress)
    {
        if (progress <= 0)
        {
            return (int)Rating.F;
        }
        else if (progress < 0.5)
        {
            return (int)Rating.D;
        }
        else if (progress < 0.6)
        {
            return (int)Rating.C;
        }
        else if (progress < 0.7)
        {
            return (int)Rating.B;
        }
        else if (progress < 0.75)
        {
            return (int)Rating.BB;
        }
        else if (progress < 0.8)
        {
            return (int)Rating.BBB;
        }
        else if (progress < 0.9)
        {
            return (int)Rating.A;
        }
        else if (progress < 0.94)
        {
            return (int)Rating.AA;
        }
        else if (progress < 0.97)
        {
            return (int)Rating.AAA;
        }
        else if (progress < 0.98)
        {
            return (int)Rating.S;
        }
        else if (progress < 0.99)
        {
            return (int)Rating.Sp;
        }
        else if (progress < 0.995)
        {
            return (int)Rating.SS;
        }
        else if (progress < 1)
        {
            return (int)Rating.SSp;
        }
        else if (progress < 1.005)
        {
            return (int)Rating.SSS;
        }
        else
        {
            return (int)Rating.SSSp;
        }
    }

    public int GetRating()
    {
        return CalcRating(GetProgress());
    }

    public int GetNegaRating() {
        return CalcRating(GetNegaProgress());
    }
}
