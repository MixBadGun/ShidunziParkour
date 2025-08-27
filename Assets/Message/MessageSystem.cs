using System.Collections;
using System.Collections.Generic;
using Unity.Entities.UniversalDelegates;
using UnityEngine;

public class MessageSystem : MonoBehaviour
{
    private static MessageSystem instance;
    [SerializeField]
    private MessageBigBox messageBigBox;
    void Awake()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        DontDestroyOnLoad(this.gameObject);
        instance = this;
    }

    private void m_SendInfo(string info, float delay = 2f)
    {
        Debug.Log(info);
        MessageBigBox messageBox = Instantiate(messageBigBox, transform);
        messageBox.gameObject.SetActive(true);
        messageBox.TriggerInfo(info);
        StartCoroutine(DelayDestroyInfo(messageBox.gameObject, delay));
    }

    private IEnumerator DelayDestroyInfo(GameObject messageBox, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(messageBox);
    }

    public static void SendInfo(string info, float delay = 2f)
    {
        instance.m_SendInfo(info, delay);
    }
}
