using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class BulletCtrl : MonoBehaviourPunCallbacks, IPunObservable
{
    PhotonView pv;

    public float m_Speed = 10;

    //Prefabs
    [Header("Prefabs")]
    public GameObject m_HitSparkPrefab;
    public GameObject m_HitHolePrefab;
    public GameObject m_BloodPrefab;

    public RaycastHit a_hit;
    string HitTag = "null";
    string a_HitTag = "null";

    public PlayerDamage Enemy = null;

    // Start is called before the first frame update
    void Start()
    {
        pv = GetComponent<PhotonView>();

        if (pv.IsMine)
            HitTag = a_hit.transform.tag;
        else
            HitTag = a_HitTag;

        pv.RPC("Effect", RpcTarget.AllBuffered, null);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [PunRPC]
    void Effect()
    {        
        if (HitTag == "Player")
        {
            GameObject blood = (GameObject)Instantiate(m_BloodPrefab, this.transform.position, Quaternion.identity);//a_hit.point, Quaternion.FromToRotation(Vector3.up, a_hit.normal));
            Destroy(blood, 1f);
            blood.transform.SetParent(a_hit.transform);
        }
        else
        {
            GameObject hitHole = (GameObject)Instantiate(m_HitHolePrefab, this.transform.position, Quaternion.FromToRotation(Vector3.up, a_hit.normal));//a_hit.point, Quaternion.FromToRotation(Vector3.up, a_hit.normal));
            Destroy(hitHole, 3f);
            hitHole.transform.SetParent(a_hit.transform);

            GameObject hitSpark = (GameObject)Instantiate(m_HitSparkPrefab, this.transform.position, Quaternion.identity);//a_hit.point, Quaternion.FromToRotation(Vector3.up, a_hit.normal));
            Destroy(hitSpark, 0.2f);
            hitSpark.transform.SetParent(a_hit.transform);

        }

        Destroy(this.gameObject, 1f);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            if(a_hit.transform.tag != null)
            stream.SendNext(a_hit.transform.tag);
        }
        else
        {
            a_HitTag = (string)stream.ReceiveNext();
        }
    }
}
