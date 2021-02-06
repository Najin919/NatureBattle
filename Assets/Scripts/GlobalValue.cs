using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalValue
{
    public static ShopMgr m_ShopMgr = null;

    public static string g_Unique_ID = "";
    public static string g_NickName = "";
    public static int g_MyGold = 0;

    public static ulong UniqueCount = 0;
    public static List<SkinValue> g_AKMSkinList = new List<SkinValue>();
    public static List<SkinValue> g_PistolSkinList = new List<SkinValue>();

    public static void ReflashMySkinLoad()  //<---- SkinList  갱신
    {
        g_AKMSkinList.Clear();
        g_PistolSkinList.Clear();

        SkinValue a_AKMNode;
        string a_KeyBuff = "";
        int a_AKMCount = PlayerPrefs.GetInt("AKM_Count", 0);
        

        for (int a_ii = 0; a_ii < a_AKMCount; a_ii++)
        {
            a_AKMNode = new SkinValue();
            a_KeyBuff = string.Format("AKM_{0}_UniqueID", a_ii);
            string stUniqueID = PlayerPrefs.GetString(a_KeyBuff, "");
            if (stUniqueID != "")
                a_AKMNode.UniqueID = ulong.Parse(stUniqueID);
            a_KeyBuff = string.Format("AKM_{0}_skin_Type", a_ii);
            a_AKMNode.m_skin_Type = (Skin_Type)PlayerPrefs.GetInt(a_KeyBuff, 0);
            a_KeyBuff = string.Format("AKM_{0}_skinName", a_ii);
            a_AKMNode.m_skinName = PlayerPrefs.GetString(a_KeyBuff, "");

            g_AKMSkinList.Add(a_AKMNode);
        }

        SkinValue a_PistolNode;
        int a_PistolCount = PlayerPrefs.GetInt("Pistol_Count", 0);
        for (int a_ii = 0; a_ii < a_PistolCount; a_ii++)
        {
            a_PistolNode = new SkinValue();
            a_KeyBuff = string.Format("Pistol_{0}_UniqueID", a_ii);
            string stUniqueID = PlayerPrefs.GetString(a_KeyBuff, "");
            if (stUniqueID != "")
                a_PistolNode.UniqueID = ulong.Parse(stUniqueID);
            a_KeyBuff = string.Format("Pistol_{0}_skin_Type", a_ii);
            a_PistolNode.m_skin_Type = (Skin_Type)PlayerPrefs.GetInt(a_KeyBuff, 0);
            a_KeyBuff = string.Format("Pistol_{0}_skinName", a_ii);
            a_PistolNode.m_skinName = PlayerPrefs.GetString(a_KeyBuff, "");

            g_PistolSkinList.Add(a_PistolNode);
        }
    }

    public static ulong GetAKMUnique() //임시 고유키 발급기...
    {
        UniqueCount = (ulong)PlayerPrefs.GetInt("AKMUnique", 0);
        UniqueCount++;
        ulong a_Index = UniqueCount;

        //<--자신의 인벤토리에 있는 아이템 번호랑 겹치는 번호보다는 큰 수로 유니크ID가 
        //발급되게 처리하는 부분
        if (0 < g_AKMSkinList.Count)
            for (int a_bb = 0; a_bb < g_AKMSkinList.Count; ++a_bb)
            {
                if (g_AKMSkinList[a_bb] == null)
                    continue;

                if (a_Index < g_AKMSkinList[a_bb].UniqueID)
                {
                    a_Index = g_AKMSkinList[a_bb].UniqueID + 1;
                }
            }//for (int a_bb = 0; a_bb < g_SkinList.Count; ++a_bb)

        UniqueCount = a_Index;
        PlayerPrefs.SetInt("AKMUnique", (int)UniqueCount);
        return a_Index;
    }//public ulong GetUnique()

    public static bool SearchSkin(Skin_Type a_Type, string a_Name)
    {
        List<string> AKMSkinNames = new List<string>();
        List<string> PistolSkinNames = new List<string>();
        string a_KeyBuff = "";

        if (a_Type == Skin_Type.SK_AKM)
            for (int a_ii = 0; a_ii < g_AKMSkinList.Count; a_ii++)
            {
                a_KeyBuff = string.Format("AKM_{0}_skinName", a_ii);
                AKMSkinNames.Add(PlayerPrefs.GetString(a_KeyBuff));

                if (a_Name == AKMSkinNames[a_ii])
                    return false;
            }
        else
            for (int a_ii = 0; a_ii < g_PistolSkinList.Count; a_ii++)
            {
                a_KeyBuff = string.Format("Pistol_{0}_skinName", a_ii);
                PistolSkinNames.Add(PlayerPrefs.GetString(a_KeyBuff));

                if (a_Name == PistolSkinNames[a_ii])
                    return false;
            }

        return true;
    }

    public static void AddMyAKMSkin(Skin_Type a_Type, string a_Name)
    {
        SkinValue a_Node = new SkinValue();
        a_Node.UniqueID = GetAKMUnique();
        a_Node.m_skin_Type = a_Type;
        a_Node.m_skinName = a_Name;

        g_AKMSkinList.Add(a_Node);

        m_ShopMgr = null;
        GameObject a_ShopMgr = GameObject.Find("ShopMgr");
        if (a_ShopMgr != null)
            m_ShopMgr = a_ShopMgr.GetComponent<ShopMgr>();

        if (m_ShopMgr != null)
            m_ShopMgr.AddNodeAMKScrollView(a_Node);

        ReflashAKMSkinSave();
    }

    public static void AddMyPistolSkin(Skin_Type a_Type, string a_Name)
    {
        SkinValue a_Node = new SkinValue();
        a_Node.UniqueID = GetAKMUnique();
        a_Node.m_skin_Type = a_Type;
        a_Node.m_skinName = a_Name;

        g_PistolSkinList.Add(a_Node);

        m_ShopMgr = null;
        GameObject a_ShopMgr = GameObject.Find("ShopMgr");
        if (a_ShopMgr != null)
            m_ShopMgr = a_ShopMgr.GetComponent<ShopMgr>();

        if (m_ShopMgr != null)
            m_ShopMgr.AddNodePistolScrollView(a_Node);

        ReflashPistolSkinSave();

    }

    public static void ReflashAKMSkinSave()  //<-- 리스트 다시 저장
    {
        //---------기존에 저장되어 있었던 아이템 목록 제거
        SkinValue a_SvNode;
        string a_KeyBuff = "";
        int a_AKMCount = PlayerPrefs.GetInt("AKM_Count", 0);
        for (int a_ii = 0; a_ii < a_AKMCount + 10; a_ii++)
        {
            a_KeyBuff = string.Format("AKM_{0}_UniqueID", a_ii);
            PlayerPrefs.DeleteKey(a_KeyBuff);
            a_KeyBuff = string.Format("AKM_{0}_skin_Type", a_ii);
            PlayerPrefs.DeleteKey(a_KeyBuff);
            a_KeyBuff = string.Format("AKM_{0}_skinName", a_ii);
            PlayerPrefs.DeleteKey(a_KeyBuff);
        }
        PlayerPrefs.DeleteKey("AKM_Count");
        PlayerPrefs.Save(); //폰에서 마지막 저장상태를 확실히 저장하게 하기 위하여...
        //---------기존에 저장되어 있었던 아이템 목록 제거

        //---------- 새로운 리스트 저장
        PlayerPrefs.SetInt("AKM_Count", g_AKMSkinList.Count);
        for (int a_ii = 0; a_ii < g_AKMSkinList.Count; a_ii++)
        {
            a_SvNode = g_AKMSkinList[a_ii];
            a_KeyBuff = string.Format("AKM_{0}_UniqueID", a_ii);
            PlayerPrefs.SetString(a_KeyBuff, a_SvNode.UniqueID.ToString());
            a_KeyBuff = string.Format("AKM_{0}_skin_Type", a_ii);
            PlayerPrefs.SetInt(a_KeyBuff, (int)a_SvNode.m_skin_Type);
            a_KeyBuff = string.Format("AKM_{0}_skinName", a_ii);
            PlayerPrefs.SetString(a_KeyBuff, a_SvNode.m_skinName);
        }
        PlayerPrefs.Save(); //폰에서 마지막 저장상태를 확실히 저장하게 하기 위하여...
        //---------- 새로운 리스트 저장
    }
    //------------ Item Reflash

    public static void ReflashPistolSkinSave()
    {
        SkinValue a_SvNode;
        string a_KeyBuff = "";
        int a_PistolCount = PlayerPrefs.GetInt("Pistol_Count", 0);
        for (int a_ii = 0; a_ii < a_PistolCount + 10; a_ii++)
        {
            a_KeyBuff = string.Format("Pistol_{0}_UniqueID", a_ii);
            PlayerPrefs.DeleteKey(a_KeyBuff);
            a_KeyBuff = string.Format("Pistol_{0}_skin_Type", a_ii);
            PlayerPrefs.DeleteKey(a_KeyBuff);
            a_KeyBuff = string.Format("Pistol_{0}_skinName", a_ii);
            PlayerPrefs.DeleteKey(a_KeyBuff);
        }
        PlayerPrefs.DeleteKey("Pistol_Count");  //아이템 수 제거
        PlayerPrefs.Save();

        PlayerPrefs.SetInt("Pistol_Count", g_PistolSkinList.Count);
        for (int a_ii = 0; a_ii < g_PistolSkinList.Count; a_ii++)
        {
            a_SvNode = g_PistolSkinList[a_ii];
            a_KeyBuff = string.Format("Pistol_{0}_UniqueID", a_ii);
            PlayerPrefs.SetString(a_KeyBuff, a_SvNode.UniqueID.ToString());
            a_KeyBuff = string.Format("Pistol_{0}_skin_Type", a_ii);
            PlayerPrefs.SetInt(a_KeyBuff, (int)a_SvNode.m_skin_Type);
            a_KeyBuff = string.Format("Pistol_{0}_skinName", a_ii);
            PlayerPrefs.SetString(a_KeyBuff, a_SvNode.m_skinName);
        }
        PlayerPrefs.Save();
    }

}


