using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name; //곡의 이름
    public AudioClip clip; //곡
}

public class SoundMgr : G_Singleton<SoundMgr>
{
    AudioClip m_audioClip;
    public AudioSource m_audioSouce = null;    //백그라운드 AudioSource 컴포넌트
    float m_bgmVolume = 1f;

    public Sound[] BGMSound;

    // Start is called before the first frame update
    void Start()
    {
        if (m_audioSouce == null)
        {
            m_audioSouce = gameObject.AddComponent<AudioSource>();
            m_audioSouce.playOnAwake = false;
            m_audioSouce.loop = true;
            m_bgmVolume = PlayerPrefs.GetFloat("SoundVolume", 1.0f);
        }
        PlayBGM(BGMSound[0].name);
    }


    // Update is called once per frame
    void Update()
    {

    }

    public void PlayBGM(string a_FileName, bool a_loop = true)
    {
        for (int i = 0; i < BGMSound.Length; i++)
        {
            if (a_FileName == BGMSound[i].name)
            {
                m_audioClip = Resources.Load("Sounds/" + a_FileName) as AudioClip;

                if (m_audioClip != null && m_audioSouce != null)
                {
                    m_audioSouce.clip = m_audioClip;
                    m_audioSouce.playOnAwake = false;
                    m_audioSouce.volume = m_bgmVolume;
                    m_audioSouce.loop = a_loop;
                    m_audioSouce.Play(0);
                    return;
                }
            }
        }              
    }

    public void SoundOnOff(bool a_OnOff = true)
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

    public void BGMVolume(float fVolume = 1f)
    {
        if (m_audioSouce != null)
            m_audioSouce.volume = fVolume;
        m_bgmVolume = fVolume;
    }

}
