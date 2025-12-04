using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerModel : MonoBehaviour
{
    public static Dictionary<string, string> PlayerModels = new()
    {
        { "shidunzi", "Model/default" },
        { "ball", "Model/ball"},
        { "bandeng", "Model/bandeng"},
        { "chocolate", "Model/chocolate" },
    };
    [SerializeField]
    private GameObject currentModel;

    void Awake()
    {
        string modelName = DataStorager.settings.modelName;
        if (modelName == "shidunzi" || !PlayerModels.ContainsKey(modelName))
        {
            return;
        }
        Destroy(currentModel);
        currentModel = Instantiate(Resources.Load<GameObject>(PlayerModels[modelName]), transform);
    }
}
