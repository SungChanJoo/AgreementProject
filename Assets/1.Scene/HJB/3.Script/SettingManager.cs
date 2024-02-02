using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingManager : MonoBehaviour
{
	public static SettingManager Instance = null;

    [SerializeField] private GameObject setting_Btn;

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
        }
    }

    public void Setting_Btn()
    {
        setting_Btn.SetActive(!setting_Btn.activeSelf);
    }
}
