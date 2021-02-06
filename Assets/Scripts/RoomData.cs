using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomData : MonoBehaviour
{
    [HideInInspector]
    public string roomName = "";
    [HideInInspector]
    public string roomCode = "";
    [HideInInspector]
    public int connectPlayer = 0;
    [HideInInspector]
    public int maxPlayer = 0;

    //룸 이름 표실할 Text UI 항목
    public Text textRoomName;
    //룸 접속자 수와 최대 접속자 수를 표시할 Text UI 항목
    public Text textConnectInfo;

    [HideInInspector] public string ReadyState = "";  //레디 상태 표시

    [Header("Audio")]
    [SerializeField] private string UIClick_Sound;

    //// Start is called before the first frame update
    //void Start()
    //{

    //}

    //// Update is called once per frame
    //void Update()
    //{

    //}

    //룸 정보를 전달한 후 Text UI 항목에 표시하는 함수

    public void PlayUIClickEffectSound()
    {
        EffectMgr.Instance.PlayEffect(UIClick_Sound);
    }
    public void DispRoomData(bool a_IsOpen)
    {
        if (a_IsOpen == true)
        {
            textRoomName.color = new Color32(0, 0, 0, 255);
            textConnectInfo.color = new Color32(0, 0, 0, 255);
        }
        else
        {
            textRoomName.color = new Color32(255, 0, 0, 255);
            textConnectInfo.color = new Color32(255, 0, 0, 255);
        }

        textRoomName.text = roomName;
        textConnectInfo.text = "(" + connectPlayer.ToString() + "/" + maxPlayer.ToString() + ")";
    }

    public void DispPlayerData()
    {
        textRoomName.text = roomName;
        textConnectInfo.text = ReadyState;
    }
}
