using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private static SoundManager instance;
    public AudioClip init_boom_sound;
    public static AudioClip boom_sound;

    public AudioClip init_best_sound;
    public static AudioClip best_sound;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        DontDestroyOnLoad(this.gameObject);
        instance = this;
        boom_sound = init_boom_sound;
        best_sound = init_best_sound;
    }
}
