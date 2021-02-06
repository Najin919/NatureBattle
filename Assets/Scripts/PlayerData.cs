using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerData : MonoBehaviour
{
    [HideInInspector]
    public string PlayerNick = "";
    [HideInInspector] public string ReadyState = ""; //레디 상태 표시

    public Text PlayerNickTxt;
    public Text ReadyTxt;

    // Start is called before the first frame update
    void Start()
    { 
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void DisPlayerData()
    {
        PlayerNickTxt.text = PlayerNick;
        ReadyTxt.text = ReadyState;
    }
}
