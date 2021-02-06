using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerAudioCtrl : MonoBehaviour
{
    PhotonView pv;

    [Header("AudioSource")]
    public AudioSource WalkaudioSource;
    public AudioSource EffectaudioSource;

    [Header("AudioClip")]
    public AudioClip WalkClip;
    public AudioClip ProneWalkClip;
    public AudioClip Sit_upClip;

    public AudioClip GrenadeClip;
    public AudioClip AKMReloadClip;
    public AudioClip PistolReloadClip;
    public AudioClip AimClip;

    private void Awake()
    {
        pv = this.GetComponentInParent<PhotonView>();
    }

    // Start is called before the first frame update
    void Start()
    {
        float a_EffectV = PlayerPrefs.GetFloat("EffectVolume", 1.0f);
        if (WalkaudioSource != null)
            WalkaudioSource.volume = a_EffectV;

        if (EffectaudioSource != null)
            EffectaudioSource.volume = a_EffectV;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void WalkSound()
    {
        //if (pv.IsMine)
            WalkaudioSource.PlayOneShot(WalkClip);
    }

    public void ProneWalkSound()
    {
        //if (pv.IsMine)
            WalkaudioSource.PlayOneShot(ProneWalkClip);
    }

    public void ThorwGrenade()
    {
        //if (pv.IsMine)
            EffectaudioSource.PlayOneShot(GrenadeClip);
    }

    public void AKMReload()
    {
        //if (pv.IsMine)
            EffectaudioSource.PlayOneShot(AKMReloadClip);
    }

    public void PistolReload()
    {
        //if (pv.IsMine)
            EffectaudioSource.PlayOneShot(PistolReloadClip);
    }

    public void Aim()
    {
        //if (pv.IsMine)
            EffectaudioSource.PlayOneShot(AimClip);
    }

    public void Sit_Prone_StandUp()
    {
        //if (pv.IsMine)
            WalkaudioSource.PlayOneShot(Sit_upClip);

    }

}
