using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SpawnMgr : MonoBehaviour
{
    public GameObject GrenadePrefab = null;

    public GameObject _7mm_ItemPrefab = null;
    public GameObject _9mm_ItemPrefab = null;

    public GameObject PotionPrefab = null;

    // Start is called before the first frame update
    void Start()
    {
        Create_7mmItem();

        Create_9mmItem();

        Create_Potion();

        Create_Grenade();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Create_7mmItem()
    {
        if (PhotonNetwork.IsMasterClient == false)
            return;
                
        for(int i=0; i<5; i++) 
        {
            float posX = Random.Range(-90.0f, 90.0f);
            float posY = Random.Range(-140.0f, 140.0f);

            PhotonNetwork.InstantiateRoomObject(_7mm_ItemPrefab.name, new Vector3(posX, 25.0f, posY), Quaternion.identity, 0);
        }//if (a_PotionObj.Length < 5)
    }

    void Create_9mmItem()
    {
        if (PhotonNetwork.IsMasterClient == false)
            return;

        for (int i = 0; i < 5; i++)
        {
            float posX = Random.Range(-90.0f, 90.0f);
            float posY = Random.Range(-140.0f, 140.0f);

            PhotonNetwork.InstantiateRoomObject(_9mm_ItemPrefab.name, new Vector3(posX, 25.0f, posY), Quaternion.identity, 0);
        }//if (a_PotionObj.Length < 5)
    }

    void Create_Potion()
    {
        if (PhotonNetwork.IsMasterClient == false)
            return;

        for (int i = 0; i < 5; i++)
        {
            float posX = Random.Range(-90.0f, 90.0f);
            float posY = Random.Range(-140.0f, 140.0f);

            PhotonNetwork.InstantiateRoomObject(PotionPrefab.name, new Vector3(posX, 25.0f, posY), Quaternion.identity, 0);
        }//if (a_PotionObj.Length < 5)
    }

    void Create_Grenade()
    {
        if (PhotonNetwork.IsMasterClient == false)
            return;

        for (int i = 0; i < 5; i++)
        {
            float posX = Random.Range(-90.0f, 90.0f);
            float posY = Random.Range(-140.0f, 140.0f);

            PhotonNetwork.InstantiateRoomObject(GrenadePrefab.name, new Vector3(posX, 25.0f, posY), Quaternion.identity, 0);
        }//if (a_PotionObj.Length < 5)
    }
}
