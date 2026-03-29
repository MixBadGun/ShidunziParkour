using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkinOption : MonoBehaviour
{
    private int currentSkinIndex = 0;
    [SerializeField]
    private SkinItem skinItemTemplate;

    private List<SkinItem> skinItems = new List<SkinItem>();

    public void SetCurrentSkinIndex(int index)
    {
        currentSkinIndex = index;
    }

    public void UpdateCurrentSkinIndex(int index)
    {
        skinItems[currentSkinIndex].OffSelect();
        currentSkinIndex = index;
        skinItems[currentSkinIndex].OnSelect();
    }


    void Awake()
    {
        // 从 PlayerModel 获取全部模型
        List<string> model_list = PlayerModel.PlayerModels.Keys.ToList();
        // 将 List 里的值
        for(int i = 0;i < model_list.Count; i++)
        {
            SkinItem item = Instantiate(skinItemTemplate, transform);
            skinItems.Add(item);
            item.SetSkinIndex(i);
            item.gameObject.SetActive(true);
        }
        skinItems[currentSkinIndex].OnSelect();
    }

    public int GetCurrentSkinIndex()
    {
        return currentSkinIndex;
    }
}
