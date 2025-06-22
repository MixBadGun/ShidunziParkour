using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PracticingBar : MonoBehaviour
{
    public BeatmapManager beatmapManager;
    public TMP_InputField inputField;
    public Sprite[] pauseAndStartImage;
    public Button pauseButton;
    bool playing = false;
    bool firstTime = true;
    public void SetFirstValue()
    {
        if (firstTime)
        {
            inputField.text = beatmapManager.GetRealPlayingTime().ToString();
            firstTime = false;
        }
    }

    public void Reload()
    {
        if (firstTime)
        {
            return;
        }
        if (!float.TryParse(inputField.text, out float ctime))
        {
            ctime = 0;
        }
        beatmapManager.ReloadFromTime(ctime);
    }

    public void GetStartOrPause()
    {
        if (playing)
        {
            pauseButton.image.sprite = pauseAndStartImage[0];
            beatmapManager.GetIntoPractice();
            playing = !playing;
            inputField.enabled = true;
            return;
        }
        pauseButton.image.sprite = pauseAndStartImage[1];
        if (!float.TryParse(inputField.text, out float ctime))
        {
            ctime = 0;
        }
        beatmapManager.ReStart(ctime);
        playing = !playing;
        inputField.enabled = false;
    }
}
