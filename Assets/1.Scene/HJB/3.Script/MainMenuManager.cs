using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    //ȯ�漳�� â On
    public void OnSetting_Btn()
    {
        SettingManager.Instance.Setting_Btn();
    }
}
