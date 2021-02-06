using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using PlayFab;
using PlayFab.ClientModels;

public class RoomMgr : MonoBehaviourPunCallbacks
{
    //RPC 호출을 위한 PhotonView
    private PhotonView pv;

    //접속한 플레이어 수를 표시할  Text UI 항목 변수
    public Text PlayerNumTxt;
    public GameObject PlayerPrefab;
    public GameObject ScrollViewCont;

    //접속 로그를 표시할 Text UI 항목 변수'
    public Text txtLogMsg;
    public Button ExitRoomBtn;
    public InputField ChatInputfield;

    //게임시작 관련  변수
    bool IsReady = false;
    public Button ReadymBtn;
    float m_GoWaitGame = 4;
    public Text m_WaitTimeTxt = null;
    //게임시작 관련  변수

    bool bEnter = false;

    //환경설정 변수
    public Button SettingBtn;
    public GameObject Canvas_Dialog = null;
    private GameObject m_ConfigBoxObj = null;
    //환경설정 변수

    [Header("Audio")]
    [SerializeField] private string UIClick_Sound;

    ExitGames.Client.Photon.Hashtable a_PlayerReady = new ExitGames.Client.Photon.Hashtable();


    private void OnApplicationFocus(bool focus) //윝도우 창 활성화 비활성화 일때
    {
        PhotonInit.isFocus = focus;
    }

    private void Awake()
    {
        //PhotonView 컴포넌트 할당
        pv = GetComponent<PhotonView>();

        //모든 클라우드의 네트워크 메시지 수신을 다시 연결
        PhotonNetwork.IsMessageQueueRunning = true;

        //룸 입장 후 기존 접속자 정보를 출력
        GetConnectPlayerCount();
    }

    // Start is called before the first frame update
    IEnumerator Start()
    {
        if (ExitRoomBtn != null)
            ExitRoomBtn.onClick.AddListener(OnClickExitRoom);

        //로그 메시지에 출력할 문자열 생성
        string msg = "\n<color=#00ff00>[" + PhotonNetwork.LocalPlayer.NickName + "] entered the room. </color>";
        //RPC 함수 호출
        pv.RPC("LogMsg", RpcTarget.AllBuffered, msg);


        a_PlayerReady.Clear();
        a_PlayerReady.Add("IamReady", 0);
        PhotonNetwork.LocalPlayer.SetCustomProperties(a_PlayerReady);  //Player 별로 동기화 시키고 싶은 경우

        //내가 입장할때 나를 포함한 다른 사람들에게 내 등장을 알린다. 
        pv.RPC("RefreshPlayer", RpcTarget.AllViaServer); //내가 방에 입장할 때도 한번 보냅니다.

        if (ReadymBtn != null)
            ReadymBtn.onClick.AddListener(ClickReadyBtn);

        if (SettingBtn != null)
            SettingBtn.onClick.AddListener(() =>
            {
                if (m_ConfigBoxObj == null)
                    m_ConfigBoxObj = Resources.Load("Prefab/SettingPrefab") as GameObject;

                GameObject a_CfgBoxObj = (GameObject)Instantiate(m_ConfigBoxObj);
                a_CfgBoxObj.transform.SetParent(Canvas_Dialog.transform, false);
            });

        //룸에 있는 네트워크 객체 간의 통신이 완료될 때까지 잠시 대기
        yield return new WaitForSeconds(1.0f);
    }

    // Update is called once per frame
    void Update()
    {
        Chatting();
        GameStart();
    }

    public void PlayUIClickEffectSound()
    {
        EffectMgr.Instance.PlayEffect(UIClick_Sound);
    }

    void GameStart()
    {
        if (0.0f < m_GoWaitGame)
        {
            bool a_AllReady = true;
            foreach (Player _player in PhotonNetwork.PlayerList)
            {
                Debug.Log((int)_player.CustomProperties["IamReady"]);

                if (_player.CustomProperties.ContainsKey("IamReady") == true)
                {
                    if ((int)_player.CustomProperties["IamReady"] <= 0)
                    {
                        a_AllReady = false;
                        break;
                    }
                }
                else
                {
                    a_AllReady = false;
                    break;
                }
            }

            if(PhotonNetwork.CurrentRoom != null)
            if (a_AllReady == true)
                {
                SoundMgr.Instance.m_audioSouce.Stop();  //음악 멈추기
                SoundMgr.Instance.m_audioSouce.clip = null;

                if (m_GoWaitGame >=4)
                EffectMgr.Instance.PlayEffect("CountDown");

                ReadymBtn.gameObject.SetActive(false);  //레디버튼 없애기

                //누가 발생시켰든 동기화 시키려고 하면....
                if (PhotonNetwork.CurrentRoom.IsOpen == true)
                    PhotonNetwork.CurrentRoom.IsOpen = false;

                //게임이 시작되었음을 동기화 시킴

                if (0.0f < m_GoWaitGame)
                {
                    m_GoWaitGame = m_GoWaitGame - Time.deltaTime;

                    if (m_WaitTimeTxt != null)
                    {
                        m_WaitTimeTxt.gameObject.SetActive(true);
                        m_WaitTimeTxt.text = ((int)m_GoWaitGame).ToString();
                    }

                    if (m_GoWaitGame <= 0.0f)
                        UnityEngine.SceneManagement.SceneManager.LoadScene("InGameScene");
                    
                }//if (0.0f < m_GoWaitGame)
            }//if (a_AllReady == true)

        }//if (m_GameState == GameState.GS_Ready && 0.0f < m_GoWaitGame)
    }

