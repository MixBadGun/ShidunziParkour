using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class HitEffectManager : MonoBehaviour
{
    private static HitEffectManager _instance;
    // 击打效果管理器
    public enum HitEffectType
    {
        Perfect,
        Great,
        Miss
    }

    [SerializeField]
    private GameObject[] EffectPrefabs;

    private void Awake()
    {
        _instance = this;
    }

    /// <summary>
    /// 创建击打效果
    /// </summary>
    public static void CreateHitEffect(HitEffectType effectType, Vector3 WorldPosition)
    {
        _instance._CreateHitEffect(effectType, WorldPosition);   
    }

    private void _CreateHitEffect(HitEffectType effectType, Vector3 WorldPosition)
    {
        // 根据 WorldPosition, 映射到 Canvas 的位置
        Vector2 canvasPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _instance.GetComponent<RectTransform>(),
            Camera.main.WorldToScreenPoint(WorldPosition),
            _instance.GetComponent<Canvas>().worldCamera,
            out canvasPosition);
        // 如果太靠近 Canvas 底部了，就矫正
        RectTransform canvasRect = _instance.GetComponent<RectTransform>();
        float canvasHeight = canvasRect.rect.height;
        float bottomThreshold = -canvasHeight * 0.5f + 50f; // 距离底部50像素
        
        if (canvasPosition.y < bottomThreshold)
        {
            canvasPosition.y = bottomThreshold;
        }

        // 创建击打效果
        GameObject effectPrefab = EffectPrefabs[(int)effectType];
        GameObject effectInstance = Instantiate(effectPrefab, _instance.transform);
        effectInstance.transform.localPosition = canvasPosition;

        // 设置击打效果的生命周期
        Destroy(effectInstance, 1.0f); // 1秒后销毁
    }
}
