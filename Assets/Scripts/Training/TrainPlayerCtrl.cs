using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TrainPlayerCtrl : MonoBehaviour
{
    Rigidbody rigid;
    Transform tr;

    PlayerAudioCtrl m_playerAudioCtrl;

    //--------------------------점프관련 변수
    public float m_JumpForce = 30.0f;   //점프
    int m_JumCount = 0;
    //--------------------------점프관련 변수

    //---------키보드 입력값 변수
    float h = 0, v = 0;
    float moveSpeed = 5.0f;
    //---------키보드 입력값 변수

    //---------애니메이션 변수
    Animator Anim;

    [HideInInspector] public bool m_Crouch = false;
    [HideInInspector] public bool m_Prone = false;
    [HideInInspector] public bool m_isReload = false;

    //---------애니메이션 변수

    //Camera Controll
    Camera m_MainCamera;
    public GameObject m_Spin;
    [HideInInspector] public float Y = 0f;
    public SkinnedMeshRenderer m_BodyRender = null;
    //Camera Controll

    TrainingMgr m_RefGameMgr = null;
    [HideInInspector] public bool m_isFire = false;
    [HideInInspector] public bool m_isAiming = false;

    void Awake()
    {
        tr = GetComponent<Transform>();

        m_playerAudioCtrl = GetComponentInChildren<PlayerAudioCtrl>();

        m_MainCamera = Camera.main;
        m_MainCamera.transform.SetParent(m_Spin.transform);
        m_MainCamera.transform.localPosition = new Vector3(-0.97f, 0.25f, -0.08f);
        m_MainCamera.transform.localRotation = Quaternion.Euler(new Vector3(-96.35f, 90f, 0));

        m_BodyRender.gameObject.layer = LayerMask.NameToLayer("Body");

    }

    // Start is called before the first frame update
    void Start()
    {
        Anim = this.gameObject.GetComponentInChildren<Animator>();
        rigid = this.gameObject.GetComponent<Rigidbody>();
        tr = this.gameObject.GetComponent<Transform>();

        m_JumpForce = 50f;
        moveSpeed = 5f;
               
        GameObject a_GObj = GameObject.Find("TrainingMgr");
        if (a_GObj != null)
        {
            m_RefGameMgr = a_GObj.GetComponent<TrainingMgr>();
            if (m_RefGameMgr != null)
            {
                m_RefGameMgr.m_RefPlayer = this;
            }
        }//if(a_GObj != null)
    }

    float a_CacPosLen = 0.0f;
    // Update is called once per frame
    void Update()
    {
        if (TrainingMgr.Inst.m_TrainingState != TrainingState.Play)
            return;

        keyBDMove();
        Crouch();   //앉기
        Jump();
        Prone();    //엎드리기

        if (Input.GetMouseButton(0))
        {
            TrainWeaponCtrl a_Wp = m_RefGameMgr.m_WeaponCtrl;
            if (a_Wp.m_currentBullets > 0)
                FireAnim();
            else
                DoReload();
        }
        if (Input.GetMouseButtonUp(0))
            m_isFire = false;

        if (Input.GetKeyDown(KeyCode.R))
            DoReload();

        CamRote();
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

        if (!m_Prone)
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

            if (m_Prone)//눕기중일경우
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
        //if (collision.gameObject.name.Contains("Ground"))    //이단 점프 방지
        //{
            m_JumCount = 0;
        //}
    }
  
    void FireAnim()
    {
        if (m_isReload)
            return;

        if (m_RefGameMgr.m_weapon == TRWeapon.Assault57)
        {
            if (m_Prone == true)
                Anim.SetTrigger("Prone Shoot");
            else
                Anim.SetTrigger("Shoot");

        }
        else if (m_RefGameMgr.m_weapon == TRWeapon.Pistol)
        {
            if (m_Prone == true)
                Anim.SetTrigger("Pistol Prone Shoot");
            else
                Anim.SetTrigger("Pistol Shoot");
        }
    }

    public void DoReload()
    {
        TrainWeaponCtrl a_Wp = m_RefGameMgr.m_WeaponCtrl;
        if (!m_isReload && a_Wp.m_currentBullets < a_Wp.m_bulletsPerMag && a_Wp.m_bulletsTotal > 0)
        {
            if (!m_Prone)
                Anim.SetTrigger("Reload");
            else
                Anim.SetTrigger("Prone Reload");

            if (m_RefGameMgr.m_weapon == TRWeapon.Assault57)
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
}
