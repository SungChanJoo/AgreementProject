using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//�ε�ȭ�� ��Ʈ�� Ŭ����
public class LoadingSceneController : MonoBehaviour
{
    static string nextScene = string.Empty;
    [SerializeField] Image progressBar;

    public static void LoadScene(string sceneName)
    {
        nextScene = sceneName;
        SceneManager.LoadScene("LoadingScene");
    }

    private void Start()
    {
        //���۽� �ٷ� title�� �Ѿ��
        if(nextScene == string.Empty)
        {
            nextScene = "HJB_Title";
            StartCoroutine(LoadSceneProcess());
        }
        else
        {
            StartCoroutine(LoadSceneProcess());
        }
    }

    //�ε� ��ũ�ѹ� ó��
    IEnumerator LoadSceneProcess()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(nextScene);
        op.allowSceneActivation = false;

        float timer = 0f;
        while(!op.isDone)
        {
            yield return null;
            if(op.progress<0.9f)
            {
                progressBar.fillAmount = op.progress;
            }
            else
            {
                timer += Time.unscaledDeltaTime;
                progressBar.fillAmount = Mathf.Lerp(0.9f, 1f, timer);
                if (progressBar.fillAmount >= 1f)
                {
                    op.allowSceneActivation = true;
                    yield break;
                }
            }
        }
    }
}
