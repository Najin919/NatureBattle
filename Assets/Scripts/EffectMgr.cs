using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Effect
{
    public string name;
    public AudioClip clip;
}

public class EffectMgr : G_Singleton<EffectMgr>
{
    AudioClip m_audioClip;
    AudioSource m_audioSouce = null;    //백그라운드 AudioSource 컴포넌트
    float m_effectVolume = 1f;

    public Effect[] EffectSound;

    // Start is called before the first frame update
    void Start()
    {
        if (m_audioSouce == null)
        {
            m_audioSouce = gameObject.AddComponent<AudioSource>();
            m_audioSouce.playOnAwake = false;
            m_effectVolume = PlayerPrefs.GetFloat("EffectVolume", 1.0f);

        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PlayEffect(string a_FileName)
    {
        for (int i = 0; i < EffectSound.Length; i++)
        {
            if (a_FileName == EffectSound[i].name)
            {
                m_audioClip = Resources.Load("Effects/" + a_FileName) as AudioClip;

                if (m_audioClip != null && m_audioSouce != null)
                {
                    m_audioSouce.clip = m_audioClip;
                    m_audioSouce.volume = m_effectVolume;
                    m_audioSouce.Play(0);
                }
            }

        }
    }

    public void EffectOnOff(bool a_OnOff = true)
    {
        bool a_MuteOnOff = !a_OnOff;

        if (m_audioSouce != null)
        {
            m_audioSouce.mute = a_MuteOnOff;    //mute == true 끄기 켜기

            if (a_MuteOnOff == false)
            {
                m_audioSouce.time = 0;  //처음부터 다시 플레이
            }
        }
    }

    public void EffectVolume(float fVolume = 1f)
    {
        if (m_audioSouce != null)
            m_audioSouce.volume = fVolume;
        m_effectVolume = fVolume;
    }

}
