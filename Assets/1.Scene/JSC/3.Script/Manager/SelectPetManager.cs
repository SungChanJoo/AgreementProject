using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//���� ���� ��Ÿ���� ������ �����ϴ� �Ŵ���
public class SelectPetManager : MonoBehaviour
{
    public static SelectPetManager Instance = null;
    
    public List<GameObject> PetPrefebs = new List<GameObject>();
    public int SelectedPetIndex = 0;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            //���� ������ ������ ���� �Ѱ��ֱ� ���� 
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

    public void SelectPet(int index)
    {
        //todo 0213 �긮��Ʈ �޾Ƽ� ���þȵǰ� ����ó������
        SelectedPetIndex = index;
    }
}
