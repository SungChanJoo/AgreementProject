using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject MainMenuCanvas;
    [SerializeField] private GameObject StartRegistCanvas;
    [SerializeField] private GameObject TitleCancas;
    [SerializeField] private GameObject PetSelectCavas;
    [SerializeField] private GameObject CollectionCavas;
    [SerializeField] private GameObject LevelCavas;
    [SerializeField] private GameObject CameraCavas;

        
    public void OnMainMenuSet_Btn()
    {
        MainMenuCanvas.SetActive(!MainMenuCanvas.activeSelf);
        StartRegistCanvas.SetActive(!StartRegistCanvas.activeSelf);
    }
    public void SceneGame_1()
    {
        SceneManager.LoadScene("HJB_ArithmeticOperationsGame");
    }

    //환경설정 창 On
    public void OnSetting_Btn()
    {
        SettingManager.Instance.Setting_Btn();
    }

    //Application 종료 버튼
    public void Exit_Btn()
    {
        SettingManager.Instance.ApplicationExit_Btn();
    }
    public void CollectionShop_Btn()
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
    public void MetaWorldSceneLoad_Btn()
    {
        PetSelectCavas.SetActive(!PetSelectCavas.activeSelf);
        //SettingManager.Instance.MetaWorldSceneLoad_Btn();
    }
}
