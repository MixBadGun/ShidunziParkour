using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class GlobalSettings : MonoBehaviour
{
    void Awake() {
        handleSettings();
    }
    public AudioMixer AudioMixer;
    public void handleSettings(){
        float vol_1 = DataStorager.settings.MusicVolume == 0 ? -80 : (float)Math.Log10(DataStorager.settings.MusicVolume) * 25;
        float vol_2 = DataStorager.settings.SoundVolume == 0 ? -80 : (float)Math.Log10(DataStorager.settings.SoundVolume) * 25;
        AudioMixer.SetFloat("MusicVolume", vol_1);
        AudioMixer.SetFloat("SoundVolume", vol_2);
    }
}
