using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class fpsDisplay : MonoBehaviour
{
    float lastUpdateTime = 0f;
    int frames = 0;

    void Start()
    {
        lastUpdateTime = Time.realtimeSinceStartup;
        if (!DataStorager.settings.fpsDisplay)
        {
            gameObject.SetActive(false);
        }
    }

    void Update()
    {
        frames++;
        if (Time.realtimeSinceStartup - lastUpdateTime < 0.5f)
        {
            return; // Update only once per second
        }
        float fps = frames / (Time.realtimeSinceStartup - lastUpdateTime);
        frames = 0;
        GetComponent<TMP_Text>().text = $"FPS: {fps:F2}";
        lastUpdateTime = Time.realtimeSinceStartup;
    }
}
