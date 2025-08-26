using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DifficultyToggle : MonoBehaviour
{
    [SerializeField]
    private Toggle toggle;
    [SerializeField]
    private TMP_Text label;

    public void SetLabelAndToggle(string text, bool forceOn = false)
    {
        label.text = text;
        toggle.isOn = true;
        if (forceOn)
        {
            toggle.interactable = false;
        }
    }

    public bool IsOn()
    {
        return toggle.isOn;
    }
}
