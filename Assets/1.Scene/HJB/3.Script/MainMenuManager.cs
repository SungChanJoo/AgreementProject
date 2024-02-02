using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    //환경설정 창 On
    public void OnSetting_Btn()
    {
        SettingManager.Instance.Setting_Btn();
    }
}
