using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class GameStartAniamation : MonoBehaviour
{
    [SerializeField] private GameSetting gameSetting;    
    [SerializeField] private GameObject go_image;

    [SerializeField] private AudioClip[] audioClips;

    private TextMeshProUGUI startTimeSet;
    private Animator startAni;
    private AudioSource source;

    private void Awake()
    {
        startTimeSet = GetComponent<TextMeshProUGUI>();
        startAni = startTimeSet.gameObject.GetComponent<Animator>();
        source = GetComponent<AudioSource>();
    }

    public void StartAniamtion()
    {        
        startAni.SetTrigger("StartAnimation");
    }
    public void Three_CallBack()
    {        
        source.PlayOneShot(audioClips[0]);
        startTimeSet.text = "3";
    }
    public void Two_CallBack()
    {
        source.PlayOneShot(audioClips[1]);
        startTimeSet.text = "2";
    }
    public void One_CallBack()
    {
        source.PlayOneShot(audioClips[2]);
        startTimeSet.text = "1";
    }
    public void Start_CallBack()
    {
        source.PlayOneShot(audioClips[3]);
        startTimeSet.text = string.Empty;
        go_image.SetActive(true);
    }
    public void StartGame_CallBack()
    {
        startTimeSet.gameObject.SetActive(false);
        gameSetting.GameStart_Btn();        
    }
}
