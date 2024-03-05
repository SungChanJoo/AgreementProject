using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Android;
using UnityEngine.UI;
using Mirror;

public enum Sound_
{
    Master = 0,
    BGM,
    SFX = 2,
}

public class SettingManager : MonoBehaviour
{
	public static SettingManager Instance = null;
    
    public GameObject setting_Canvas;
    [SerializeField] Canvas setting;
    [SerializeField] private GameObject InGameBtn_panel;
    [SerializeField] private GameObject ReQuestion_panel;        
    
    [Header("Checkmark ON/OFF ������ �������")]
    [SerializeField] private GameObject[] on_img;
    [SerializeField] private GameObject[] Off_img;
    
    [Header("��� OFF�� �����̴��� �÷�����")]
    [SerializeField] private Image[] MasterSlider_color;
    [SerializeField] private Image[] BgmSlider_color;
    [SerializeField] private Image[] SfxSlider_color;

    public bool Stop = true;
    
    
    int count = 0;

    private int sound_num;

    [Header("MetaWorld")]
    public bool IsMetaWorld = false;
    [SerializeField] private GameObject Restart_Btn;
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

    private IEnumerator Start()
    {        
        yield return StartCoroutine(AppSetPermission_Co());
    }
    public void EnableSettingBtn()
    {
        if (SceneManager.GetActiveScene().buildIndex!=1)
        {
            InGameBtn_panel.SetActive(true);
        }
        else
        {
            InGameBtn_panel.SetActive(false);
        }
    }
    //���� �� ó�� ���� �� �ź��ߴٸ� �ξ� ������ �ٽ� ���� ���� ��û�ϱ� ����
    public void Re_AppSetPermission()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera) ||
            !Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead) ||
            !Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
        {
            string[] permissionsToRequest =
                { Permission.ExternalStorageRead, Permission.ExternalStorageWrite, Permission.Camera };
            Permission.RequestUserPermissions(permissionsToRequest);
        }
    }         
    
    private IEnumerator AppSetPermission_Co()
    {
        //����� �� Camera ���� ��û
        string[] permissionsToRequest =
            { Permission.ExternalStorageRead, Permission.ExternalStorageWrite, Permission.Camera };
        Permission.RequestUserPermissions(permissionsToRequest);


        //������ �ο��Ǿ����� Ȯ�� True�� ��ȯ�ϱ� ������ ����
        yield return new WaitUntil(() =>
            Permission.HasUserAuthorizedPermission(Permission.Camera) &&
            Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead) &&
            Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite)
        );
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera) ||
            !Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead) ||
            !Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
        {
            Debug.Log("���� �ź�");
        }

        //Camera�� ���� ��� ó��
        if (WebCamTexture.devices.Length == 0)
        {
            yield break;
        }
        yield return null;
    }
    //ȯ�漳�� UI Off
    public void Setting_Btn()
    {
        //ĵ������ �Ҵ�� ī�޶� ���ٸ�
        if (setting.worldCamera == null)
        {
            setting.worldCamera = Camera.main;
        }
        if (TimeSlider.Instance != null&&!Stop)
        {
            TimeSlider.Instance.TimeSliderControll();
        }
        setting_Canvas.SetActive(!setting_Canvas.activeSelf);
    }
    public void NonTemporalSetting_Btn()
    {
        //ĵ������ �Ҵ�� ī�޶� ���ٸ�
        if (setting.worldCamera == null)
        {
            setting.worldCamera = Camera.main;
        }
        setting_Canvas.SetActive(!setting_Canvas.activeSelf);
        if(IsMetaWorld)
        {
            Restart_Btn.SetActive(false);
        }
    }
    
    public void MetaWorldSceneLoad_Btn()
    {
        SceneManager.LoadScene("JSC_Test_MetaWorld");
    }
    public void InGameExit_UI()
    {
        ReQuestion_panel.SetActive(!ReQuestion_panel.activeSelf);
    }
    public void ReStartGameScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void NextMainScene()
    {

        //�޴� Scene �����ȣ 1�� ����
        SceneManager.LoadScene(1);
        if (IsMetaWorld)
        {
            //��Ÿ����ȿ��� ȣ���ϸ� �� �̵��� �ϰ� �Ǹ� ������ �� ���·� ������
            Restart_Btn.SetActive(true);
            NetworkClient.Disconnect();
            Destroy(FindObjectOfType<PetSwitchNetworkManager>().gameObject);
            Destroy(FindObjectOfType<CrewSelectManager>().gameObject);
            if (AudioManager.Instance != null)
                AudioManager.Instance.BGM_Play(0);
            IsMetaWorld = false;
        }

    }
    public void Sound_Num(int num)
    {
        sound_num = num;
    }
    public void SoundMutEvent(bool check)
    {

        if (!check)
        {
            Off_img[sound_num].SetActive(true);
            on_img[sound_num].SetActive(false);           
        }
        else
        {
            on_img[sound_num].SetActive(true);
            Off_img[sound_num].SetActive(false);
        }        
        SliderColorChange(sound_num,check);
    }    
    private void SliderColorChange(int num , bool check)
    {
        for (int i = 0; i < MasterSlider_color.Length; i++)
        {
            switch (num)
            {
                case 0:
                    if (!check)
                    {
                        MasterSlider_color[i].color = Color.gray;
                    }
                    else
                    {
                        MasterSlider_color[i].color = new Color(1, 1, 1);
                    }
                    break;
                case 1:
                    if (!check)
                    {
                        BgmSlider_color[i].color = Color.gray;
                    }
                    else
                    {
                        BgmSlider_color[i].color = new Color(1, 1, 1);
                    }
                    break;
                case 2:
                    if (!check)
                    {
                        SfxSlider_color[i].color = Color.gray;
                    }
                    else
                    {
                        SfxSlider_color[i].color = new Color(1, 1, 1);
                    }
                    break;
            }            
        }
        AudioManager.Instance.SliderControll(num,check);

        
    }
    private void PlayerSaveData()
    { 
        //���⼭ ������ �޼��� �޾Ƽ� Save �� ��.(Player_DB����)
        //DataBase.Instance.PlayerCharacter[0];
    }
     


    //Application ���� ��ư
    public void ApplicationExit_Btn()
    {
        
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}
