using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.UI;
public enum TrainingState
{
    Play,
    Setting,
}

public enum TRWeapon
{
    Assault57,
    Pistol
}

public class TrainingMgr : MonoBehaviour
{
    //싱글턴 패턴을 위한 인스턴스 변수 선언
    public static TrainingMgr Inst = null;
    public TrainingState m_TrainingState = TrainingState.Play;

    public TRWeapon m_weapon = TRWeapon.Assault57;
    public TrainPlayerCtrl m_RefPlayer;
    Animator m_PlayerAnim = null;

    public GameObject m_Pistol = null;
    public GameObject m_Assault57 = null;
    public TrainWeaponCtrl m_WeaponCtrl;

    public float m_WpDelay = 2f;
    [HideInInspector] public bool m_WpChange = false;

    public RawImage[] m_WeaponIcon;

    //환경설정 변수
    public GameObject Canvas_Dialog = null;
    private GameObject m_ConfigBoxObj = null;
    //환경설정 변수

    [Header("Recoil")]
    public Image[] m_CrossHair = new Image[4];


    [Header("Audio")]
    [SerializeField] private string UIClick_Sound;



    private void Awake()
    {
        Inst = this;

        Application.targetFrameRate = 60;
        Cursor.lockState = CursorLockMode.Locked; //커서 고정

        CreatePlayer();
    }

    // Start is called before the first frame update
    void Start()
    {
        SoundMgr.Instance.m_audioSouce.Stop();
        SoundMgr.Instance.m_audioSouce.clip = null;


        if (m_RefPlayer != null)
            m_PlayerAnim = m_RefPlayer.GetComponentInChildren<Animator>();
        
    }

    // Update is called once per frame
    void Update()
    {
        WeaponChange();

        if (m_RefPlayer.m_isFire)
            MoveCrosshair();
        else
            MoveBackCrosshair();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.Confined;
            m_TrainingState = TrainingState.Setting;

            if (m_ConfigBoxObj == null)
                m_ConfigBoxObj = Resources.Load("Prefab/SettingPrefab") as GameObject;

            GameObject a_CfgBoxObj = (GameObject)Instantiate(m_ConfigBoxObj);
            a_CfgBoxObj.transform.SetParent(Canvas_Dialog.transform, false);
        }
    }

    void WeaponChange()
    {
        if (m_PlayerAnim == null)
            m_PlayerAnim = m_RefPlayer.GetComponentInChildren<Animator>();

        if (!m_WpChange)    //중간에 무기를 바꿀수없게 막아줌
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                m_weapon = TRWeapon.Assault57;
                m_WeaponCtrl = m_Assault57.GetComponent<TrainWeaponCtrl>();
                m_WpChange = true;
                m_PlayerAnim.SetBool("Weapon Change", true);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                m_weapon = TRWeapon.Pistol;
                m_WeaponCtrl = m_Pistol.GetComponent<TrainWeaponCtrl>();
                m_WpChange = true;
                m_PlayerAnim.SetBool("Weapon Change", true);
            }
        }

        if (m_WpChange)
            m_WpDelay -= Time.deltaTime;
        if (m_WpDelay <= 0.5f)
        {
            m_WpDelay = 2f;
            m_WpChange = false;
            m_PlayerAnim.SetBool("Weapon Change", false);
        }

        if (m_weapon == TRWeapon.Assault57)
        {
            if (m_WpDelay <= 1)
            {
                m_Assault57.SetActive(true);
                m_Pistol.SetActive(false);
            }
            m_PlayerAnim.SetBool("Assault", true);
            m_PlayerAnim.SetBool("Pistol", false);
            m_WeaponIcon[0].gameObject.SetActive(true);
            m_WeaponIcon[1].gameObject.SetActive(false);
        }
        else if (m_weapon == TRWeapon.Pistol)
        {
            if (m_WpDelay <= 1)
            {
                m_Assault57.SetActive(false);
                m_Pistol.SetActive(true);
            }
            m_PlayerAnim.SetBool("Assault", false);
            m_PlayerAnim.SetBool("Pistol", true);
            m_WeaponIcon[1].gameObject.SetActive(true);
            m_WeaponIcon[0].gameObject.SetActive(false);
        }

    }
    void CreatePlayer()
    {
        Vector3 a_HPos = Vector3.zero;
        Instantiate(Resources.Load("TrainingPlayer"), a_HPos, Quaternion.identity);        
    }

    void MoveCrosshair()
    {
        foreach (Image a_CrossHair in m_CrossHair)
        {
            Vector3 a_MoveVec = new Vector3(0, 40, 0);
            a_CrossHair.transform.localPosition = Vector3.Lerp(a_CrossHair.transform.localPosition, a_MoveVec, Time.deltaTime * 15f);
        }
    }

    void MoveBackCrosshair()
    {
        foreach (Image a_CrossHair in m_CrossHair)
        {
            Vector3 a_MoveVec = new Vector3(0, 20, 0);
            a_CrossHair.transform.localPosition = Vector3.Lerp(a_CrossHair.transform.localPosition, a_MoveVec, Time.deltaTime * 15f);
        }
    }

    void OnClickExitRoom()
    {
        
    }
}
