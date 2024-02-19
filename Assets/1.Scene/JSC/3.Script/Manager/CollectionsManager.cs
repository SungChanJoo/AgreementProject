using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//���� ���� ��Ÿ���� ������ �����ϴ� �Ŵ���
public class CollectionsManager : MonoBehaviour
{
    public ExpenditionCrew collections;
    [SerializeField] GameObject Content;
    public GameObject UI;


    private void Awake()
    {
        SetCollections();
    }
    public void LoadMetaWorld(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void SelectPet(int index)
    {
        //todo 0213 �긮��Ʈ �޾Ƽ� ���þȵǰ� ����ó������
        SelectCrewManager.Instance.SelectedCrewIndex = index;
        //todo 0219 ���õ� ��� DB�� ���������
        collections.SelectedCrew = index;
    }

    public void ViewDetails()
    {

    }

    //DB���� Ž����� �ݷ��� �ҷ�����
    public void SetCollections()
    {

        //todo 0219 DB�� ������ �޾Ƽ� ���õȴ���� �����Ѵ�� ����Ʈ �޾ƿ���
        int selectedCrew = 0;
        List<bool> ownedCrew = new List<bool>();
        ownedCrew.Add(true);
        ownedCrew.Add(false);
        collections = new ExpenditionCrew(selectedCrew, ownedCrew);
    }
}
