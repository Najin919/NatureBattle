using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCtrl : MonoBehaviour
{
    public GameObject m_Plyaer;

    PlayerCtrl m_RefPlayerCtrl;

    Transform tr;
    Vector3 m_StartVec;  //시작할때 Tr저장
    Vector3 m_ProneVec = new Vector3(-1.2f, -0.186f, 0);
    Quaternion m_StartCamangle = Quaternion.Euler(new Vector3(-96.35f, 90, 0));
    Quaternion m_Camangle = Quaternion.Euler(new Vector3(-175f, 90, 0));

    // Start is called before the first frame update
    void Start()
    {
        //m_Plyaer = //GameObject.Find("Team1Root(Clone)");
        m_RefPlayerCtrl = GameManager.Inst.m_RefPlayer;//m_Plyaer.GetComponent<PlayerCtrl>();
        tr = this.GetComponent<Transform>();
        m_StartVec = tr.localPosition;
        //m_StartCamangle = tr.localRotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Inst.m_GameState == GameState.Start)
        {
            if (m_RefPlayerCtrl == null)
                m_RefPlayerCtrl = GameManager.Inst.m_RefPlayer;

            Prone();
        }

        
    }

    private void LateUpdate()
    {
        GameEnd();
    }

    void Prone()    //엎드리기
    {
       if(m_RefPlayerCtrl.m_Prone == true && tr.localPosition != m_ProneVec && tr.localRotation != m_Camangle)
        {
            tr.localPosition = Vector3.Lerp(tr.localPosition, m_ProneVec, Time.deltaTime*10f);
            tr.localRotation = Quaternion.Slerp(tr.localRotation, m_Camangle, Time.deltaTime * 10f);
        }
       else if(m_RefPlayerCtrl.m_Prone == false && tr.localPosition != m_StartVec && tr.localRotation != m_StartCamangle)
        {
            tr.localPosition = Vector3.Lerp(tr.localPosition, m_StartVec , Time.deltaTime * 10f);
            tr.localRotation = Quaternion.Slerp(tr.localRotation, m_StartCamangle , Time.deltaTime*10f);
        }
    }

    Vector3 DieVec = Vector3.zero;
    void GameEnd()
    {
        if (GameManager.Inst.m_GameState != GameState.End)
            return;

        if (DieVec == Vector3.zero)
        {
            DieVec = tr.position + new Vector3(0, 2, 0);
        }

        if (m_Plyaer != null)
        {
            tr.LookAt(m_Plyaer.transform);
            tr.position =  Vector3.Lerp(tr.position, DieVec, Time.deltaTime);
        }
    }
}
