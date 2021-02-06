using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class RagdollCtrl : MonoBehaviourPunCallbacks
{
    public float DestroyTime = 10f;

    public GameObject[] RagDollWeapon;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (DestroyTime > 0)
            DestroyTime -= Time.deltaTime;
        else
        {
            DestroyTime = 0;
            SkinnedMeshRenderer[] skinnedMeshRenders = GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach(SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenders)
            {
                skinnedMeshRenderer.enabled = false;
            }
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.enabled = false;
            }
        }
    }

    public void CopyDefaultModelToRagdoll(Transform origin, Transform Rag,GameObject[] defaultWp)//기존의 모습에서 힘빠지게 연출
    {
        for (int i = 0; i < origin.transform.childCount; i++)
        {
            if (origin.transform.childCount != 0)
            {
                //CopyDefaultModelToRagdoll(origin.transform.GetChild(i), Rag.transform.GetChild(i));
            }
            Rag.transform.GetChild(i).localPosition = origin.transform.GetChild(i).localPosition;
            Rag.transform.GetChild(i).localRotation = origin.transform.GetChild(i).localRotation;
        }

        for (int i = 0; i < 2; i++)
        {
            RagDollWeapon[i].SetActive(defaultWp[i].activeSelf);
        }

    }
}
