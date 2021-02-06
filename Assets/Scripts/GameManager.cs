using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Photon.Pun;
using Photon.Realtime;
using PlayFab;
using PlayFab.ClientModels;

public enum GameState
{
    Loading,
    Start,
    Setting,
    Map,
    End,
}

public enum Weapon
{
    Assault57,
    Pistol
}

public class GameManager : MonoBehaviourPunCallbacks, IPunObservable
{
    //싱글턴 패턴을 위한 인스턴스 변수 선언
    public static GameManager Inst = null;

    private PhotonView pv;

    public GameState m_GameState = GameState.Loading;
    
    //Weapon
    public Weapon m_weapon = Weapon.Assault57;
    //public MeshRenderer[] m_AKMRenders;
    //public MeshRenderer[] m_PistolRenders;
    //Weapon

    public Canvas DefaultCanvas;
    public Canvas GameEndCanvas;

    [HideInInspector] public PlayerCtrl m_RefPlayer;
    Animator m_PlayerAnim = null;

    [HideInInspector] public PlayerDamage m_RefDamage;


    public GameObject m_Pistol = null;
    public GameObject m_Assault57 = null;
    //public WeaponCtrl m_WeaponCtrl;

    bool m_AKMSkinable = true;
    bool a_AKMSkinable = true;
    bool m_PistolSkinable = false;
    bool a_PistolSkinable = false;

    [HideInInspector] public float m_WpDelay = 2f;
    [HideInInspector] public bool m_WpChange = false;

    public RawImage[] m_WeaponIcon;

    public Image DelayPanel;

    //접속 로그를 표시할 Text UI 항목 변수 (채팅)
    public Text txtLogMsg;
    public InputField textChat;
    public bool bEnter = false;
    //접속 로그를 표시할 Text UI 항목 변수 (채팅)

    //환경설정 변수
    public GameObject Canvas_Dialog = null;
    private GameObject m_ConfigBoxObj = null;
    //환경설정 변수

    [Header("Recoil")]
    public Image[] m_CrossHair = new Image[4];

    [Header("Rank")]
    int m_TotalPlayer;
    public Text PlayerNumTxt;
    public Text KillTxt;
    public int m_Rank;
    bool m_Win = false;
    int m_RewardGold;

    //--GameEnd
    [Header("GameEnd")]
    public Image GameEndPanel;
    public Text UserNick;
    public Text MentTxt;
    public Text RankTxt;
    public Text TotalTxt;
    public Text TimeTxt;
    public Button LobbyBtn;

    private float EndTimer = 31f;
    //--GameEnd

    [Header("Genade")]
    public Text GrenadeCountTxt;

    [Header("Map")]
    public Camera m_MainCamera;
    public MapCameraCtrl m_MapCtrl;
    [HideInInspector] public bool MapActive = false;


    private void OnApplicationFocus(bool focus)  //윈도우 창 활성화 비활성화 일때
    {
        PhotonInit.isFocus = focus;
    }

    private void Awake()
    {
        Inst = this;

        Application.targetFrameRate = 60;
        Cursor.lockState = CursorLockMode.Locked; //커서 고정
                                                 
        pv = GetComponent<PhotonView>();
        CreatePlayer();
    }

    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.IsMessageQueueRunning = true;

        if (pv.IsMine)
        {
            if (m_RefPlayer != null)
                m_PlayerAnim = m_RefPlayer.GetComponentInChildren<Animator>();

            UserNick.text = GlobalValue.g_NickName;
        }

        if (LobbyBtn != null)
            LobbyBtn.onClick.AddListener(OnClickExitRoom);

