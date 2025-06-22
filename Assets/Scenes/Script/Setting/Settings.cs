using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    public GlobalSettings globalSettings;
    public Slider MusicVolume;
    public Slider SoundVolume;
    public Toggle hasMotionBlur;
    public Text MaxLife;
    public InputField CustomMaxLife;
    public Toggle notShake;
    public Toggle fpsDisplay;
    public InputField fpsLimit;
    public InputField MusicGameSpeed;
    public Toggle isAutoPlay;
    public Toggle notVibrate;
    public Toggle notBoomFX;
    public Toggle notBeatmapSkin;
    public Toggle RelaxMod;
    public Toggle CinemaMod;
    public Toggle HideKeyDisplay;
    public InputField MusicGameOffsetMs;
    public TMP_Dropdown SkinDropdown;
    List<string> subfolders;
    void Awake(){
        MusicVolume.value = DataStorager.settings.MusicVolume;
        SoundVolume.value = DataStorager.settings.SoundVolume;
        hasMotionBlur.isOn = DataStorager.settings.hasMotionBlur;
        notShake.isOn = DataStorager.settings.notShake;
        isAutoPlay.isOn = DataStorager.settings.isAutoPlay;
        notVibrate.isOn = DataStorager.settings.notVibrate;
        notBoomFX.isOn = DataStorager.settings.notBoomFX;
        notBeatmapSkin.isOn = DataStorager.settings.notBeatmapSkin;
        RelaxMod.isOn = DataStorager.settings.relaxMod;
        CinemaMod.isOn = DataStorager.settings.cinemaMod;
        HideKeyDisplay.isOn = DataStorager.settings.hideKeyDisplay;
        MaxLife.text = DataStorager.maxLife.count.ToString();
        MusicGameOffsetMs.text = DataStorager.settings.offsetMs.ToString();
        if(DataStorager.settings.CustomMaxLife > 0){
            CustomMaxLife.text = DataStorager.settings.CustomMaxLife.ToString();
        } else {
            CustomMaxLife.text = DataStorager.maxLife.count.ToString();
        }
        if(DataStorager.settings.MusicGameSpeed > 0){
            MusicGameSpeed.text = DataStorager.settings.MusicGameSpeed.ToString();
        } else {
            MusicGameSpeed.text = "1";
        }
        fpsDisplay.isOn = DataStorager.settings.fpsDisplay;
        fpsLimit.text = DataStorager.settings.fpsLimit.ToString();

        // 皮肤
        string skinFolder = $"{Application.persistentDataPath}/skin";
        string[] subfolderPaths = Directory.GetDirectories(skinFolder, "*", SearchOption.TopDirectoryOnly);
        for(int i = 0;i < subfolderPaths.Count(); i++){
            subfolderPaths[i] = Path.GetFileName(subfolderPaths[i]);
        }
        subfolders = new(subfolderPaths);
        SkinDropdown.AddOptions(subfolders);
        if(subfolders.Contains(DataStorager.settings.skinPath)){
            SkinDropdown.value = subfolders.IndexOf(DataStorager.settings.skinPath) + 1;
        } else {
            SkinDropdown.value = 0;
        }
    }

    public void SaveAndExit(){
        DataStorager.settings.MusicVolume = MusicVolume.value;
        DataStorager.settings.SoundVolume = SoundVolume.value;
        DataStorager.settings.hasMotionBlur = hasMotionBlur.isOn;
        DataStorager.settings.notShake = notShake.isOn;
        DataStorager.settings.notVibrate = notVibrate.isOn;
        DataStorager.settings.isAutoPlay = isAutoPlay.isOn;
        DataStorager.settings.notBoomFX = notBoomFX.isOn;
        DataStorager.settings.notBeatmapSkin = notBeatmapSkin.isOn;
        DataStorager.settings.relaxMod = RelaxMod.isOn;
        DataStorager.settings.cinemaMod = CinemaMod.isOn;
        DataStorager.settings.fpsDisplay = fpsDisplay.isOn;
        DataStorager.settings.hideKeyDisplay = HideKeyDisplay.isOn;
        DataStorager.settings.fpsLimit = int.TryParse(fpsLimit.text, out int fps) && fps > 0 ? fps : 300;

        if (!int.TryParse(CustomMaxLife.text, out int clife))
        {
            DataStorager.settings.CustomMaxLife = DataStorager.maxLife.count;
        }
        else
        {
            if (clife > 0 && clife <= DataStorager.maxLife.count)
            {
                DataStorager.settings.CustomMaxLife = clife;
            }
            else
            {
                DataStorager.settings.CustomMaxLife = DataStorager.maxLife.count;
            }
        }
        if (!float.TryParse(MusicGameSpeed.text, out float cspeed))
        {
            DataStorager.settings.MusicGameSpeed = 1;
        }
        else
        {
            if (cspeed > 0)
            {
                DataStorager.settings.MusicGameSpeed = cspeed;
            }
            else
            {
                DataStorager.settings.MusicGameSpeed = 1;
            }
        }
        if (!int.TryParse(MusicGameOffsetMs.text, out int coffset))
        {
            DataStorager.settings.offsetMs = 0;
        }
        else
        {
            DataStorager.settings.offsetMs = coffset;
        }
        // 皮肤
        DataStorager.settings.skinPath = SkinDropdown.options[SkinDropdown.value].text;
        // 保存
        DataStorager.SaveSettings();
        DataStorager.SaveKeySettings();
        globalSettings.handleSettings();
        SceneManager.LoadScene("Initalize");
    }
}
