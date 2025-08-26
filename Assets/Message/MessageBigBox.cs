using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MessageBigBox : MonoBehaviour
{
    [SerializeField]
    private TMP_Text infoText;

    public void TriggerInfo(string info)
    {
        infoText.text = info;
        GetComponent<Animator>().SetTrigger("Show");
    }
}
