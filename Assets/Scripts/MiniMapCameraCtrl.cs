using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapCameraCtrl : MonoBehaviour
{
    public PlayerCtrl m_Player;

    Transform Tr;

    private void Awake()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        Tr = this.GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LateUpdate()
    {
        if (m_Player == null)
            return;

        Quaternion rotate = Quaternion.Euler(90, 0, -m_Player.transform.localEulerAngles.y);

        if (GameManager.Inst.m_GameState == GameState.Start)
        {
            Tr.position = Vector3.Lerp(Tr.position, new Vector3(m_Player.transform.position.x, Tr.position.y, m_Player.transform.position.z), Time.deltaTime * 10);
            Tr.rotation = rotate;
        }
    }
}
