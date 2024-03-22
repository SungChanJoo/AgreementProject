using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class GameStartAniamation : MonoBehaviour
{
    [SerializeField] private GameSetting gameSetting;    
    [SerializeField] private GameObject go_image;
    [SerializeField] private Sprite[] ready_img;

    [SerializeField] private AudioClip[] audioClips;
    
    private Animator startAni;
    private AudioSource source;
    private Image start_img;
    Game_Type Gametype;
    int scene_num;    
    private void Awake()
    {
        start_img = go_image.GetComponent<Image>();        
        startAni = GetComponent<Animator>();
        source = GetComponent<AudioSource>();
        
    }
    

    public void StartAniamtion()
    {
        go_image.SetActive(true);
        startAni.SetTrigger("StartAnimation");
        
    }
    public void Three_CallBack()
    {        
        source.PlayOneShot(audioClips[0]);
        start_img.sprite = ready_img[0];
    }
    public void Two_CallBack()
    {
        source.PlayOneShot(audioClips[1]);
        start_img.sprite = ready_img[1];
    }
    public void One_CallBack()
    {
        source.PlayOneShot(audioClips[2]);
        start_img.sprite = ready_img[2];
    }
    public void Start_CallBack()
    {
        source.PlayOneShot(audioClips[3]);        
        start_img.sprite = ready_img[3];
    }
    public void StartGame_CallBack()
    {
        gameObject.SetActive(false);
        Gametype = gameSetting.game_Type;
        switch (Gametype)
        {
            case Game_Type.A:
            case Game_Type.B:
                scene_num = 1;
                break;
            case Game_Type.C:
            case Game_Type.D:
            case Game_Type.E:
                scene_num = 2;
                break;
        }
        AudioManager.Instance.BGM_Play(scene_num);
        gameSetting.GameStart_Btn();
    }
}
