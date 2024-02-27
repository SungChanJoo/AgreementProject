using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Android;
using UnityEngine.UI;


public class SettingManager : MonoBehaviour
{
	public static SettingManager Instance = null;
    
    [SerializeField] private GameObject setting_Canvas;
    [SerializeField] private GameObject InGameBtn_panel;
    [SerializeField] private GameObject ReQuestion_panel;        
    
    [Header("토글 ON/OFF 위부터 순서대로")]
    [SerializeField] private Toggle[] on_tog;
    [SerializeField] private Toggle[] off_tog;
    [Header("토글 OFF시 슬라이더바 컬러조정")]
    [SerializeField] private Image[] MasterSlider_color;
    [SerializeField] private Image[] BgmSlider_color;
    [SerializeField] private Image[] SfxSlider_color;

    int count = 0;
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
    //만약 앱 처음 실행 시 거부했다면 인앱 내에서 다시 접근 권한 요청하기 위함
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
        //저장소 및 Camera 권한 요청
        string[] permissionsToRequest =
            { Permission.ExternalStorageRead, Permission.ExternalStorageWrite, Permission.Camera };
        Permission.RequestUserPermissions(permissionsToRequest);


        //권한이 부여되었는지 확인 True를 반환하기 전까지 정지
        yield return new WaitUntil(() =>
            Permission.HasUserAuthorizedPermission(Permission.Camera) &&
            Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead) &&
            Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite)
        );
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera) ||
            !Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead) ||
            !Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
        {
            Debug.Log("권한 거부");
        }

        //Camera가 없는 경우 처리
        if (WebCamTexture.devices.Length == 0)
        {
            yield break;
        }
        yield return null;
    }
    //환경설정 UI Off
    public void Setting_Btn()
    {
        if (TimeSlider.Instance != null)
        {
            TimeSlider.Instance.TimeSliderControll();
        }
        setting_Canvas.SetActive(!setting_Canvas.activeSelf);
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
        //메뉴 Scene 빌드번호 1로 지정
        SceneManager.LoadScene(1);
    }

    public void MasterSoundTogglEvent(bool check)
    {        
        if (!check)
        {
            on_tog[0].isOn = !off_tog[0].isOn;
        }
        else
        {
            off_tog[0].isOn = !on_tog[0].isOn;
        }
        SliderColorChange(0);
    }
    public void BGMSoundTogglEvent(bool check)
    {
        if (!check)
        {
            on_tog[1].isOn = !off_tog[1].isOn;
        }
        else
        {
            off_tog[1].isOn = !on_tog[1].isOn;
        }
        SliderColorChange(1);
    }
    public void SFXSoundTogglEvent(bool check)
    {
        if (!check)
        {
            on_tog[2].isOn = !off_tog[2].isOn;
        }
        else
        {
            off_tog[2].isOn = !on_tog[2].isOn;
        }
        SliderColorChange(2);
    }
    private void SliderColorChange(int num)
    {
        for (int i = 0; i < MasterSlider_color.Length; i++)
        {
            switch (num)
            {
                case 0:
                    if (!on_tog[0].isOn)
                    {
                        MasterSlider_color[i].color = Color.gray;
                    }
                    else
                    {
                        MasterSlider_color[i].color = new Color(1, 1, 1);
                    }
                    break;
                case 1:
                    if (!on_tog[1].isOn)
                    {
                        BgmSlider_color[i].color = Color.gray;
                    }
                    else
                    {
                        BgmSlider_color[i].color = new Color(1, 1, 1);
                    }
                    break;
                case 2:
                    if (!on_tog[2].isOn)
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
        AudioManager.Instance.SliderControll(num,on_tog[num].isOn);

        
    }
    //Application 종료 버튼
    public void ApplicationExit_Btn()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}
