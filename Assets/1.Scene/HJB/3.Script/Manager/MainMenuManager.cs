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



    public void SceneGame_1()
    {
        SceneManager.LoadScene("HJB_ArithmeticOperationsGame");
    }

    

    //Application 종료 버튼
    public void Exit_Btn()
    {
        SettingManager.Instance.ApplicationExit_Btn();
    }
    public void CollectionShop_UI()
    {
        CollectionCavas.SetActive(!CollectionCavas.activeSelf);
    }

    public void Select_Level(int level)
    {
        SceneGame_1();
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
    //환경설정 창 On
    public void Setting_UI()
    {
        SettingManager.Instance.Setting_Btn();
    }
    public void Profile_UI()
    {
        ProfileCanvas.SetActive(!ProfileCanvas.activeSelf);
    }
}
