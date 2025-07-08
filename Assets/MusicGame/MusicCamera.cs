using UnityEngine;
using System;
using static BeatmapManager;
using Unity.VisualScripting.Antlr3.Runtime;
using DG.Tweening;
public class MusicCamera : MonoBehaviour
{
    public GameObject center;
    private Vector3 offset;
    private Vector3 initRotation;
    private float move_timer = 0f;
    private float shake_timer = 1f;
    private bool toShake = false;
    private const float CROSS_TIME = 1f;
    private const float SHAKE_TIME = 0.5f;

    private bool toTrans = false;
    private CameraData ori_cameraData = new();
    private CameraData now_cameraData = new();
    private float trans_cross_time = 1f;
    private float trans_timer = 1f;

    private Vector3 fixedInitPosition;

    void Start()
    {
        fixedInitPosition = transform.position;
        offset = transform.position - center.transform.position;
        initRotation = transform.rotation.eulerAngles;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = center.transform.position + offset;
        fixedCamera();
        moveCamera();
    }

    void fixedCamera(){
        if (DataStorager.settings.fixedCameraMod)
        {
            transform.position = new Vector3(fixedInitPosition.x, transform.position.y, transform.position.z);
        }
    }

    public void triggerShake()
    {
        if (!DataStorager.settings.notShake)
        {
            toShake = true;
        }
    }

    public void triggerTransform(CameraData from_cameraData)
    {
        ori_cameraData = now_cameraData;
        now_cameraData = from_cameraData;
        trans_cross_time = from_cameraData.cross_time;
        toTrans = true;
    }

    float CalcProgress(float timer, float cross_time, int type = 1)
    {
        if (type >= (int)Ease.INTERNAL_Zero || type <= (int)Ease.Unset)
        {
            type = 1;
        }
        return DOVirtual.EasedValue(0, 1, timer / cross_time, (Ease)type);
    }
    void moveCamera()
    {
        if (toShake)
        {
            shake_timer = 0f;
            toShake = false;
        }
        if (shake_timer < SHAKE_TIME)
        {
            transform.position += new Vector3(UnityEngine.Random.Range(shake_timer / SHAKE_TIME - 1, 1 - shake_timer / SHAKE_TIME), UnityEngine.Random.Range(shake_timer / SHAKE_TIME - 1, 1 - shake_timer / SHAKE_TIME));
        }
        shake_timer += Time.deltaTime;

        // 自定义变换
        if (toTrans)
        {
            trans_timer = 0f;
            toTrans = false;
        }
        float progress = 1;
        if (trans_timer < trans_cross_time)
        {
            progress = CalcProgress(trans_timer, trans_cross_time, now_cameraData.ease_type);
        }
        Vector3 new_rotation = initRotation;
        CameraData new_transdata = progress * now_cameraData + (1 - progress) * ori_cameraData;
        new_rotation.z += new_transdata.z_angle;
        new_rotation.y += new_transdata.y_angle;
        new_rotation.x += new_transdata.x_angle;
        transform.rotation = Quaternion.Euler(new_rotation);
        transform.position += new Vector3(new_transdata.x_pos, new_transdata.y_pos, new_transdata.z_pos);

        trans_timer += Time.deltaTime;
    }

    Vector3 CalcOffsetByTimer()
    {
        float t = move_timer += Time.deltaTime;
        return offset * (float)(1 - Math.Pow(1 - (t / CROSS_TIME), 4));
    }
}
