using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class ControlDisplay : MonoBehaviour
{
    public Dictionary<KeyCode, OneKey> keyCodes = new();
    public GameObject OneKeyTemplate;
    public void TriggerKey(KeyCode keyCode)
    {
        if (keyCodes.ContainsKey(keyCode))
        {
            keyCodes[keyCode].TriggerKey();
        }
        else
        {
            OneKey newKey = Instantiate(OneKeyTemplate, transform).GetComponent<OneKey>();
            newKey.keyCode = keyCode;
            keyCodes[keyCode] = newKey;
            newKey.TriggerKey();
        }
    }
}