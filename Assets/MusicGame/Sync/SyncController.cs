using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.Discovery;
using UnityEngine;
using UnityEngine.UI;

public class SyncController : NetworkBehaviour
{
    public GameObject triggerButton;
    public GameObject RetryButton;
    public GameObject RetryBigButton;
    public GameObject PracticeButton;
    public GameObject ResumeButton;

    void Start()
    {
        if (isClient)
        {
            Time.timeScale = 0;

            RetryButton.SetActive(false);
            PracticeButton.SetActive(false);
            RetryBigButton.SetActive(false);
            ResumeButton.SetActive(false);
        }
        if (isServer)
        {
            FindObjectOfType<NetworkDiscovery>().AdvertiseServer();
            triggerButton.SetActive(true);
        }
    }

    [Server]
    public void TriggerStart()
    {
        triggerButton.SetActive(false);
        RpcStart();
    }

    [ClientRpc]
    void RpcStart()
    {
        Time.timeScale = 1;
    }
}
