using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum Skin_Type
{
    SK_AKM,
    SK_Pistol,
}

public class SkinValue
{
    public ulong UniqueID = 0;
    public Skin_Type m_skin_Type;
    public string m_skinName = "";

    public SkinValue() { }
}

public class SkinInfo : MonoBehaviour
{
    [HideInInspector] public SkinValue m_skinValue = new SkinValue();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
