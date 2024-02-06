using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance = null;

    public AudioSource BGMAudio;

    public AudioClip[] bgmClip;

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider master_slider;
    [SerializeField] private Slider bgm_slider;
    

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }                
    }

    private void Start()
    {
        audioMixer.SetFloat("Master", Mathf.Log10(master_slider.value) * 20);
        audioMixer.SetFloat("BGM", Mathf.Log10(bgm_slider.value) * 20);
        BGM_Play(0);
    }

    // 매개변수로 받은 값으로 BGM 실행
    public void BGM_Play(int idx)
    {
        BGMAudio.clip = bgmClip[idx];
        BGMAudio.Play();
    }


    public void Master_VolumeSet()
    {
        audioMixer.SetFloat("Master", Mathf.Log10(master_slider.value) * 20);

    }

    public void BGM_VolumeSet()
    {
        audioMixer.SetFloat("BGM", Mathf.Log10(bgm_slider.value) * 20);

    }
    public void SFX_VolumeSet()
    {
       
    }


}