        //현재 입장한 룸 정보를 받아옴
        Room currRoom = PhotonNetwork.CurrentRoom;
        m_TotalPlayer = currRoom.PlayerCount;
        m_Rank = m_TotalPlayer;
        PlayerNumTxt.text = m_Rank + " 생존";

    }

    float a_Delay = 3f;
    // Update is called once per frame
    void Update()
    {
        if(DelayPanel!= null)
        if (DelayPanel.gameObject.activeSelf)
        {
            if (a_Delay > 0)
            {
                a_Delay -= Time.deltaTime;
            }
            else
            {
                DelayPanel.gameObject.SetActive(false);
                m_GameState = GameState.Start;
            }
        }

        SetConnectPlayerScore();

        if (m_GameState == GameState.Start)
        {
            if (m_RefPlayer.m_isFire)
                MoveCrosshair();
            else
                MoveBackCrosshair();

            if(Input.GetKeyDown(KeyCode.Escape))
            {
                m_GameState = GameState.Setting;
                Cursor.lockState = CursorLockMode.None; 

                if (m_ConfigBoxObj == null)
                    m_ConfigBoxObj = Resources.Load("Prefab/SettingPrefab") as GameObject;

                GameObject a_CfgBoxObj = (GameObject)Instantiate(m_ConfigBoxObj);
                a_CfgBoxObj.transform.SetParent(Canvas_Dialog.transform, false);
            }

            if (KillTxt.gameObject.activeSelf)
                {
                    float delay = 3f;
                    if (delay > 0)
                        delay -= Time.deltaTime;
                    else
                        KillTxt.gameObject.SetActive(false);
                }

            if (m_Rank <= 1)
                RankPlayer();
        }

        if (m_GameState ==GameState.Start || m_GameState == GameState.Map)
        {
            if (Input.GetKeyDown(KeyCode.M))    //지도
            {
                MapActive = !MapActive;
                m_MapCtrl.gameObject.SetActive(MapActive);
                m_MainCamera.gameObject.SetActive(!MapActive);
                Canvas_Dialog.gameObject.SetActive(!MapActive);

                if (MapActive)
                {
                    m_GameState = GameState.Map;
                    Cursor.lockState = CursorLockMode.Confined;
                }
                else
                {
                    m_GameState = GameState.Start;
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }
        }
        
        Chatting();
        GameEnd();

        PlayerNumTxt.text = m_Rank + " 생존";
    }

    void CreatePlayer()
    {
        Vector3 a_HPos = Vector3.zero;
        Vector3 a_AddPos = Vector3.zero;

        a_AddPos.x = Random.Range(-90.0f, 90.0f);
        a_AddPos.z = Random.Range(-140.0f, 140.0f);
        a_AddPos.y = 25f;
        a_HPos = a_AddPos;
        
        PhotonNetwork.Instantiate("MultyPlayer", a_HPos, Quaternion.identity, 0);
       
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

    void EnterChat()
    {
        string msg = "";

        msg = "\n<color=#ffffff>[" + GlobalValue.g_NickName + "] : " + textChat.text + "</color>";

        pv.RPC("LogMsg", RpcTarget.AllBuffered, msg);
        textChat.text = "";
    }

    [PunRPC]
    void LogMsg(string msg)
    {
        //로그 메시지 Text UI에 텍스트를 누적시켜 표시
        txtLogMsg.text = txtLogMsg.text + msg;
    }

    public void OnClickExitRoom()
    {
        Cursor.lockState = CursorLockMode.None;

        //로그 메시지에 출력할 문자열 생성
        string msg = "\n<color=#ff0000>[" + GlobalValue.g_NickName +
            "] Disconnected </color>";

        //RPC 함수 호출
        pv.RPC("LogMsg", RpcTarget.AllBuffered, msg);
        //설정이 완료된 후 빌드파일을 여러개 실행해 
        //동일한 룸에 입장해 보면 접속로그가 표기되는 것을 확인할 수 있다.
        //또한 PhotonTarget.AllBuffered 옵션으로
        //RPC를 호출했기 떄문에 나중에 입장해도 기존의 접속로그 메시지가 표시된다.

        //마지막 사람이 방을 떠날때 룸의 CustomProperties를 초기화 해주어야 한다.
        if (PhotonNetwork.PlayerList != null && PhotonNetwork.PlayerList.Length <= 1)
        {
            if (PhotonNetwork.CurrentRoom != null)
                PhotonNetwork.CurrentRoom.CustomProperties.Clear();
        }

        //지금 나가려는 플레이어를 찾아서 그 플레이어의
        //모든 CustomProperites를 초기화 해주고 나가는 것이 좋다.
        //그렇지 않으면 나갔다 즉시 방에 다시 입장시 오류발생
        if (PhotonNetwork.LocalPlayer != null)
            PhotonNetwork.LocalPlayer.CustomProperties.Clear();
        //그래야 중개되던 것이 모두 초기화 될것이다.

        //현재 룸을 빠져나가며 생성한 모든 네트워크 객체를 삭제
        PhotonNetwork.LeaveRoom();
    }

    //룸에서 접속 종료됐을 때 호출되는 콜백함수
    public override void OnLeftRoom()   //PhotonNetwork.LeaveRoom() 성공했을 때
    {
        Time.timeScale = 1f;
        
        GlobalValue.g_MyGold += m_RewardGold;
        UpMyGoldCo();
       
        UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");
        //로비씬을 호출
    }

    void Chatting()
    {
        //체팅 구현 예제
        if (Input.GetKeyDown(KeyCode.Return)) //<-- 엔터치면 인풋 필드 활성화
        {
            bEnter = !bEnter;

            if (bEnter == true)
            {
                textChat.gameObject.SetActive(bEnter);
                textChat.ActivateInputField(); //<--- 커서를 인풋필드로 이동시켜 줌
            }
            else
            {
                textChat.gameObject.SetActive(bEnter);

                if (textChat.text != "")
                {
                    EnterChat();
                }
            }
        }//if (Input.GetKeyDown(KeyCode.Return)) 
    }

    void UpMyGoldCo()
    {
        if (GlobalValue.g_Unique_ID == "")
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("LoginScene");
            return;
        }

        var request = new UpdateUserDataRequest()
        {
            //KeysToRemove 특정키 값을 삭제하는 거 까지는 할 수 있다.
            //Public 공개 설정 : 다른 유저들이 볼 수도 있게 하는 옶션
            //Private 비공개 설정(기본 설정임) : 나만 접근할 수 있는값의 속성으로 변경
            //유저가 언제든 Permission 만 바꿀 수 도 있다.
            //Permission = UserDataPermission.Public
            //Permission = UserDataPermission.Private,
            Data = new Dictionary<string, string>()
            {
                {"MyGold", GlobalValue.g_MyGold.ToString() }
            }
        };

        PlayFabClientAPI.UpdateUserData(request,
            (result) =>
            {
                Debug.Log("데이터 저장 성공");

                UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");
            },
            (error) =>
            {
                Debug.Log("데이터 저장 실패");

                UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");

            });
    }   

    float a = 0;
    void GameEnd()
    {
        if (m_GameState != GameState.End)
            return;
        Cursor.lockState = CursorLockMode.None;

        DefaultCanvas.gameObject.SetActive(false);
        GameEndCanvas.gameObject.SetActive(true);

        UserNick.text = GlobalValue.g_NickName;
        if (!m_Win)
            MentTxt.text = "BATTLE LUCK NEXT TIME!";   
        else
            MentTxt.text = "WINNER WINNER!";

        if (a < 150)
            a += Time.deltaTime*50;
        else
        {
            a = 150;
            UserNick.gameObject.SetActive(true);
            MentTxt.gameObject.SetActive(true);
            RankTxt.gameObject.SetActive(true);
            TotalTxt.gameObject.SetActive(true);
            TimeTxt.gameObject.SetActive(true);
            LobbyBtn.gameObject.SetActive(true);
            EndTimer -= Time.deltaTime;
        }

        GameEndPanel.color = new Color32(0, 0, 0, (byte)a);

        TimeTxt.text = "Going out to the Lobby in " + (int)EndTimer + " seconds.";

        if (EndTimer < 1)
            OnClickExitRoom();
    }

    int CalcGold(int rank, int kill)
    {
        //rank 1=> 100, 20 =>5
        //kill당 +
        m_RewardGold = 0;
        m_RewardGold += (-1 * (rank - 21)) * 5;
        m_RewardGold += kill*10;
        if (m_Win)
            m_RewardGold += 300;    //최후의 1인 추가금

        return m_RewardGold;
    }

    public void RankPlayer(int KillCount = 0)
    {
        if (m_Rank <=1) //최후의 1인이 남으면
        {
            float a_CurHP = 0;
            int a_Kill = 0;
            PhotonView a_Ref_PV = null;
            Debug.Log("FinalRank");

            GameObject[] Players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject _player in Players)
            {
                a_Ref_PV = _player.GetComponent<PhotonView>();

                if (a_Ref_PV.Owner.CustomProperties.ContainsKey("curHp") == true)
                {
                    a_CurHP = (float)a_Ref_PV.Owner.CustomProperties["curHp"];   //모든 캐릭터... 매플레임 계속 동기화                     

                    if(a_CurHP > 0 && a_Ref_PV.IsMine) //남은 플레이어의 체력이 0이상(살아있다면) 이캐릭터를 우승처리
                    {
                        a_Kill = (int)a_Ref_PV.Owner.CustomProperties["KillCount"];
                        Debug.Log("Rank");
                        m_GameState = GameState.End;
                        m_Win = true;
                        RankTxt.text = "#" + m_Rank + "<size=70><color=#7E7C7C>/" + m_TotalPlayer + "</color></size>";
                        TotalTxt.text = "RANK <b>#" + m_Rank + "</b>   KILL <b>#" + a_Kill + "</b>   REWARD     <b>" + CalcGold(m_Rank, a_Kill) + "</b>";
                        return;
                    }
                }
            }
           
        }
        else
        {
            Debug.Log("Rank");

            m_GameState = GameState.End;
            RankTxt.text = "#" + m_Rank + "<size=70><color=#7E7C7C>/" + m_TotalPlayer + "</color></size>";
            TotalTxt.text = "RANK <b>#" + m_Rank + "</b>   KILL <b>#" + KillCount + "</b>   REWARD     <b>" + CalcGold(m_Rank, KillCount) + "</b>";
        }      

    }

    void SetConnectPlayerScore()
    {
        //모든 Tank 프리팹을 배열에 저장
        GameObject[] Players = GameObject.FindGameObjectsWithTag("Player");

        PhotonView a_Ref_PV = null;
        foreach (GameObject player in Players)
        {
            a_Ref_PV = player.GetComponent<PhotonView>();
            if (a_Ref_PV == null) //"BodyMesh"인 경우는 제외시켜야 한다.
                continue;

            //------------- 마스터 클라이언트가 동기화 하는 방법
            int currKillCount = 0;
            if (a_Ref_PV.Owner.CustomProperties.ContainsKey("KillCount") == true)
            {
                currKillCount = (int)a_Ref_PV.Owner.CustomProperties["KillCount"];
            }

            m_RefDamage.killCount = currKillCount;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(m_AKMSkinable);
            stream.SendNext(m_PistolSkinable);
            stream.SendNext(m_TotalPlayer);

            stream.SendNext(m_Rank);
        }
        else
        {
            a_AKMSkinable = (bool)stream.ReceiveNext();
            a_PistolSkinable = (bool)stream.ReceiveNext();
            m_TotalPlayer = (int)stream.ReceiveNext();

            m_Rank = (int)stream.ReceiveNext();
        }
    }
}
