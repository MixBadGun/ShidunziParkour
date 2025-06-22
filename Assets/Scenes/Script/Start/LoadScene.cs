using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    // Start is called before the first frame update
    // void Awake() {
    // }
    void Start()
    {
        // if(GameObject.Find("MultyScript") != null){
        //     Destroy(GameObject.Find("MultyScript"));
        // }
        Application.targetFrameRate = DataStorager.settings.fpsLimit > 0 ? DataStorager.settings.fpsLimit : 300;
        SceneManager.LoadScene("Start");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
