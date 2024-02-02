using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject MainMenuCanvas;
    [SerializeField] private GameObject StartMenuCanvas;
    public void OnMainMenuSet_Btn()
    {
        MainMenuCanvas.SetActive(!MainMenuCanvas.activeSelf);
        StartMenuCanvas.SetActive(!StartMenuCanvas.activeSelf);
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

    public void MetaWorldSceneLoad_Btn()
    {
        SettingManager.Instance.MetaWorldSceneLoad_Btn();
    }
}
