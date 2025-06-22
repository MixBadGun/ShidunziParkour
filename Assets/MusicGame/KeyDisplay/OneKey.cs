using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OneKey : MonoBehaviour
{
    public KeyCode keyCode;
    public TMP_Text KeyText;
    public TMP_Text KeyNums;
    int Nums = 0;
    public Animator HoverAnimator;
    void Start()
    {
        KeyText.text = keyCode switch
        {
            KeyCode.LeftArrow => "←",
            KeyCode.RightArrow => "→",
            KeyCode.UpArrow => "↑",
            KeyCode.DownArrow => "↓",
            _ => keyCode.ToString(),
        };
    }

    public void TriggerKey()
    {
        Nums++;
        KeyNums.text = Nums.ToString();
        HoverAnimator.SetTrigger("KeyTrigger");
    }
}
