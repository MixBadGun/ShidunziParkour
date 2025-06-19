using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandGenerator : MonoBehaviour
{
    public GameObject player;
    public GameObject land;
    public GameObject land_parent;
    private int land_count = 0;

    public const int LAND_LENGTH = 80;
    public const int OFFSET = 30;
    public const int PRE_OFFSET = 200;

    public void RespawnLand()
    {
        land_count = -1;
        while (land_parent.transform.childCount > 0)
        {
            DestroyImmediate(land_parent.transform.GetChild(0).gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        while (player.transform.position.z > land_count * LAND_LENGTH - OFFSET - PRE_OFFSET){
            land_count += 1;
            if ((land_count * LAND_LENGTH - OFFSET) - player.transform.position.z < -40)
            {
                continue;
            }
            GameObject newland = Instantiate(land, land_parent.transform);
            newland.GetComponent<MusicLand>().setLand();
            newland.transform.position = new Vector3(0,0,land_count * LAND_LENGTH - OFFSET);
        }
    }
}
