using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerModel : MonoBehaviour
{
    public static Dictionary<string, string> PlayerModels = new()
    {
        { "shidunzi", "Model/default" },
        { "ball", "Model/ball"},
        { "bandeng", "Model/bandeng"},
        { "chocolate", "Model/chocolate" },
        { "ria", "Model/ria_bollard" }
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

    public static int GetPlayerModelIndex(string modelName)
    {
        List<string> model_list = PlayerModels.Keys.ToList();
        if (!model_list.Contains(modelName))
        {
            return 0; // 默认模型索引
        }
        return model_list.IndexOf(modelName);
    }
}
