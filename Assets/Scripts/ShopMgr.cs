using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;

public class ShopMgr : MonoBehaviour
{
    public Text m_MyGoldTxt;
    public Button LobbyBtn;
    public Button m_MyRoomBtn;
    public Button m_AKMBtn;
    public Button m_PistolBtn;
    public GameObject m_AKMScrollView;
    public GameObject m_PistolScrollView;

    [Header("MyRoom")]
    //MyRoom 변수
    public GameObject MyRoomPanel;
    public Button m_CloseBtn2;
    public Button m_MyAKMBtn;
    public Button m_MyPistolBtn;
    public GameObject m_MyAKMScrollView;
    public Transform m_MyAKMContent;
    public GameObject m_MyPistolScrollView;
    public Transform m_MyPistolContent;
    public Sprite BtnActiveImage;
    public Sprite BtnDeActiveImage;

    public GameObject m_MyAKMSkinPrefab;
    public GameObject m_MyPistolSkinPrefab;

    [Header("Audio")]
    [SerializeField] private string UIClick_Sound;
    //MyRoom 변수

    [Header("BuyPanel")]
    //BuyPanel 변수
    public GameObject BuyPanel;
    public Button m_CloseBtn1;
    [HideInInspector] public Skin_Type m_Skin_Type;
    public Image AKMSkinImage;
    public Image PistolSkinImage;
    public Text m_SkinName;
    public Text m_skinPrice;
    public Button m_BuyBtn;
    //BuyPanel 변수

    [Header("Setting")]
    //환경설정 변수
    public Button SettingBtn;
    public GameObject Canvas_Dialog = null;
    private GameObject m_ConfigBoxObj = null;
    //환경설정 변수

    [Header("Warning")]
    public GameObject WarningPanel = null;


