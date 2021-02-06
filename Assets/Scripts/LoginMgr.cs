using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;

public class LoginMgr : MonoBehaviour
{
    [HideInInspector] public string g_Message = "";

    [Header("LoginPanel")]
    public GameObject m_LoginPanelObj;
    public InputField IDInputField;     
    public InputField PassInputField;
    public Button m_LoginBtn = null;
    public Button m_CreateAccOpenBtn = null;

    [Header("CreateAccountPanel")]
    public GameObject m_CreateAccountPanel;
    public InputField New_IDInputField;     
    public InputField New_PassInputField;
    public InputField New_PassInputField2;
    public InputField New_NickInputField;
    public Button m_CreateAccountBtn = null;
    public Button m_CancleBtn = null;

    [Header("Setting")]
    //환경설정 변수
    public Button SettingBtn;
    public GameObject Canvas_Dialog = null;
    private GameObject m_ConfigBoxObj = null;
    //환경설정 변수

    [Header("Audio")]
    [SerializeField] private string UIClick_Sound;


    // Start is called before the first frame update
    void Start()
    {
        if (m_LoginBtn != null)
            m_LoginBtn.onClick.AddListener(LoginBtn);

        if (m_CreateAccOpenBtn != null)
            m_CreateAccOpenBtn.onClick.AddListener(OpenCreateAccBtn);

        if (m_CreateAccountBtn != null)
            m_CreateAccountBtn.onClick.AddListener(CreateAccountBtn);

        if (m_CancleBtn != null)
            m_CancleBtn.onClick.AddListener(CreateCancelBtn);

        if (SettingBtn != null)
            SettingBtn.onClick.AddListener(() =>
            {
                if (m_ConfigBoxObj == null)
                    m_ConfigBoxObj = Resources.Load("Prefab/SettingPrefab") as GameObject;

                GameObject a_CfgBoxObj = (GameObject)Instantiate(m_ConfigBoxObj);
                a_CfgBoxObj.transform.SetParent(Canvas_Dialog.transform, false);
                //false로 해야 로컬 프리즘에 설정된 좌표를 유지한체 차일드로 붙게된다.
                //flase하지 않으면 부모 좌표로 자동설정
            });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayUIClickEffectSound()
    {
        EffectMgr.Instance.PlayEffect(UIClick_Sound);
    }


    public void LoginBtn()
    {
        string a_IdStr = IDInputField.text;
        string a_PwStr = PassInputField.text;

        if (a_IdStr.Trim() == "" || a_PwStr.Trim() == "")
        {
            g_Message = "ID, PW 빈칸 없이 입력해 주셔야 합니다.";
            return;
        }

        if (!(3 <= a_IdStr.Length && a_IdStr.Length < 20))
        {
            g_Message = "ID는 3글자 이상 20글자 이하로 작성해 주세요.";
            return;
        }

        if (!(6 <= a_PwStr.Length && a_PwStr.Length < 20))
        {
            g_Message = "비밀번호는 6글자 이상 20글자 이하로 작성해 주세요.";
            return;
        }

        //----- ID로만 로그인 하게 하는 코드
        var request = new LoginWithPlayFabRequest
        {
            Username = IDInputField.text,
            Password = PassInputField.text,
            //------- 이 옵션을 추가해 줘야 로그인하면서 유저의 각종 정보를 가져올 수 있다.
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams()
            {
                //- 이 옵션으로 DisplayName, AvatarUrl을 가져올 수 있다.
                GetPlayerProfile = true,
                ProfileConstraints = new PlayerProfileViewConstraints()
                {
                    ShowDisplayName = true, // 이 옵션으로 DisplayName,
                    //ShowAvatarUrl = true  // 이 옵션으로 AvatarUrl을 가져올 수 있다.
                },
                //- 이 옵션으로 DisplayName, AvatarUrl을 가져올 수 있다.

                //GetPlayerStatistics = true, //- 이 옵션으로 통계값(순위표에 관여하는)을 불러올 수 있다.
                GetUserData = true, //- 이 옵션으로 < 플레이어 데이터(타이틀) >값을 불러올 수 있다.
            }
            //------- 이 옵션을 추가해 줘야 로그인하면서 유저의 각종 정보를 가져올 수 있다.
        };

        PlayFabClientAPI.LoginWithPlayFab(request, OnLoginSuccess, OnLoginFailure);
        //----- ID로만 로그인 하게 하는 코드
    }

    private void OnLoginSuccess(LoginResult result)
    {
        g_Message = "로그인 성공";
        //Debu.Log(result.PlayFabId);   //로그인이 성공하면 바로 PlayFabId (고유ID : 마스터 플레이어)
        GlobalValue.g_Unique_ID = result.PlayFabId;

        if (result.InfoResultPayload != null)
        {
            //옵션에 설정에 의해 Displayname을 가져올 수 있다.
            //PlayerProfile는 Playfab 관리페이지 설정에 클라이언트 프로필 옵션에 "표시이름"에 체크가 되어있어야 한다.
            GlobalValue.g_NickName = result.InfoResultPayload.PlayerProfile.DisplayName;
            //옵션에 설정에 의해 Displayname을 가져올 수 있다.

            ////옵션에 설정에 의해 LoginWithEmailAddress()만으로도 유저 통계값(순위표에 관여하는)을 가져올 수 있다.
            //foreach (var eachStat in result.InfoResultPayload.PlayerStatistics)
            //{
            //    if (eachStat.StatisticName == "BestScore")
            //    {
            //        GlobalValue.g_BestScore = eachStat.Value;
            //    }
            //}
            ////옵션에 설정에 의해 LoginWithEmailAddress()만으로도 유저 통계값(순위표에 관여하는)을 가져올 수 있다.

            //<플레이어 데이터(타이틀)> 값 받아오기
            foreach (var eachData in result.InfoResultPayload.UserData)
            {
                if (eachData.Key == "MyGold")
                {
                    GlobalValue.g_MyGold = int.Parse(eachData.Value.Value);
                }
            }
            //<플레이어 데이터(타이틀)> 값 받아오기

        }
        UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");
    }

    private void OnLoginFailure(PlayFabError error)
    {
        g_Message = "로그인 실패 " + error.GenerateErrorReport();
    }

    public void OpenCreateAccBtn()
    {
        IDInputField.text = "";
        PassInputField.text = "";

        if (m_LoginPanelObj != null)
            m_LoginPanelObj.SetActive(false);
        if (m_CreateAccountPanel != null)
            m_CreateAccountPanel.SetActive(true);
    }

    public void CreateAccountBtn()
    {
        string a_IdStr = New_IDInputField.text;
        string a_PwStr = New_PassInputField.text;
        string a_PwStr2 = New_PassInputField2.text;
        string a_NickStr = New_NickInputField.text;

        if (a_IdStr.Trim() == "" || a_PwStr.Trim() == "" || a_NickStr.Trim() == "")
        {
            g_Message = "ID, PW 빈칸 없이 입력해 주셔야 합니다.";
            return;
        }

        if (!(3 <= a_IdStr.Length && a_IdStr.Length < 20))
        {
            g_Message = "ID는 3글자 이상 20글자 이하로 작성해 주세요.";
            return;
        }

        if (!(6 <= a_PwStr.Length && a_PwStr.Length < 20))
        {
            g_Message = "비밀번호는 6글자 이상 20글자 이하로 작성해 주세요.";
            return;
        }

        if(a_PwStr != a_PwStr2)
        {
            g_Message = "비밀번호와 확인 비밀번호가 다릅니다.";
            return;
        }

        //----- ID로만 계정생성 하게 하는 코드
        var request = new RegisterPlayFabUserRequest
        {
            Username = New_IDInputField.text,   //ID로만 계정 생성을 위해서...
            Password = New_PassInputField.text,
            DisplayName = New_NickInputField.text,
            RequireBothUsernameAndEmail = false
            //Username = "구글이나 페이스북 유니크ID"
        };
        //----- ID로만 계정생성 하게 하는 코드    
        PlayFabClientAPI.RegisterPlayFabUser(request, RegisterSuccess, RegisterFailure);
    }

    private void RegisterSuccess(RegisterPlayFabUserResult result)
    {
        g_Message = "가입 성공";

        if (m_LoginPanelObj != null)
            m_LoginPanelObj.SetActive(true);
        if (m_CreateAccountPanel != null)
            m_CreateAccountPanel.SetActive(false);
    }

    private void RegisterFailure(PlayFabError error)
    {
        Debug.LogWarning("가입 실패");
        g_Message = error.GenerateErrorReport();
    }

    public void CreateCancelBtn()
    {
        New_IDInputField.text = "";
        New_PassInputField.text = "";
        New_PassInputField2.text = "";
        New_NickInputField.text = "";

        if (m_LoginPanelObj != null)
            m_LoginPanelObj.SetActive(true);
        if (m_CreateAccountPanel != null)
            m_CreateAccountPanel.SetActive(false);
    }

    void OnGUI()
    {
        if (g_Message != "")
        {
            GUILayout.Label("<color=White><size=25>" + g_Message + "</size></color>");
        }
    }
}
