using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WarningCtrl : MonoBehaviour
{
    public Button m_CloseBtn;

    // Start is called before the first frame update
    void Start()
    {
        if (m_CloseBtn != null)
            m_CloseBtn.onClick.AddListener(() =>
            {
                this.gameObject.SetActive(false);
            });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
