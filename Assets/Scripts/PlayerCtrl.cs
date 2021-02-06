using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class PlayerCtrl : MonoBehaviourPunCallbacks, IPunObservable
{
    Rigidbody rigid;
    Transform tr;

    PlayerAudioCtrl m_playerAudioCtrl;

    float m_MoveVelocity = 3.0f;    //이동 속도

    //--------------------------점프관련 변수
    public float m_JumpForce = 30.0f;   //점프
    int m_JumCount = 0;
    //--------------------------점프관련 변수

    //---------키보드 입력값 변수
    float h = 0, v = 0;
    float moveSpeed = 5.0f;
    //---------키보드 입력값 변수

    //수류탄 관련 변수
    public GameObject GrenadePrefab = null;
    public Transform m_grenadePoint = null;
    bool m_isThrow;
    public int m_Grenades = 0;

    public MeshRenderer[] m_WeaponRenders;
    public GameObject[] m_Weapons;
    //수류탄 관련 변수

    //LimitMove
    Vector3 HalfSize = Vector3.zero;

    float a_LmtBdLeft = 0;
    float a_LmtBdTop = 0;
    float a_LmtBdRight = 0;
    float a_LmtBdBottom = 0;

    Vector3 m_CacCurPos = Vector3.zero;
    //LimitMove


    //---------애니메이션 변수
    Animator Anim;
    [HideInInspector] public bool m_Crouch = false;
    [HideInInspector] public bool m_Prone = false;
    [HideInInspector] public bool m_isReload = false;
    //---------애니메이션 변수

    //Camera Controll
    public Camera m_MainCamera;
    public GameObject m_Spin;
    [HideInInspector] public float Y = 0f;
    public SkinnedMeshRenderer m_BodyRender = null;
    //Camera Controll
    
    //Map
    GameObject m_MiniMapCamera;
    //Map


    //WeaponChange
    public WeaponCtrl m_WeaponCtrl;
    bool m_AKMSkinable = true;
    bool a_AKMSkinable = true;
    bool m_PistolSkinable = false;
    bool a_PistolSkinable = false;

    [HideInInspector] public float m_WpDelay = 2f;
    [HideInInspector] public bool m_WpChange = false;
    //WeaponChange

    //PhotonView 컴포넌트를 할당할 변수
    [HideInInspector] public PhotonView pv = null;

    //위치 정보를 송수신할 때 사용할 변수 선언 및 초깃값 설정
    private Vector3 currPos = Vector3.zero;
    private Quaternion currRot = Quaternion.identity;
    private Quaternion spinRot = Quaternion.identity;

    GameManager m_RefGameMgr = null;
    [HideInInspector] public bool m_isFire = false;
    [HideInInspector] public bool m_isAiming = false;

    void Awake()
    {
        tr = GetComponent<Transform>();

        pv = GetComponent<PhotonView>();

        m_playerAudioCtrl = GetComponentInChildren<PlayerAudioCtrl>();

        if (pv.IsMine)
        {
            m_MainCamera = Camera.main;
            m_MainCamera.transform.SetParent(m_Spin.transform);
            m_MainCamera.transform.localPosition = new Vector3(-0.97f, 0.25f, -0.08f);
            m_MainCamera.transform.localRotation = Quaternion.Euler(new Vector3(-96.35f, 90f, 0));

            m_MiniMapCamera = GameObject.Find("MiniMapCamera");
            m_MiniMapCamera.transform.position = new Vector3(this.tr.position.x, 45, this.tr.position.z);
            m_MiniMapCamera.GetComponent<MiniMapCameraCtrl>().m_Player = this;

            m_BodyRender.gameObject.layer = LayerMask.NameToLayer("Body");
        }
        //원격 캐릭터의 위치 및 회전 값을 처리할 변수의 초깃값 설정
        currPos = tr.position;
        currRot = tr.rotation;
        spinRot = m_Spin.transform.localRotation;
    }

    // Start is called before the first frame update
    void Start()
    {
        Anim = this.gameObject.GetComponentInChildren<Animator>();
        rigid = this.gameObject.GetComponent<Rigidbody>();
        tr = this.gameObject.GetComponent<Transform>();

        m_JumpForce = 50f;
        moveSpeed = 5f;

        if ( pv.IsMine)
        {
            GameObject a_GObj = GameObject.Find("GameManager");
            if (a_GObj != null)
            {
                m_RefGameMgr = a_GObj.GetComponent<GameManager>();
                if (m_RefGameMgr != null)
                {
                    m_RefGameMgr.m_RefPlayer = this;
                }
            }//if(a_GObj != null)
        }//if (pv.IsMine)
    }
    
    float a_CacPosLen = 0.0f;
    // Update is called once per frame
    void Update()
    {
        LimitMove();

        if (pv.IsMine)
        {
            if (m_RefGameMgr.m_GameState != GameState.Start)
                return;

            if (!m_RefGameMgr.bEnter)
            {
                keyBDMove();
                Crouch();   //앉기
                Jump();     //점프
                Prone();    //엎드리기
                WeaponChange(); //무기변경

                if (Input.GetMouseButton(0))    //발사
                {
                    WeaponCtrl a_Wp = m_WeaponCtrl;
                    if (a_Wp.m_currentBullets > 0)
                        FireAnim();
                    else
                        DoReload();
                }
                if (Input.GetMouseButtonUp(0))
                    m_isFire = false;

                if (Input.GetKeyDown(KeyCode.R))    //재장전
                    DoReload();

                if (Input.GetKeyDown(KeyCode.V))    //수류탄 던지기
                    DoThrowGrenade();

                CamRote();  //캐릭터 회전
            }
        }
        else
        {
            a_CacPosLen = (tr.position - currPos).magnitude;
            if (10.0f < a_CacPosLen)
            {
                tr.position = currPos;
            }
            else
            {
                //원격 플레이어의 Hero를 수신받은 위치까지 부드럽게 이동시킴
                tr.position = Vector3.Lerp(tr.position, currPos, Time.deltaTime * 10.0f);
            }
            //원격 플레이어의 Hero를 수신받은 각도만큼 부트럽게 회전시킴
            tr.rotation = Quaternion.Slerp(tr.rotation, currRot, Time.deltaTime * 10.0f);
            m_Spin.transform.localRotation = Quaternion.Slerp(m_Spin.transform.localRotation, spinRot, Time.deltaTime * 10.0f);

            //m_AKMSkinable = a_AKMSkinable;
            //m_PistolSkinable = a_PistolSkinable;
        }
        //m_Hpbar.fillAmount = m_CurHp / m_MaxHp;
    }

    void CamRote()
    {
        //-----카메라 회전
        Y += Input.GetAxis("Mouse Y");
        if (!m_Prone)
            Y = Mathf.Clamp(Y, -40, 40);
        else
            Y = Mathf.Clamp(Y, -4, 13);

        tr.Rotate(new Vector3(0, Input.GetAxis("Mouse X"), 0));

        if(!m_Prone)
            m_Spin.transform.localEulerAngles = new Vector3(30f, 0, Y);
        else
            m_Spin.transform.localEulerAngles = new Vector3(0, 0, Y);
        //-----카메라 회전
    }


    void keyBDMove()
    {
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");

        Vector3 moveDir = (Vector3.forward * v) + (Vector3.right * h);

        if (h != 0 || v != 0)
        {
            tr.Translate(moveDir.normalized * moveSpeed * Time.deltaTime, Space.Self);

            //-------애니메이션 처리
            if (v != 0)
            {
                Anim.SetBool("Right Walk", false);
                Anim.SetBool("Left Walk", false);

                if (v > 0)  //전진
                {
                    if (Input.GetKeyUp("left shift"))
                    {
                        Anim.SetBool("Run", false);
                        moveSpeed = 5f;
                    }

                    if (Input.GetKey("left shift"))
                    {
                        if (m_Prone)    //누워있으면 달릴수없다
                            return;

                        Anim.SetBool("Run", true);
                        moveSpeed = 7.5f;
                    }
                    else
                        Anim.SetBool("Forward Walk", true);

                    Anim.SetFloat("speed", 1);

                }
                if (v < 0)      //후진
                {
                    Anim.SetBool("Back Walk", true);
                    Anim.SetFloat("speed", -1);
                }
            } 
            else if (h != 0)
            {
                Anim.SetBool("Forward Walk", false);
                Anim.SetBool("Back Walk", false);

                if (h > 0)  //오른쪽
                {
                    Anim.SetBool("Right Walk", true);
                    Anim.SetFloat("speed", 1);
                }
                if (h < 0)    //왼쪽
                {
                    Anim.SetBool("Left Walk", true);
                    Anim.SetFloat("speed", -1);
                }
            }

        }
        else    //Idle
        {
            Anim.SetBool("Forward Walk", false);
            Anim.SetBool("Back Walk", false);
            Anim.SetBool("Right Walk", false);
            Anim.SetBool("Left Walk", false);

            if (m_Prone)
                m_playerAudioCtrl.WalkaudioSource.Stop();

        }
        //-------애니메이션 처리        
    }

    void Crouch()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            m_playerAudioCtrl.Sit_Prone_StandUp();

            if(m_Prone)//눕기중일경우
            m_Prone = false;

            m_Crouch = !m_Crouch;

            if (m_Crouch == true)
            {
                Anim.SetTrigger("Crouch");
                moveSpeed = 2.5f;
            }
            else
            {
                Anim.SetTrigger("Idle");
                moveSpeed = 5f;
            }
        }
    }
    void Prone()    //엎드리기
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            m_playerAudioCtrl.Sit_Prone_StandUp();

            if (m_Crouch)//앉기중일경우
                m_Crouch = false;

            m_Prone = !m_Prone;

            if (m_Prone)
            {
                Anim.SetTrigger("Prone");
                moveSpeed = 1.8f;
                m_Spin.transform.localEulerAngles = new Vector3(0, 0, -2.587f);
            }
            else
            {
                Anim.SetTrigger("Idle");
                moveSpeed = 5f;
            }
        }
    }
       

    void Jump()
    {        
        if (Input.GetKeyDown(KeyCode.Space) && m_JumCount <= 0)
        {
            if (m_Prone || m_Crouch)
            {
                if (m_Prone)
                {   //누워있으면 앉기
                    m_Crouch = true;
                    m_Prone = false;
                    Anim.SetTrigger("Crouch");
                    moveSpeed = 2.5f;
                }
                else//앉아있으면 일어나기
                {
                    m_Crouch = false;
                    Anim.SetTrigger("Idle");
                    moveSpeed = 5f;
                }

                return;
            }

            rigid.AddForce(0, m_JumpForce, 0, ForceMode.Impulse);
            m_JumCount++;
        }
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name.Contains("Ground"))    //이단 점프 방지
        {
            m_JumCount = 0;
        }
    }

    void FireAnim()
    {
        if (m_isReload)
            return;

        if (m_RefGameMgr.m_weapon == Weapon.Assault57)
        {
            if (m_Prone == true)
                Anim.SetTrigger("Prone Shoot");
            else
                Anim.SetTrigger("Shoot");

        }
        else if (m_RefGameMgr.m_weapon == Weapon.Pistol)
        {
            if (m_Prone == true)
                Anim.SetTrigger("Pistol Prone Shoot");
            else
                Anim.SetTrigger("Pistol Shoot");
        }
    }

    public void DoReload()
    {
        WeaponCtrl a_Wp = m_WeaponCtrl;
        if (!m_isReload && a_Wp.m_currentBullets < a_Wp.m_bulletsPerMag && a_Wp.m_bulletsTotal > 0)
        {
            if(!m_Prone)
                Anim.SetTrigger("Reload");
            else
                Anim.SetTrigger("Prone Reload");

            if (m_RefGameMgr.m_weapon == Weapon.Assault57)
            {
                if (!m_Prone)
                    a_Wp.a_ReloadStr = "Reload";
                else
                    a_Wp.a_ReloadStr = "Prone Reload";
            }
            else
            {
                if (!m_Prone)
                    a_Wp.a_ReloadStr = "Pistol Reload";
                else
                    a_Wp.a_ReloadStr = "Pistol Prone Reload";
            }
        }
    }

    void LimitMove()
    {
        m_CacCurPos = transform.position;

        a_LmtBdLeft = -150f;
        a_LmtBdRight = 150f;
        a_LmtBdTop = -110f;
        a_LmtBdBottom = 110f;

        if (m_CacCurPos.z < (float)a_LmtBdLeft)
            m_CacCurPos.z = (float)a_LmtBdLeft;

        if ((float)a_LmtBdRight < m_CacCurPos.x)
            m_CacCurPos.z = (float)a_LmtBdRight;

        if (m_CacCurPos.x < (float)a_LmtBdTop)
            m_CacCurPos.x = (float)a_LmtBdTop;

        if ((float)a_LmtBdBottom < m_CacCurPos.x)
            m_CacCurPos.x = (float)a_LmtBdBottom;

        transform.position = m_CacCurPos;
    }

    [PunRPC]
    public void AddGrenadeCount()
    {
        if (pv.IsMine == false) 
            return;   

        m_Grenades++;
        m_RefGameMgr.GrenadeCountTxt.text = m_Grenades.ToString();
    }

    float a_Delay = 2f;
    void DoThrowGrenade()
    {
        if (m_Grenades <= 0)
            return;

        if(!m_isThrow && m_Grenades > 0)
        {//수류탄 던지기

            if (!m_Prone)
                Anim.SetTrigger("Grenade");
            else
                Anim.SetTrigger("Prone Grenade");

            //GameObject genade = PhotonNetwork.Instantiate(GrenadePrefab.name, m_grenadePoint.position, Quaternion.identity) as GameObject;
        }        

    }

    public void SetAKM(bool isEnabled)
    {
        pv.RPC("SetAKMRenders", RpcTarget.All, isEnabled);
    }

    public void SetPistol(bool isEnabled)
    {
        pv.RPC("SetPistolRenders", RpcTarget.All, isEnabled);
    }


    [PunRPC]
    public void CreateGrenadePrefab()
    {
        GameObject genade = PhotonNetwork.Instantiate(GrenadePrefab.name, m_grenadePoint.position, Quaternion.identity) as GameObject;
        m_Grenades--;
        m_RefGameMgr.GrenadeCountTxt.text = m_Grenades.ToString();
    }

    void WeaponChange()
    {
        if (!m_WpChange)    //중간에 무기를 바꿀수없게 막아줌
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                m_RefGameMgr.m_weapon = Weapon.Assault57;
                m_WeaponCtrl = m_Weapons[1].GetComponent<WeaponCtrl>();
                m_WpChange = true;
                Anim.SetBool("Weapon Change", true);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                m_RefGameMgr.m_weapon = Weapon.Pistol;
                m_WeaponCtrl = m_Weapons[0].GetComponent<WeaponCtrl>();
                m_WpChange = true;
                Anim.SetBool("Weapon Change", true);
            }
        }

        if (m_WpChange)
            m_WpDelay -= Time.deltaTime;
        if (m_WpDelay <= 0.5f)
        {
            m_WpDelay = 2f;
            m_WpChange = false;
            Anim.SetBool("Weapon Change", false);
        }

        if (m_RefGameMgr.m_weapon == Weapon.Assault57)
        {
            if (m_WpDelay <= 1)
            {
                pv.RPC("SetAKMRenders", RpcTarget.All, true);
                pv.RPC("SetPistolRenders", RpcTarget.All, false);
            }
            Anim.SetBool("Assault", true);
            Anim.SetBool("Pistol", false);
            m_RefGameMgr.m_WeaponIcon[0].gameObject.SetActive(true);
            m_RefGameMgr.m_WeaponIcon[1].gameObject.SetActive(false);
        }
        else if (m_RefGameMgr.m_weapon == Weapon.Pistol)
        {
            if (m_WpDelay <= 1)
            {
                pv.RPC("SetAKMRenders", RpcTarget.All, false);
                pv.RPC("SetPistolRenders", RpcTarget.All, true);
            }
            Anim.SetBool("Assault", false);
            Anim.SetBool("Pistol", true);
            m_RefGameMgr.m_WeaponIcon[1].gameObject.SetActive(true);
            m_RefGameMgr.m_WeaponIcon[0].gameObject.SetActive(false);
        }
    }

    [PunRPC]
    void SetAKMRenders(bool isEnabled)
    {
        m_AKMSkinable = isEnabled;

        m_WeaponRenders[1].enabled = m_AKMSkinable;

        if (m_AKMSkinable)
            m_Weapons[1].GetComponent<WeaponCtrl>().m_weaponState = weaponState.Active;
        else
            m_Weapons[1].GetComponent<WeaponCtrl>().m_weaponState = weaponState.Deactive;
    }

    [PunRPC]
    void SetPistolRenders(bool isEnabled)
    {
        Debug.Log("PistolMeshOnOff");
        m_PistolSkinable = isEnabled;
        m_WeaponRenders[0].enabled = m_PistolSkinable;

        if (m_PistolSkinable)
            m_Weapons[0].GetComponent<WeaponCtrl>().m_weaponState = weaponState.Active;
        else
            m_Weapons[0].GetComponent<WeaponCtrl>().m_weaponState = weaponState.Deactive;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(tr.position);
            stream.SendNext(tr.rotation);
            stream.SendNext(m_Spin.transform.localRotation);

        }
        else 
        {
            currPos = (Vector3)stream.ReceiveNext();
            currRot = (Quaternion)stream.ReceiveNext();
            spinRot = (Quaternion)stream.ReceiveNext();
        }
    }
}
