using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject MainMenuCanvas;
    [SerializeField] private GameObject StartMenuCanvas;
    [SerializeField] private GameObject PetSelectCavas;
    [SerializeField] private GameObject CollectionCavas;
    [SerializeField] private GameObject LevelCavas;

    public void OnMainMenuSet_Btn()
    {
        MainMenuCanvas.SetActive(!MainMenuCanvas.activeSelf);
        StartMenuCanvas.SetActive(!StartMenuCanvas.activeSelf);
    }
    public void SceneGame_1()
    {
        SceneManager.LoadScene("HJB_ArithmeticOperationsGame");
    }

    //ȯ�漳�� â On
    public void OnSetting_Btn()
    {
        SettingManager.Instance.Setting_Btn();
    }

    //Application ���� ��ư
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

    public void MetaWorldSceneLoad_Btn()
    {
        PetSelectCavas.SetActive(!PetSelectCavas.activeSelf);
        //SettingManager.Instance.MetaWorldSceneLoad_Btn();
    }
}
