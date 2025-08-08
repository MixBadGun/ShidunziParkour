using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalResources : MonoBehaviour
{
    public static GameObject perfectboom;
    public static GameObject greatboom;
    public static GameObject showboom;
    public static GameObject missboom;
    public static GameObject fenceTemplate;
    [SerializeField]
    private GameObject input_perfectboom;
    [SerializeField]
    private GameObject input_greatboom;
    [SerializeField]
    private GameObject input_showboom;
    [SerializeField]
    private GameObject input_missboom;
    [SerializeField]
    private GameObject input_fenceTemplate;
    // Start is called before the first frame update
    void Start()
    {
        perfectboom = input_perfectboom;
        greatboom = input_greatboom;
        showboom = input_showboom;
        missboom = input_missboom;
        fenceTemplate = input_fenceTemplate;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
