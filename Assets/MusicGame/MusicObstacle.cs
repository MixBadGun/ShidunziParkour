using System;
using System.Linq;
using Lofelt.NiceVibrations;
using UnityEngine;

public class MusicObstacle : MonoBehaviour
{
    public int index = 0;
    public float size = 1;
    private bool isTouched = false;
    // private bool isPerfect = false;
    private bool isInit = true;
    private bool isShow = false;
    private bool isLast = false;
    // 躲避属性
    private bool isDodge = false;
    public bool isBest = false;
    bool forcePerfect = false;
    public int[] track;
    public GameObject player;
    public GameObject camera;
    public AudioSource bestSound;
    public GameObject boomSounds;
    public BeatmapManager beatmapManager;
    void Start()
    {
        if(DataStorager.settings.cinemaMod || track.Count() <= 0 || isShow){
            isTouched = true;
            forcePerfect = true;
        }

        // 愚人节彩蛋
        if(DateTime.Now.Day == 1 && DateTime.Now.Month == 4){
            isTouched = true;
            forcePerfect = true;
        }
    }

    void GenerateBoom(GameObject theboom){
        camera.GetComponent<MusicCamera>().triggerShake();
        var newboom = Instantiate(theboom);
        newboom.transform.position = gameObject.transform.position + new Vector3(0,2.5f,0) * size;
        newboom.transform.localScale *= size;

        // 愚人节彩蛋
        if (DateTime.Now.Day == 1 && DateTime.Now.Month == 4)
        {
            newboom.transform.localScale *= 100;
            newboom.transform.position += new Vector3(0, 0, 20) * player.GetComponent<Player>().GetVelocity() / 50;
        }

        // 播放声音
        boomSounds.transform.GetChild(index % boomSounds.transform.childCount).GetComponent<AudioSource>().volume = Math.Min(size, 2);
        boomSounds.transform.GetChild(index % boomSounds.transform.childCount).GetComponent<AudioSource>().Play();
        if (isBest)
        {
            bestSound.Play();
        }

        if(!DataStorager.settings.notVibrate){
            HapticPatterns.PlayEmphasis(1.0f, 0.0f);
        }

    }

    public void setNote(){
        isInit = false;
    }

    public void setBestNote(){
        isBest = true;
    }

    public void setShowNote(){
        isShow = true;
    }

    public void setLastNote(){
        isLast = true;
    }

    public void setDodgeNote()
    {
        isDodge = true;
        Instantiate(GlobalResources.fenceTemplate, transform).transform.localPosition = Vector3.zero;
    }

    void triggerEnd() {
        beatmapManager.triggerEnd();
    }

    bool isOnTrack() {
        if(track.Contains(player.GetComponent<Player>().GetNowTrack())){
            return true;
        }
        foreach(var inputImp in player.GetComponent<Player>().inputImpluses){
            if(track.Contains(inputImp.track)){
                player.GetComponent<Player>().inputImpluses.Remove(inputImp);
                return true;
            }
        }
        return false;
    }

    // 严格的判定是否在轨道上，仅判定当前轨道
    bool isOnTrackCritical() {
        if(track.Contains(player.GetComponent<Player>().GetNowTrack())){
            return true;
        }
        return false;
    }

    void FixedUpdate()
    {
        if (isDodge)
        {
            DodgeUpdate();
        }
        else
        {
            NormalUpdate();
        }
    }

    private bool dodgeHandled = false; // 避免多次处理躲避音符

