using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapCameraCtrl : MonoBehaviour
{
    public PlayerCtrl m_Player;

    //ZoomInOut
    Vector3 DefaultPos;
    Vector3 ZoomPos;

    float ZoomSpeed = 10f;
    float MaxSize;
    float MinSize = 37;
    //ZoomInOut

    //Drag
    private Vector3 MouseStartPos;
    //Drag

    // Start is called before the first frame update
    void Start()
    {
        m_Player = FindObjectOfType<GameManager>().m_RefPlayer;
        //Point.transform.localPosition = Vector3.Lerp(Point.transform.localPosition, new Vector3(m_Player.transform.position.x, 15, m_Player.transform.position.z), Time.deltaTime * 10);
        DefaultPos = this.transform.position;

        MaxSize = this.GetComponent<Camera>().orthographicSize;
        MinSize = 37f;
        this.gameObject.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetAxis("Mouse ScrollWheel") != 0 && GameManager.Inst.MapActive)
            ZoomInOut(Input.GetAxis("Mouse ScrollWheel"));

        DragMap();
    }


    void ZoomInOut(float a_mouse)
    {
        Camera MapCam = this.GetComponent<Camera>();
        ZoomPos = MapCam.ScreenToWorldPoint(Input.mousePosition);
        ZoomPos.y = 100f;
        Debug.Log(ZoomPos);

        if (a_mouse < 0)        //ZoomOut
        {
            if(MapCam.orthographicSize < MaxSize)
                MapCam.orthographicSize += ZoomSpeed;

            transform.position = Vector3.Lerp(transform.position, DefaultPos, Time.deltaTime * 10);

        }
        else if (a_mouse > 0 && MapCam.orthographicSize > MinSize)   //ZoomIn
        {
            MapCam.orthographicSize -= ZoomSpeed;            
            transform.position = Vector3.Lerp(transform.position, ZoomPos, Time.deltaTime * 10);
        }

    }

    void DragMap()
    {
        Camera MapCam = this.GetComponent<Camera>();

        if (Input.GetMouseButtonDown(0))
        {
            MouseStartPos = MapCam.ScreenToWorldPoint(Input.mousePosition);
            MouseStartPos.y = transform.position.y;

        }
        else if (Input.GetMouseButton(0))
        {
            var MouseMove = MapCam.ScreenToWorldPoint(Input.mousePosition);
            MouseMove.y = transform.position.y;
            transform.position = transform.position - (MouseMove - MouseStartPos);
        }

        //드래그 제한
        if (transform.position.x > 90)
            transform.position = new Vector3(90f, transform.position.y, transform.position.z);
        else if (transform.position.x < -90)
            transform.position = new Vector3(-90f, transform.position.y, transform.position.z);
        else if (transform.position.z >115)
            transform.position = new Vector3(transform.position.x, transform.position.y,115f);
        else if (transform.position.z < -100)
            transform.position = new Vector3(transform.position.x, transform.position.y, -100f);
        //드래그 제한

    }
}
