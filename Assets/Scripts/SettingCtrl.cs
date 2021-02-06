using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingCtrl : MonoBehaviour
{
    public Button m_CloseBtn;
    public Button m_LobbyBtn;
    public Slider m_SoundSd;
    public Slider m_EffectSd;

    public Text txtLogMsg;

    private void Awake()
    {
        if (SceneManager.GetActiveScene().name == "InGameScene" || SceneManager.GetActiveScene().name == "TrainingScene")
        {
            m_LobbyBtn.gameObject.SetActive(true);  // 게임플레이 화면 에서만 로비로가는 버튼을 보여준다.
            if(SceneManager.GetActiveScene().name == "InGameScene")
                txtLogMsg = GameObject.Find("ChatTxt").GetComponent<Text>();
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        if (m_CloseBtn != null)
            m_CloseBtn.onClick.AddListener(() =>
            {
                EffectMgr.Instance.PlayEffect("UIclick");

                if (SceneManager.GetActiveScene().name == "InGameScene" || SceneManager.GetActiveScene().name == "TrainingScene")
                    Cursor.lockState = CursorLockMode.Locked;

                if (SceneManager.GetActiveScene().name == "InGameScene")
                    GameManager.Inst.m_GameState = GameState.Start;
                else if(SceneManager.GetActiveScene().name == "TrainingScene")
                    TrainingMgr.Inst.m_TrainingState = TrainingState.Play;

                PlayerAudioCtrl a_playerAudio = FindObjectOfType<PlayerAudioCtrl>();
                float effectV = PlayerPrefs.GetFloat("EffectVolume", 1.0f);

                if (a_playerAudio != null)
                {
                    a_playerAudio.WalkaudioSource.volume = effectV;
                    a_playerAudio.EffectaudioSource.volume = effectV;
                }

                Destroy(this.gameObject);
            });

        if (m_LobbyBtn != null)
            m_LobbyBtn.onClick.AddListener(() =>
            {
                EffectMgr.Instance.PlayEffect("UIclick");

                if (SceneManager.GetActiveScene().name == "InGameScene")
                    GameManager.Inst.OnClickExitRoom();
                else if (SceneManager.GetActiveScene().name == "TrainingScene")
                    TrainExitRoom();
            });

        if (m_SoundSd != null)
            m_SoundSd.onValueChanged.AddListener(SoundsdChange);

        if (m_EffectSd != null)
            m_EffectSd.onValueChanged.AddListener(EffectsdChange);

        float a_SoundV = PlayerPrefs.GetFloat("SoundVolume", 1.0f);
        if (m_SoundSd != null)
            m_SoundSd.value = a_SoundV;

        float a_EffectV = PlayerPrefs.GetFloat("EffectVolume", 1.0f);
        if (m_EffectSd != null)
            m_EffectSd.value = a_EffectV;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void TrainExitRoom()
    {
        Time.timeScale = 1.0f;

        SceneManager.LoadScene("LobbyScene");
    }

    public void SoundsdChange(float value)
    {
        SoundMgr.Instance.BGMVolume(value);
        PlayerPrefs.SetFloat("SoundVolume", value);
    }

    public void EffectsdChange(float value)
    {
        EffectMgr.Instance.EffectVolume(value);
        PlayerPrefs.SetFloat("EffectVolume", value);
    }
}
