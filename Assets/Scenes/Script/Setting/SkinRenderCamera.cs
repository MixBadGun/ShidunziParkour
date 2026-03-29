using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkinRenderCamera : MonoBehaviour
{
    [SerializeField]
    private Transform modelPosition;

    public Texture2D GetPicture(int skinIndex)
    {
        Vector3 offsetPosition = new Vector3(0, 0, -5) * skinIndex; // 为了防止多次调用时模型重叠，根据 skinIndex 调整位置
        Vector3 originalCameraPosition = transform.position;
        transform.position += offsetPosition;
        modelPosition.gameObject.SetActive(false);
        // 通过将模型加载在另一个地方，并使用摄像机拍摄，然后渲染到一个 RenderTexture 来实现预览图的功能。
        // 触发 SkinRenderCamera 的 GetPicture 方法获取像素即可
        GameObject model = Instantiate(Resources.Load<GameObject>(PlayerModel.PlayerModels.Values.ToList()[skinIndex]));
        model.transform.position = modelPosition.position + offsetPosition;

        RenderTexture renderTexture = new RenderTexture(256, 256, 24);
        Camera camera = GetComponent<Camera>();
        camera.targetTexture = renderTexture;
        camera.Render();
        RenderTexture.active = renderTexture;
        Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();
        camera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(renderTexture);
        // Destroy(model);
        transform.position = originalCameraPosition;
        return texture;
    }
}
