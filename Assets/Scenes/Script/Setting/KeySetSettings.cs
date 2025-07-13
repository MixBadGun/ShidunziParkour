using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror.BouncyCastle.Crypto.Modes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KeySetSettings : MonoBehaviour
{
    public TMP_Text esc_text;
    public TMP_Text left_text;
    public TMP_Text right_text;
    public TMP_Text up_text;
    public TMP_Text down_text;
    public TMP_Text pad1_text;
    public TMP_Text pad2_text;
    public TMP_Text pad3_text;
    public GameObject WaitForKeyDisplay;

    void Awake()
    {
        DataStorager.keysettings.esc ??= new KeyCode[] { KeyCode.Escape };
    }

    void Update(){
        esc_text.text = string.Join(", ", DataStorager.keysettings.esc);
        left_text.text = string.Join(", ", DataStorager.keysettings.left);
        right_text.text = string.Join(", ", DataStorager.keysettings.right);
        up_text.text = string.Join(", ", DataStorager.keysettings.up);
        down_text.text = string.Join(", ", DataStorager.keysettings.down);
        pad1_text.text = string.Join(", ", DataStorager.keysettings.pad1);
        pad2_text.text = string.Join(", ", DataStorager.keysettings.pad2);
        pad3_text.text = string.Join(", ", DataStorager.keysettings.pad3);
    }

    IEnumerator WaitForKey(int index) {
        while(!Input.anyKeyDown){
            yield return new WaitForFixedUpdate();
        }
        foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
        {
            if (!Input.GetKeyDown(keyCode)) continue;
            switch (index)
            {
                case 0:
                    {
                        List<KeyCode> codes = new(DataStorager.keysettings.left)
                    {
                        keyCode
                    };
                        DataStorager.keysettings.left = codes.ToArray();
                        break;
                    }
                case 1:
                    {
                        List<KeyCode> codes = new(DataStorager.keysettings.right)
                    {
                        keyCode
                    };
                        DataStorager.keysettings.right = codes.ToArray();
                        break;
                    }
                case 2:
                    {
                        List<KeyCode> codes = new(DataStorager.keysettings.up)
                    {
                        keyCode
                    };
                        DataStorager.keysettings.up = codes.ToArray();
                        break;
                    }
                case 3:
                    {
                        List<KeyCode> codes = new(DataStorager.keysettings.down)
                    {
                        keyCode
                    };
                        DataStorager.keysettings.down = codes.ToArray();
                        break;
                    }
                case 4:
                    {
                        List<KeyCode> codes = new(DataStorager.keysettings.pad1)
                    {
                        keyCode
                    };
                        DataStorager.keysettings.pad1 = codes.ToArray();
                        break;
                    }
                case 5:
                    {
                        List<KeyCode> codes = new(DataStorager.keysettings.pad2)
                    {
                        keyCode
                    };
                        DataStorager.keysettings.pad2 = codes.ToArray();
                        break;
                    }
                case 6:
                    {
                        List<KeyCode> codes = new(DataStorager.keysettings.pad3)
                    {
                        keyCode
                    };
                        DataStorager.keysettings.pad3 = codes.ToArray();
                        break;
                    }
                case 7:
                    {
                        List<KeyCode> codes = new(DataStorager.keysettings.esc)
                    {
                        keyCode
                    };
                        DataStorager.keysettings.esc = codes.ToArray();
                        break;
                    }
            }
            break;
        }
        WaitForKeyDisplay.SetActive(false);
    }

    public void WaitForKeyMapping(int index){
        WaitForKeyDisplay.SetActive(true);
        StartCoroutine(WaitForKey(index));
    }

    public void ResetForKeyMapping(int index){
        switch (index)
        {
            case 0:
                {
                    DataStorager.keysettings.left = new KeyCode[] { };
                    break;
                }
            case 1:
                {
                    DataStorager.keysettings.right = new KeyCode[] { };
                    break;
                }
            case 2:
                {
                    DataStorager.keysettings.up = new KeyCode[] { };
                    break;
                }
            case 3:
                {
                    DataStorager.keysettings.down = new KeyCode[] { };
                    break;
                }
            case 4:
                {
                    DataStorager.keysettings.pad1 = new KeyCode[] { };
                    break;
                }
            case 5:
                {
                    DataStorager.keysettings.pad2 = new KeyCode[] { };
                    break;
                }
            case 6:
                {
                    DataStorager.keysettings.pad3 = new KeyCode[] { };
                    break;
                }
            case 7:
                {
                    DataStorager.keysettings.esc = new KeyCode[]{};
                    break;
                }
        }
    }

    public void ResetDefaultKeyMapping()
    {
        DataStorager.keysettings = DataStorager.GetDefaultKeySettings();
    }
}