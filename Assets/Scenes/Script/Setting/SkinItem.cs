using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkinItem : MonoBehaviour
{
    private int skinIndex;
    [SerializeField]
    private TMP_Text skinNameText;

    [SerializeField]
    private Image highlightImage;
    [SerializeField]
    private SkinImage skinImage;

    public void SetSkinIndex(int index)
    {
        skinIndex = index;
        UpdateItem();
    }

    private void UpdateItem()
    {
        List<string> model_list = PlayerModel.PlayerModels.Keys.ToList();
        if (skinIndex < 0 || skinIndex >= model_list.Count)
        {
            return;
        }
        skinNameText.text = LanguageManager.GetLocalizedString(model_list[skinIndex]);
        skinImage.UpdateImage(skinIndex);
    }

    public void OffSelect()
    {
        highlightImage.gameObject.SetActive(false);
    }

    public void OnSelect()
    {
        highlightImage.gameObject.SetActive(true);
    }

    private void Update()
    {
        // 如果被点击，则更新当前选中项
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Input.mousePosition;
            RectTransform rectTransform = GetComponent<RectTransform>();
            if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, mousePos))
            {
                SkinOption skinOption = GetComponentInParent<SkinOption>();
                skinOption.UpdateCurrentSkinIndex(skinIndex);
            }
        }
    }
}
