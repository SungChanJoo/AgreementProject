using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class MainMenuManager : MonoBehaviour
{    
    [SerializeField] private GameObject MainMenuCanvas;
    [SerializeField] private GameObject PetSelectCavas;
    [SerializeField] private GameObject CollectionCavas;
    [SerializeField] private GameObject LevelCavas;
    [SerializeField] private GameObject CameraCavas;
    [SerializeField] private GameObject ProfileCanvas;
    [SerializeField] private GameObject StepCanvas;

    public void SceneGame_1()
    {
        SceneManager.LoadScene("HJB_ElementaryArithmeticGame");
    }

    //Application ���� ��ư
    public void Exit_Btn()
    {
        SettingManager.Instance.ApplicationExit_Btn();
    }
    public void CollectionShop_UI()
    {
        CollectionCavas.SetActive(!CollectionCavas.activeSelf);        
    }

    
    //������ Level �Ҵ� �̺�Ʈ
    public void SelectLevel_Btn(int level)
    {
        //StepUI�� ��Ȱ��ȭ�̸�..
        if (!StepCanvas.activeInHierarchy)
        {
            //Level ���� �� StepUI Ȱ��ȭ, LevelUI ��Ȱ��ȭ
            Step_UI();
            Level_UI();
        }
        StepManager.Instance.SelectLevel(level);        
    }
    
    
    //������ Step �Ҵ� �̺�Ʈ
    public void SelectStep(int step)
    {
        StepManager.Instance.SelectStep(step);
        SceneGame_1();
    }
    public void SelectTime(int time)
    {
        StepManager.Instance.SelectTimeSet(time);
        Debug.Log(time);
    }

    
    public void Level_UI()
    {
        LevelCavas.SetActive(!LevelCavas.activeSelf);
    }
    public void Camera_UI()
    {
        CameraCavas.SetActive(!CameraCavas.activeSelf);
    }
    public void MetaWorldPetSelect_UI()
    {
        PetSelectCavas.SetActive(!PetSelectCavas.activeSelf);        
    }
    //ȯ�漳�� â On
    public void Setting_UI()
    {
        SettingManager.Instance.Setting_Btn();
    }
    public void Profile_UI()
    {
        ProfileCanvas.SetActive(!ProfileCanvas.activeSelf);
    }
    public void Step_UI()
    {
        StepCanvas.SetActive(!StepCanvas.activeSelf);
    }
}