    // Start is called before the first frame update
    void Start()
    {
        GlobalValue.ReflashMySkinLoad();
        ReflashSkinScrollview();
        m_MyGoldTxt.text = GlobalValue.g_MyGold.ToString();

        if (LobbyBtn != null)
            LobbyBtn.onClick.AddListener(() =>
            {
                UpMyGoldCo();
                UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");
            });

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

        if (m_CloseBtn1 != null)
            m_CloseBtn1.onClick.AddListener(() =>
            {
                BuyPanel.SetActive(false);
            });
        if (m_CloseBtn2 != null)
            m_CloseBtn2.onClick.AddListener(() =>
            {
                MyRoomPanel.SetActive(false);
            });
       
        if (m_MyRoomBtn != null)
            m_MyRoomBtn.onClick.AddListener(() =>
            {
                MyRoomPanel.SetActive(true);
            });

        if (m_BuyBtn != null)
            m_BuyBtn.onClick.AddListener(BuySkin);

        if (m_AKMBtn != null)
            m_AKMBtn.onClick.AddListener(AKMScrollAct);

        if (m_PistolBtn != null)
            m_PistolBtn.onClick.AddListener(PistolScrollAct);

        if (m_MyAKMBtn != null)
            m_MyAKMBtn.onClick.AddListener(MyAKMScrollAct);

        if (m_MyPistolBtn != null)
            m_MyPistolBtn.onClick.AddListener(MyPistolScrollAct);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayUIClickEffectSound()
    {
        EffectMgr.Instance.PlayEffect(UIClick_Sound);
    }

    void AKMScrollAct()
    {
        m_AKMBtn.GetComponent<Image>().sprite = BtnActiveImage;
        m_PistolBtn.GetComponent<Image>().sprite = BtnDeActiveImage;
        m_AKMScrollView.SetActive(true);
        m_PistolScrollView.SetActive(false);
    }
    void PistolScrollAct()
    {
        m_AKMBtn.GetComponent<Image>().sprite = BtnDeActiveImage;
        m_PistolBtn.GetComponent<Image>().sprite = BtnActiveImage;
        m_AKMScrollView.SetActive(false);
        m_PistolScrollView.SetActive(true);
    }

    void MyAKMScrollAct()
    {
        m_MyAKMBtn.GetComponent<Image>().sprite = BtnActiveImage;
        m_MyPistolBtn.GetComponent<Image>().sprite = BtnDeActiveImage;
        m_MyAKMScrollView.SetActive(true);
        m_MyPistolScrollView.SetActive(false);
    }
    void MyPistolScrollAct()
    {
        m_MyAKMBtn.GetComponent<Image>().sprite = BtnDeActiveImage;
        m_MyPistolBtn.GetComponent<Image>().sprite = BtnActiveImage;
        m_MyAKMScrollView.SetActive(false);
        m_MyPistolScrollView.SetActive(true);
    }

    void BuySkin()
    {
        string skinname = m_SkinName.text;
        int skinprice = int.Parse(m_skinPrice.text);

        if (GlobalValue.g_MyGold > 0 && GlobalValue.g_MyGold >= skinprice)
        {
            GlobalValue.g_MyGold -= skinprice;
            m_MyGoldTxt.text = GlobalValue.g_MyGold.ToString();

            if (m_Skin_Type == Skin_Type.SK_AKM)
                GlobalValue.AddMyAKMSkin(m_Skin_Type, skinname);
            else
                GlobalValue.AddMyPistolSkin(m_Skin_Type, skinname);
        }
        else
        {
            WarningPanel.SetActive(true);
        }

        BuyPanel.SetActive(false);
        
    }

    public void AddNodeAMKScrollView(SkinValue a_Node)
    {
        Debug.Log("AddNodeAMKScrollView");
        GameObject m_SkinNode = (GameObject)Instantiate(m_MyAKMSkinPrefab);

        m_SkinNode.transform.SetParent(m_MyAKMContent, false);
        //false일 경우 : 로컬 기준의 정보를 유지한 채 차일드화된다.
        MySkinData a_MySkin = m_SkinNode.GetComponent<MySkinData>();

        if (a_MySkin != null)
            a_MySkin.SetSkinRsc(a_Node);

        m_MyAKMContent.GetComponent<RectTransform>().pivot = new Vector2(0.0f, 1.0f); //스크롤뷰를 재정렬
    }

    public void AddNodePistolScrollView(SkinValue a_Node)
    {
        Debug.Log("AddNodePistolScrollView : " + GlobalValue.g_PistolSkinList.Count);
        GameObject m_SkinNode = (GameObject)Instantiate(m_MyPistolSkinPrefab);

        m_SkinNode.transform.SetParent(m_MyPistolContent, false);
        //false일 경우 : 로컬 기준의 정보를 유지한 채 차일드화된다.
        MySkinData a_MySkin = m_SkinNode.GetComponent<MySkinData>();

        if (a_MySkin != null)
            a_MySkin.SetSkinRsc(a_Node);

        m_MyPistolContent.GetComponent<RectTransform>().pivot = new Vector2(0.0f, 1.0f); //스크롤뷰를 재정렬
    }

    public void ReflashSkinScrollview()
    {
        MySkinData[] a_MyAKMSkinList = m_MyAKMContent.GetComponentsInChildren<MySkinData>(true);
        MySkinData[] a_MyPistolSkinList = m_MyPistolContent.GetComponentsInChildren<MySkinData>(true);

        for (int i = 0; i < a_MyAKMSkinList.Length; i++)
            Destroy(a_MyAKMSkinList[i].gameObject);

        for (int i = 0; i < GlobalValue.g_AKMSkinList.Count; i++)
            AddNodeAMKScrollView(GlobalValue.g_AKMSkinList[i]);

        for (int i = 0; i < a_MyPistolSkinList.Length; i++)
            Destroy(a_MyPistolSkinList[i].gameObject);

        for (int i = 0; i < GlobalValue.g_PistolSkinList.Count; i++)
            AddNodePistolScrollView(GlobalValue.g_PistolSkinList[i]);
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
            //Public 공개 설정 : 다른 유저들이 볼 수도 있게 하는 옵션
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
}
