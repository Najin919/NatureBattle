using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerDamage : MonoBehaviourPunCallbacks
{
    [HideInInspector] public PhotonView pv;

    public PlayerCtrl m_PlayerCtrl;
    public GameManager m_RefGameMgr;

    //HP
    float m_MaxHp = 100;
    public float m_CurHp = 100;
    float a_OldcurHp = 0;
    Image m_Hpbar;
    Vector3 DeathVec = Vector3.zero;
    //HP

    //플레이어 Id를 저장하는 변수
    public int playerId = -1;

    public GameObject DefaultModel;
    public GameObject RagDollModel;
    public SkinnedMeshRenderer[] m_BodyRenders;
    public MeshRenderer[] m_WeaponRenders;

    public GameObject[] DefaultWeapon;
    public GameObject[] RagDollWeapon;

    public int killCount = 0;

    ExitGames.Client.Photon.Hashtable KillProps = new ExitGames.Client.Photon.Hashtable();
    ExitGames.Client.Photon.Hashtable CurrHpProps = new ExitGames.Client.Photon.Hashtable();

    private void Awake()
    {
        pv = GetComponent<PhotonView>();

        GameObject a_GObj = GameObject.Find("GameManager");
        if (a_GObj != null)
        {
            m_RefGameMgr = a_GObj.GetComponent<GameManager>();
            if (m_RefGameMgr != null)
            {
                m_RefGameMgr.m_RefDamage = this;
            }
        }//if(a_GObj != null)

        m_CurHp = 100;
        a_OldcurHp = 100;
    }

    // Start is called before the first frame update
    void Start()
    {
        m_PlayerCtrl = GetComponent<PlayerCtrl>();

        m_Hpbar = GameObject.Find("Hpbar").GetComponent<Image>();
        m_Hpbar.fillAmount = m_CurHp / m_MaxHp;

        if (pv != null && pv.IsMine == true)
        {
            CurrHpProps.Clear();
            CurrHpProps.Add("curHp", m_CurHp);
            CurrHpProps.Add("LastEneyID", -1);
            pv.Owner.SetCustomProperties(CurrHpProps);

            KillProps.Clear();
            KillProps.Add("KillCount", 0);  //초기화
            pv.Owner.SetCustomProperties(KillProps);
        }

        playerId = pv.Owner.ActorNumber;
    }

    // Update is called once per frame
    void Update()
    {
        if (pv != null && pv.Owner != null && m_RefGameMgr.m_GameState != GameState.End)
        {
            if (pv.Owner.CustomProperties.ContainsKey("curHp") == true) //모든 캐릭터의 에너지바 동기화
            {
                m_CurHp = (float)pv.Owner.CustomProperties["curHp"];   //모든 캐릭터... 매플레임 계속 동기화 

                if (pv.IsMine)
                {
                    //if (m_RefGameMgr.m_Rank <= 1)
                        //m_RefGameMgr.RankPlayer(killCount);

                    //현재 생명치 백분율 = (현재 생명치) / (초기 생명치)
                    m_Hpbar.fillAmount = m_CurHp / m_MaxHp;

                    //생명 수치에 따라 Filled 이미지의 색상을 변경
                    if (m_Hpbar.fillAmount <= 0.5f)
                        m_Hpbar.color = new Color32(255, 124, 124, 255);
                }

                if (0 < a_OldcurHp)
                {
                    if (m_CurHp <= 0)
                    {
                        if (pv.Owner.CustomProperties.ContainsKey("LastAttackerID") == true && pv.Owner.ActorNumber != (int)pv.Owner.CustomProperties["LastAttackerID"])
                        {
                            int a_LastEmID = (int)pv.Owner.CustomProperties["LastAttackerID"];

                                if (0 <= a_LastEmID)
                                SaveKillCount(a_LastEmID);
                        }   
                        
                        if(pv.IsMine)
                        Die();

                        Debug.Log("Rank--");
                        m_RefGameMgr.m_Rank--;
                    }

                }// if (0 < a_OldcurHp)

                a_OldcurHp = m_CurHp;
            }//if (pv.owner.CustomProperties.ContainsKey("curHp") == true) 

            if (pv.IsMine)
            {
                //마스터가 킬카운트 관리한것을 받아 동기화 시켜준다.
                if (pv.Owner.CustomProperties.ContainsKey("KillCount") == true)
                {
                    int a_killCnt = (int)pv.Owner.CustomProperties["KillCount"];
                    if (killCount != a_killCnt)
                    {
                        killCount = a_killCnt;

                        if (m_RefGameMgr.KillTxt != null)
                        {
                            m_RefGameMgr.KillTxt.gameObject.SetActive(true);
                            m_RefGameMgr.KillTxt.text = killCount + "킬";
                        }

                    }
                }
            }

            if (GameManager.Inst.m_GameState == GameState.End)
            {
                this.transform.position = DeathVec;
            }
        }
    }

    public void TakeDamage(float a_Damage, int AttackerId)
    {
        if (pv.Owner.CustomProperties.ContainsKey("curHp") == true)
            m_CurHp = (float)pv.Owner.CustomProperties["curHp"];

        //if (AttackerId == playerId) //자기가 쏜 총알은 자신이 맞으면 안되기 때문에...
        //    return;

        m_CurHp -= a_Damage;

        if (m_CurHp < 0)
            m_CurHp = 0;
        //자신의 저장 공간의 값을 갱신해서 브로드 케이팅
        int a_AttPlayerID = -1;

        if (m_CurHp <= 0)  //OtherPc일때 이쪽에서 작동
        {
            a_AttPlayerID = AttackerId;
        }

        MakeCurHpPacket(m_CurHp, a_AttPlayerID);
        pv.Owner.SetCustomProperties(CurrHpProps);  //브로드 케이팅
    }

    void MakeCurHpPacket(float CurHP = 100, int a_LAtt_ID = -1)
    {
        if (CurrHpProps.ContainsKey("curHp") == true) //모든 캐릭터의 에너지바 동기화
        {
            CurrHpProps["curHp"] = CurHP;
        }
        else
        {
            CurrHpProps.Add("curHp", CurHP);
        }

        //내가 죽을 때 막타친 유저를 찾아서 킬수를 올려줌
        if (CurrHpProps.ContainsKey("LastAttackerID") == true) 
        {
            CurrHpProps["LastAttackerID"] = a_LAtt_ID;
        }
        else
        {
            CurrHpProps.Add("LastAttackerID", a_LAtt_ID);
        }
    }

    void SaveKillCount(int firePlayerId)
    {
        Debug.Log("SaveKill1");
        //Player 태그를 지정된 모든 플레이어를 가져와 배열에 저장
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            var playerDamage = player.GetComponent<PlayerDamage>();
            //player의 playerId가 포탄의 playerId(쏜사람)와 동일한지 체크
            if (playerDamage != null && playerDamage.playerId == firePlayerId)
            {
                Debug.Log("SaveKill2");

                //동일한 player일 경우 스코어를 증가
                playerDamage.IncKillCount();
                return;
            }
        }
    }
    
    void IncKillCount()
    {
        Debug.Log("IncKilll");

        //------------마스터 클라이언트가 동기화
        if (PhotonNetwork.IsMasterClient == true)    //마스터 클라이언트만 중계한다.
        {
            if (pv.Owner.CustomProperties.ContainsKey("KillCount") == true)
            {
                killCount = (int)pv.Owner.CustomProperties["KillCount"];
                killCount++;
                Debug.Log(pv.Owner + "killCount++");

                MakeKillPacket(killCount);  //Killcount 동기화
                pv.Owner.SetCustomProperties(KillProps);    //브로드 케스팅
            }
        }//if(PhotonNetwork.IsMasterClient == true)  
    }

    void MakeKillPacket(int a_KillCount = 0)
    {
        if (KillProps == null)
        {
            KillProps = new ExitGames.Client.Photon.Hashtable();
            KillProps.Clear();
        }

        if (KillProps.ContainsKey("KillCount") == true)
            KillProps["KillCount"] = a_KillCount;
        else
            KillProps.Add("KillCount", a_KillCount);
    }

    
    [PunRPC]
    void SetVisible(bool isVisible)
    {
        foreach (MeshRenderer _renderer in m_WeaponRenders) //무기
        {
            _renderer.enabled = isVisible;
        }

        foreach (SkinnedMeshRenderer _renderer in m_BodyRenders) //캐릭터
        {
            _renderer.enabled = isVisible;
        }

        Rigidbody[] a_Rigidbody = this.GetComponentsInChildren<Rigidbody>(true);
        foreach (Rigidbody _Rigidbody in a_Rigidbody)
        {
            _Rigidbody.isKinematic = !isVisible;
        }

        BoxCollider[] a_BoxColls = this.GetComponentsInChildren<BoxCollider>(true);
        foreach (BoxCollider _BoxColl in a_BoxColls)
        {
            _BoxColl.enabled = isVisible;
        }
    }

    void Die()
    {
        if (m_CurHp > 0 || a_OldcurHp <= 0)
            return;

        DeathVec = this.transform.position;

        m_RefGameMgr.RankPlayer(killCount);
        pv.RPC("SetVisible", RpcTarget.All, false);
        GameObject a_Ragdoll = PhotonNetwork.Instantiate(RagDollModel.name, this.transform.position, Quaternion.identity) as GameObject;
        m_PlayerCtrl.m_MainCamera.gameObject.GetComponent<CameraCtrl>().m_Plyaer = a_Ragdoll;
        a_Ragdoll.GetComponent<RagdollCtrl>().CopyDefaultModelToRagdoll(DefaultModel.transform, a_Ragdoll.transform, DefaultWeapon);
    }

    [PunRPC]
    public void AddHealing(float Heal)
    {
        if (pv.IsMine == false) //자기 조종하고 있는 탱크 기준으로만
            return;
        //pv.Owner : 이 게임 오브젝트 기준의 네트웍크 세션 소유자
        //pv.Owner.CustomProperties : Dictionary 같은 저장 공간
        if (pv.Owner.CustomProperties.ContainsKey("curHp") == true)
        {
            m_CurHp = (float)pv.Owner.CustomProperties["curHp"];
        }
        m_CurHp += 30;
        if (100 < m_CurHp)
            m_CurHp = 100;
        MakeCurHpPacket(m_CurHp);
        pv.Owner.SetCustomProperties(CurrHpProps);  //브로드 케이팅 <--//이걸 해 줘야 브로드 케이팅 된다.
    }
}
