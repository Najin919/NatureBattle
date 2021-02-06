using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public enum GrenadeState
{
    Sleep = 0,
    Active = 1,
    Count = 2
}

public class GrenadeCtrl : MonoBehaviourPunCallbacks, IPunObservable
{
    //PhotonView 컴포넌트를 할당할 변수
    [HideInInspector] public PhotonView pv = null;
    Rigidbody rigid = null;
    public MeshRenderer[] m_MeshList = null;

    public AudioSource audioSource = null;

    public GameObject Effect = null;

    //두명이 동시에 먹는 걸 방지하기 위해서...
    [HideInInspector] public PotionState m_LocalState = PotionState.Active; //마스터 클라이언트만 로컬에서...
    [HideInInspector] public PotionState m_CurState = PotionState.Active;   //마스터 클라이언트만 로컬에서...
    [HideInInspector] public int AttackerId = -1;

    private bool m_TakeOnece = true;
    private int m_TakeUserIdx = -1;

    private float a_Delay = 1f;

    void Awake()
    {
        pv = GetComponent<PhotonView>();
        rigid = GetComponent<Rigidbody>();

        m_LocalState = PotionState.Active; //마스터 클라이언트만 로컬에서...
        m_CurState = PotionState.Active;   //마스터 클라이언트만 로컬에서...
    }

    // Start is called before the first frame update
    void Start()
    {
        AttackerId = pv.Owner.ActorNumber;

        ThrowGrenade();
        StartCoroutine(Explosion());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator Explosion()
    {
        yield return new WaitForSeconds(3f);
        rigid.isKinematic = true;

        DisableOtherClient();
        Effect.SetActive(true);
        audioSource.Play();

        RaycastHit[] rayHits = Physics.SphereCastAll(transform.position, 7f, Vector3.up);

        foreach (RaycastHit hitobj in rayHits)
        {
            PlayerDamage a_Player = hitobj.transform.GetComponent<PlayerDamage>();

            if (a_Player != null)
            {
                a_Player.TakeDamage(20f, AttackerId);
            }
        }

        yield return new WaitForSeconds(3f);
        PhotonNetwork.Destroy(this.gameObject);

    }


    void DisableOtherClient() //다른 Other PC인 경우 즉시 아이템이 먹은 것처럼 안보이게 처리하기 지원 함수
    {
        for (int i = 0; i < m_MeshList.Length; i++)
            m_MeshList[i].enabled = false;
    }
 

    [PunRPC]
    public void MasterDestroyRPC(int a_ActorNumber) //이 마스터는 수류탄의 if (pv.IsMine == true)을 의미한다.
    {
        if (pv.IsMine == true) //MasterClient 일때만
        {
            m_TakeUserIdx = a_ActorNumber;

            m_LocalState = PotionState.Sleep;
            PhotonNetwork.Destroy(this.gameObject);

        }
    }

    public void ThrowGrenade()
    {
        Debug.Log("power");
        //this.transform.parent = null;
        rigid.AddForce(Camera.main.transform.forward * 10,ForceMode.Impulse);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //로컬 플레이어의 위치 정보 송신
        if (stream.IsWriting)
        {
            stream.SendNext((int)m_LocalState);
        }
        else //원격 플레이어의 위치 정보 수신
        {
            m_CurState = (PotionState)stream.ReceiveNext();
        }
    }

}
