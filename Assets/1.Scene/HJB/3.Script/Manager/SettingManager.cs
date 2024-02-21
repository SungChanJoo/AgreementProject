using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Android;


public class SettingManager : MonoBehaviour
{
	public static SettingManager Instance = null;
    
    [SerializeField] private GameObject setting_Canvas;    
    
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
    //Application 종료 버튼
    public void ApplicationExit_Btn()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}
