using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class MySkinData : MonoBehaviour
{
    [HideInInspector] public ulong m_UniqueID = 0;
    public static string m_MyAKMSkinName = "";
    public static string m_MyPistolSkinName = "";
    public Skin_Type m_Skin_Type = Skin_Type.SK_AKM;
    public Text SkinNameTxt;
    public Image SkinImage;


    // Start is called before the first frame update
    void Start()
    {
        if(PlayerPrefs.HasKey("MyAKMSkinName"))
            m_MyAKMSkinName = PlayerPrefs.GetString("MyAKMSkinName", "");

        if(PlayerPrefs.HasKey("MyPistolSkinName"))
            m_MyPistolSkinName = PlayerPrefs.GetString("MyPistolSkinName", "");

        if (m_Skin_Type == Skin_Type.SK_AKM && m_MyAKMSkinName == SkinNameTxt.text)
            GetComponent<Outline>().enabled = true;

        if (m_Skin_Type == Skin_Type.SK_Pistol && m_MyPistolSkinName == SkinNameTxt.text)
            GetComponent<Outline>().enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<Button>().onClick.AddListener(CheckActiveSkin);
    }

    void CheckActiveSkin()
    {
        if (GetComponent<Outline>().enabled == true)
        {
            GetComponent<Outline>().enabled = false;
            if (m_Skin_Type == Skin_Type.SK_AKM)
            {
                m_MyAKMSkinName = "";
                PlayerPrefs.DeleteKey("MyAKMSkinName");
            }
            else
            {
                m_MyPistolSkinName = "";
                PlayerPrefs.DeleteKey("MyPistolSkinName");
            }

        }
        else
        {
            MySkinData[] myskins = FindObjectsOfType<MySkinData>();
            foreach (MySkinData myskin in myskins)
            {
                myskin.gameObject.GetComponent<Outline>().enabled = false;
            }

            GetComponent<Outline>().enabled = true;

            if (m_Skin_Type == Skin_Type.SK_AKM)
            {
                m_MyAKMSkinName = SkinNameTxt.text;
                PlayerPrefs.SetString("MyAKMSkinName", m_MyAKMSkinName);

            }
            else
            {
                m_MyPistolSkinName = SkinNameTxt.text;
                PlayerPrefs.SetString("MyPistolSkinName", m_MyPistolSkinName);
            }
        }
    }

    public void SetSkinRsc(SkinValue a_Node)
    {
        Debug.Log("SetSkinRsc");
        if (a_Node == null)
            return;

        m_Skin_Type = a_Node.m_skin_Type;

        if (SkinNameTxt != null)
            SkinNameTxt.text = a_Node.m_skinName;

        if (SkinImage != null)
            SkinImage.sprite = Resources.Load<Sprite>("WpSkin/" + a_Node.m_skinName);

    }

}
