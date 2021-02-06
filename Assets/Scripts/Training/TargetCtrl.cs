using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetCtrl : MonoBehaviour
{
    Animator Anim;
    float m_Delay = 2f;

    // Start is called before the first frame update
    void Start()
    {
        Anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Anim.GetCurrentAnimatorStateInfo(0).IsName("TargetDown"))
        {
            if (m_Delay > 0)
                m_Delay -= Time.deltaTime;
            else
            {
                Anim.SetBool("Hit", false);
                m_Delay = 2f;
            }
        }
    }

    public void Hit()
    {
        Anim.SetBool("Hit", true);

    }
}