    /// 躲避墩子的判定逻辑
    void DodgeUpdate() {

        if (player.transform.position.z - gameObject.transform.position.z >= 10)
        {
            Destroy(gameObject);
        }

        if (dodgeHandled)
        {
            return;
        }

        // 仅判定 Late，判定间隔非常短，特别松
        if (player.transform.position.z >= gameObject.transform.position.z)
        {
            if (player.transform.position.z - gameObject.transform.position.z <= 0.25 * (player.GetComponent<Player>().GetVelocity() / 50))
            {
                if (isOnTrackCritical()
                    && Math.Abs(player.transform.position.y - gameObject.transform.position.y) < 1)
                {
                    if (!isShow)
                    {
                        GenerateBoom(GlobalResources.missboom);
                    }
                    else
                    {
                        GenerateBoom(GlobalResources.showboom);
                    }
                    beatmapManager.AddNowPoint(BeatmapManager.M_TYPE.Miss, !isShow);
                    if (isBest)
                    {
                        beatmapManager.AddNowBest(BeatmapManager.M_TYPE.Break_M, !isShow);
                    }
                    beatmapManager.Miss();
                    if (isLast)
                    {
                        triggerEnd();
                    }
                    Destroy(gameObject);
                }
            }
            else
            {
                // 没撞到那就是 Perfect
                dodgeHandled = true;
                beatmapManager.AddNowPoint(BeatmapManager.M_TYPE.Perfect, !isShow);
                if (isBest)
                {
                    beatmapManager.AddNowBest(BeatmapManager.M_TYPE.Break_P, !isShow);
                }
                if (isLast)
                {
                    triggerEnd();
                }
            }
        }
    }

    /// 非躲避墩子的判定逻辑
    void NormalUpdate()
    {
        
        if (Math.Abs(player.transform.position.z - gameObject.transform.position.z) < 2 * (player.GetComponent<Player>().GetVelocity() / 50)
          && isOnTrack()
          && Math.Abs(player.transform.position.y - gameObject.transform.position.y) < 2
        )
        {
            isTouched = true;
        }
        // 下落墩子也可以
        if(Math.Abs(player.transform.position.z - gameObject.transform.position.z) < 3.5 * (player.GetComponent<Player>().GetVelocity() / 50)
            && player.GetComponent<Player>().isDroping()
            && isOnTrack()
            && player.transform.position.y >= gameObject.transform.position.y
        ){
            isTouched = true;
        }

        if(player.transform.position.z >= gameObject.transform.position.z && isTouched && !isInit)
        {
            if(player.transform.position.z - gameObject.transform.position.z <= 1.25 * (player.GetComponent<Player>().GetVelocity() / 50) || forcePerfect){
                // Perfect
                if (!isShow)
                {
                    GenerateBoom(GlobalResources.perfectboom);
                }
                else
                {
                    GenerateBoom(GlobalResources.showboom);
                }

                beatmapManager.AddNowPoint(BeatmapManager.M_TYPE.Perfect,!isShow);
                if(isBest){
                    beatmapManager.AddNowBest(BeatmapManager.M_TYPE.Break_P,!isShow);
                }
                if(isLast){
                    triggerEnd();
                }
                Destroy(gameObject);
                return;
            } else {
                // Great
                {
                    if (!isShow)
                    {
                        GenerateBoom(GlobalResources.greatboom);
                    }
                    else
                    {
                        GenerateBoom(GlobalResources.showboom);
                    }

                    beatmapManager.AddNowPoint(BeatmapManager.M_TYPE.Great,!isShow);
                    if(isBest){
                        beatmapManager.AddNowBest(BeatmapManager.M_TYPE.Break_G,!isShow);
                    }
                    if(isLast){
                        triggerEnd();
                    }
                    Destroy(gameObject);
                    return;
                }
            }
        }


        if(player.transform.position.z - gameObject.transform.position.z > 10 * (player.GetComponent<Player>().GetVelocity() / 50)
            && !isInit){
            // GenerateBoom();
            beatmapManager.AddNowPoint(BeatmapManager.M_TYPE.Miss,!isShow);
            if(isBest){
                beatmapManager.AddNowBest(BeatmapManager.M_TYPE.Break_M,!isShow);
            }
            beatmapManager.Miss();
            if(isLast){
                triggerEnd();
            }
            Destroy(gameObject);
            return;
        }
    }
}
