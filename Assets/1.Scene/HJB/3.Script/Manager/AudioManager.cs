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
    [SerializeField] private Slider sfx_slider;

    private float masterValue;
    private float bgmValue;
    private float sfxValue;

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
    public void SliderControll(int num, bool check)
    {
        switch (num)
        {
            case 0:
                if (check)
                {

                    Master_VolumeSet();
                    master_slider.interactable = true;
                }
                else
                {
                    audioMixer.SetFloat("Master", -80f);
                    master_slider.interactable = false;
                }
                break;
            case 1:
                if (check)
                {
                    BGM_VolumeSet();
                    bgm_slider.interactable = true;
                }
                else
                {
                    audioMixer.SetFloat("BGM", -80f);
                    bgm_slider.interactable = false;
                }
                break;
            case 2:
                if (check)
                {
                    //SFX_VolumeSet();
                    sfx_slider.interactable = true;
                }
                else
                {
                    //audioMixer.SetFloat("SFX", -80f);
                    sfx_slider.interactable = false;
                }
                break;
        }
    }
    
    public void Master_VolumeSet()
    {
        audioMixer.SetFloat("Master", Mathf.Log10(master_slider.value) * 20f);
    }

    public void BGM_VolumeSet()
    {
        audioMixer.SetFloat("BGM", Mathf.Log10(bgm_slider.value) * 20f);
    }
    public void SFX_VolumeSet()
    {
       
    }


}
