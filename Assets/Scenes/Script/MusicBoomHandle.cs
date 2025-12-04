using System.Collections;
using UnityEngine;
using static HitEffectManager;

public class MusicBoomHandle : MonoBehaviour
{
    [SerializeField]
    private HitEffectType hitEffectType;
    public float destroyTime = 0.5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake(){
        // gameObject.GetComponent<AudioSource>().Play();
        if(!DataStorager.settings.notBoomFX){
            gameObject.GetComponent<ParticleSystem>().Play();
        }
        StartCoroutine(DestroyAfterDelay());
    }
    void Start()
    {
        HitEffectManager.CreateHitEffect(hitEffectType, transform.position);
    }

    IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(destroyTime * 50 / Player.targetPlayer.GetVelocity());
        Destroy(gameObject);
    }
}
