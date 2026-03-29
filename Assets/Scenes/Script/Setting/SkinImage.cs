using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class SkinImage : MonoBehaviour
{
    [SerializeField]
    private SkinRenderCamera skinRenderCamera;

    public void UpdateImage(int skinIndex)
    {
        // 通过将模型加载在另一个地方，并使用摄像机拍摄，然后渲染到一个 RenderTexture 来实现预览图的功能。
        // 触发 SkinRenderCamera 的 GetPicture 方法获取像素即可
        Texture2D texture = skinRenderCamera.GetPicture(skinIndex);
        GetComponent<Image>().sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
    }
}
