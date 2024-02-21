using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//���� ���� ��Ÿ���� ������ �����ϴ� �Ŵ���
public class SelectCrewManager : MonoBehaviour
{
    public static SelectCrewManager Instance = null;

    public int SelectedCrewIndex = 0;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            //���� ������ ������ ����� �Ѱ��ֱ� ���� 
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    public void LoadMetaWorld(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