   void ClickReadyBtn()
    {
        if (!IsReady)
        {
            a_PlayerReady.Clear();
            a_PlayerReady.Add("IamReady", 1);
            PhotonNetwork.LocalPlayer.SetCustomProperties(a_PlayerReady);
            pv.RPC("RefreshPlayer", RpcTarget.AllViaServer);
        }
        else
        {
            a_PlayerReady.Clear();
            a_PlayerReady.Add("IamReady", 0);
            PhotonNetwork.LocalPlayer.SetCustomProperties(a_PlayerReady);
            pv.RPC("RefreshPlayer", RpcTarget.AllViaServer);
        }

        IsReady = !IsReady;

    }

    [PunRPC]
    void RefreshPlayer()
    {
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Player_Prefab"))
        {
            Destroy(obj);
        }

        Player[] players = PhotonNetwork.PlayerList; //using Photon.Realtime;

        foreach (Player _player in players)
        {
            GameObject Player = (GameObject)Instantiate(PlayerPrefab);
            Player.transform.SetParent(ScrollViewCont.transform, false);

            string msg = "";
            if (_player.CustomProperties.ContainsKey("IamReady") == true)
            {
                if ((int)_player.CustomProperties["IamReady"] == 1)
                {
                    msg = "<color=#F82B2B>Ready</color>";
                }
            }
            else
            {
                if(_player.CustomProperties["IamReady"] != null)
                if ((int)_player.CustomProperties["IamReady"] == 1)
                {
                    msg = "";
                }
            }

            PlayerData playerData = Player.GetComponent<PlayerData>();
            playerData.PlayerNick = _player.NickName;
            playerData.ReadyState = msg;


            playerData.DisPlayerData();
        }
    }

    void Chatting()
    {
        if (Input.GetKeyDown(KeyCode.Return))    // 엔터치면 인풋필드 활성화
        {
            bEnter = !bEnter;

            if (bEnter == true)
            {
                ChatInputfield.gameObject.SetActive(bEnter);
                ChatInputfield.ActivateInputField();  //커서를 인풋필드로 이동시켜줌
            }
            else
            {
                ChatInputfield.gameObject.SetActive(bEnter);

                if (ChatInputfield.text != "")
                {
                    EnterChat();
                }
            }
        }//if(Input.GetKeyDown(KeyCode.Return))
    }

    void EnterChat()
    {
        string msg = "";
        
        msg = "\n<color=#ffffff>[" + GlobalValue.g_NickName + "] : " + ChatInputfield.text + "</color>";

        pv.RPC("LogMsg", RpcTarget.AllBuffered, msg);

        ChatInputfield.text = "";
    }

    //네트워크 플레이어가 접속했을 때 호출되는 함수
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        GetConnectPlayerCount();
        RefreshPlayer();
        //pv.RPC("RefreshPlayer", RpcTarget.AllBuffered);
    }

    //네트워크 플레이어가 룸을 나가거나 접속이 끊어졌을 때 호출되는 함수
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        GetConnectPlayerCount();
        RefreshPlayer();
        //pv.RPC("RefreshPlayer", RpcTarget.AllBuffered);
    }

    //룸 접속자 정보를 조회하는 함수
    void GetConnectPlayerCount()
    {
        //현재 입장한 룸 정보를 받아옴
        Room currRoom = PhotonNetwork.CurrentRoom;  //using Photon.Realtime;

        //현재 룸의 접속자 수와 최대 접속 가능한 수를 문자열로 구성한 후 Text UI 항목에 출력
        PlayerNumTxt.text = currRoom.PlayerCount.ToString() + "/" + currRoom.MaxPlayers.ToString();
    }

    //룸 나가기 버튼 클릭 이벤트에 연결될 함수
    public void OnClickExitRoom()
    {
        //로그 메시지에 출력할 문자열 생성
        string msg = "\n<color=#ff0000>[" + PhotonNetwork.LocalPlayer.NickName + "] has left the room.</color>";

        //RPC 함수 호출
        pv.RPC("LogMsg", RpcTarget.AllBuffered, msg);
        //설정이 완료된 후 빌드 파일을 여러개 실행해
        //동일한 룸에 입장해보면 접속 로그가 표기되는 것을 확인할 수 있다.
        //또한 PhotonTarget.AllBuffered 옵션으로
        //RPC를 호출했기 때문에 나중에 입장해도 기존의 접속 로그 메시지가 표시된다.

        if (PhotonNetwork.PlayerList != null && PhotonNetwork.PlayerList.Length <= 1)
        {
            if (PhotonNetwork.CurrentRoom != null)
                PhotonNetwork.CurrentRoom.CustomProperties.Clear();
        }
              
        //모든 CustomProperties를 초기화 해 주고 나가는 것이 좋다. 
        //(그렇지 않으며 나갔다 즉시 방 입장시 오류 발생한다.)
        if (PhotonNetwork.LocalPlayer != null)
            PhotonNetwork.LocalPlayer.CustomProperties.Clear();
        //그래야 중개되던 것이 모두 초기화 될 것이다.

        //현재 룸을 빠져나가며 생성한 모든 네트워크 객체를 삭제
        PhotonNetwork.LeaveRoom();
    }

    //룸에서 접속 종료됐을 때 호출되는 콜백함수
    public override void OnLeftRoom()   //PhotonNetwork.LeaveRoom(); 성공했을때
    {
        //PhotonNetwork.LocalPlayer.CustomProperties.Clear();
        Time.timeScale = 1.0f;

        //로비 씬을 호출
        UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");
    }

    [PunRPC]
    void LogMsg(string msg)
    {
        //로그 메시지 Text UI에 텍스트를 누적시켜 표시
        txtLogMsg.text = txtLogMsg.text + msg;
    }
}
