using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public enum _7mmState
{
    Sleep = 0,
    Active = 1,
    Count = 2
}

public class _7mm_Item : MonoBehaviourPunCallbacks, IPunObservable
{
    //PhotonView 컴포넌트를 할당할 변수
    [HideInInspector] public PhotonView pv = null;

    private MeshRenderer[] m_MeshList = null;

    //두명이 동시에 먹는 걸 방지하기 위해서...
    [HideInInspector] public _7mmState m_LocalState = _7mmState.Active; //마스터 클라이언트만 로컬에서...
    [HideInInspector] public _7mmState m_CurState = _7mmState.Active;   //마스터 클라이언트만 로컬에서...

    private bool m_TakeOnece = true;
    private int m_TakeUserIdx = -1;
    private int m_AddBul = 0;

    void Awake()
    {
        pv = GetComponent<PhotonView>();

        m_MeshList = gameObject.GetComponentsInChildren<MeshRenderer>();

        m_LocalState = _7mmState.Active; //마스터 클라이언트만 로컬에서...
        m_CurState = _7mmState.Active;   //마스터 클라이언트만 로컬에서...
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //-- 철저히 확인 자폭 코드
        if (pv.IsMine == true) //마스터 클라이언트 입장에서  RPC로 꼭 삭제할 것이다.
        {
            if (m_LocalState == _7mmState.Sleep || m_CurState == _7mmState.Sleep)
            {   // Sleep 상태인데 아직 지형에 살아 있다면...
                TakeItemNetSysc();

                PhotonNetwork.Destroy(this.gameObject); //즉시 제거
            }
        }//if (pv.IsMine == true)
    }

    void DisableOtherClient() //다른 Other PC인 경우 즉시 아이템이 먹은 것처럼 안보이게 처리하기 지원 함수
    {
        if (m_MeshList == null)
            m_MeshList = gameObject.GetComponentsInChildren<MeshRenderer>();

        for (int i = 0; i < m_MeshList.Length; i++)
            m_MeshList[i].enabled = false;
    }

    void TakeItemNetSysc()
    {
        if (m_TakeUserIdx < 0)
            return;

        if (m_TakeOnece == false) //네트워크상에서, 아이템은 한번만 주기 위한 2차 방어
            return;

        WeaponCtrl[] weaponCtrl = null;
        GameObject[] a_players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in a_players)
        {
            if (this.gameObject == player)
                continue;

            weaponCtrl = player.GetComponentsInChildren<WeaponCtrl>();
            if (weaponCtrl == null)
                continue;

            for(int i =0; i<weaponCtrl.Length; i++)
            if (m_TakeUserIdx == weaponCtrl[i].pv.Owner.ActorNumber && weaponCtrl[i].m_WeaponName == "AKM")
            {
                weaponCtrl[i].pv.RPC("AddBullet", RpcTarget.AllViaServer, m_AddBul);
                m_TakeOnece = false;
                break;
            }
        }
    }//void TakeItemNetSysc(int a_TakeUserIdx)

    void OnCollisionEnter(Collision collision)
    {
        //if (pv.IsMine == false) //pv.IsMine 일 때만 먹게하면 마스터 클라이언트 PC에서만 아이템을 먹을 수 있는 현상이 발생하는다. Other PC에서는 먹통
        //  return;

        if (m_LocalState == _7mmState.Sleep || m_CurState == _7mmState.Sleep) //<-- 2번 먹는 현상, 1차 방어
            return;

        if (collision.gameObject.name.Contains("Player"))
        {
            WeaponCtrl[] weaponCtrl = collision.gameObject.GetComponentsInChildren<WeaponCtrl>();
            if (weaponCtrl != null)
            {
                //아이템인 경우는 MasterClient 모니터링해서 같은 ID(아이템 획득시간이 나중인 해 삭제)를 갖고 있지 않게 한다. 
                //if (tankDamage.AddHealing(30) == true) //누가 먹었든 조정하고 있는 탱크 입장에서만 먹게 한다.

                for(int i =0; i < weaponCtrl.Length; i++)
                if (weaponCtrl[i].pv.IsMine == true && weaponCtrl[i].m_WeaponName =="AKM") //자기 조종하고 있는 탱크 기준으로만...
                {
                    m_TakeUserIdx = weaponCtrl[i].pv.Owner.ActorNumber;
                    m_AddBul = 30;

                    pv.RPC("MasterDestroyRPC", RpcTarget.MasterClient, m_TakeUserIdx, m_AddBul); //MasterClient에게 RPC 보내서 이 오브젝트 제거 요청
                    m_LocalState = _7mmState.Sleep;  //상태를 중계해서 MasterClient가 오브젝트 제거하게 설정함 : pv.IsMine 아닐때도 먹힐 수 있으니까 중계가 안될 수 있다.
                    //물약 입장에서 지금 이 물약이 (pv.IsMine == true : 마스터 클라이언트)일 수도 있고 
                    //(pv.IsMine == false) 아닐 수도 있다. (pv.IsMine == true : 마스터 클라이언트)가 먹힌게 아니면
                    //(pv.IsMine == false)의 m_LocalState는 마스터 클라이언트에 중계되지 않을 것이다. 
                    //일단 어떤 탱크가 와서 부딪치든 일단 안보이게 처리하고 뒤늦게 MasterClient가 "MasterDestroyRPC" 중계되서 Destroy이 될 것이다.
                    DisableOtherClient(); //일단 즉시 눈에는 안보이게 해 준다. 그래고 "MasterDestroyRPC" 중계되서 Destroy이 될 것이다.
                }
            }
        }//if (collision.gameObject.name.Contains("Tank"))
    }

    [PunRPC]
    public void MasterDestroyRPC(int a_ActorNumber, int a_AddBul) //이 마스터는 물약의 if (pv.IsMine == true)을 의미한다.
    {
        if (pv.IsMine == true) //MasterClient 일때만
        {
            m_TakeUserIdx = a_ActorNumber;
            m_AddBul = a_AddBul;
            TakeItemNetSysc();

            m_LocalState = _7mmState.Sleep; //물약 입장에서 마스터 클라이언트가 먹힌게 아니면
            //(pv.IsMine == false)의 m_LocalState는 마스터 클라이언트에 중계되지 않을 것이다. 
            //그래서 이쪽에서 모든 물약 클라이언트 물약에 상태를 다시 중계해 준다.
            PhotonNetwork.Destroy(this.gameObject);

            //누구한테 힐을 줄 건지 이 때 판단하는 것이 가장 정확할 것이다.
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        ////로컬 플레이어의 위치 정보 송신
        if (stream.IsWriting)
        {
            stream.SendNext((int)m_LocalState);
        }
        else //원격 플레이어의 위치 정보 수신
        {
            m_CurState = (_7mmState)stream.ReceiveNext();
        }
    }
}
