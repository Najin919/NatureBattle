using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

enum SkinState
{
    Active,
    DeActive,
}

public class SkinData : MonoBehaviour
{
    ShopMgr m_ShopMgr;

    SkinState m_skinState = SkinState.Active;
    public Skin_Type Skin_Type;
    [HideInInspector] public string SkinName = "";
    [HideInInspector] public int SkinPrice = 0;

    public Text SkinNameTxt;
    public Text SkinPriceTxt;
    Sprite SkinSprite;

    // Start is called before the first frame update
    void Start()
    {
        m_ShopMgr = FindObjectOfType<ShopMgr>();        

        SkinName = SkinNameTxt.text;
        SkinPrice = int.Parse(SkinPriceTxt.text);
        SkinSprite = Resources.Load<Sprite>("WpSkin/" + SkinName);
    }

    // Update is called once per frame
    void Update()
    {
        this.GetComponent<Button>().onClick.AddListener(BuyPanelActive);

        if(this.gameObject.activeSelf)
            this.gameObject.SetActive(GlobalValue.SearchSkin(Skin_Type, SkinName));

    }

    void BuyPanelActive()
    {
        m_ShopMgr.m_Skin_Type = Skin_Type;

        if (Skin_Type == Skin_Type.SK_AKM)
        {
            m_ShopMgr.AKMSkinImage.sprite = SkinSprite;
            m_ShopMgr.AKMSkinImage.gameObject.SetActive(true);
            m_ShopMgr.PistolSkinImage.gameObject.SetActive(false);
        }
        else
        {
            m_ShopMgr.PistolSkinImage.sprite = SkinSprite;
            m_ShopMgr.AKMSkinImage.gameObject.SetActive(false);
            m_ShopMgr.PistolSkinImage.gameObject.SetActive(true);
        }

        m_ShopMgr.m_SkinName.text = SkinName;
        m_ShopMgr.m_skinPrice.text = SkinPrice.ToString();
        m_ShopMgr.BuyPanel.SetActive(true);        
    }
}
