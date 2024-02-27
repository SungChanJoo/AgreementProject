using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

//���� ���� ��Ÿ���� ������ �����ϴ� �Ŵ���
public class CrewSelectManager : MonoBehaviour
{
    public static CrewSelectManager Instance = null;
    public List<GameObject> _crewList;
    public GameObject CrewSelectCanvas;
    private List<TMP_Text> _crewStatusText;
    private List<GameObject> _crewStatusBtn;
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
    private void Start()
    {
        SetSeleteCrewUI();
    }
    public void LoadMetaWorld(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void OnViewSelectCrewUI()
    {
        CrewSelectCanvas.SetActive(!CrewSelectCanvas.activeSelf);
        SelectedCrewIndex = CollectionsManager.Instance.Collections.SelectedCrew;
        for (int i = 0; i < _crewList.Count; i++)
        {
            //�������� ����� �ƴҰ�� ���ӿ�����Ʈ Ȱ��ȭ -> ��Ȱ��ȭ, ��Ȱ��ȭ -> Ȱ��ȭ
            if (CollectionsManager.Instance.Collections.OwnedCrew[i])
            {
                _crewList[i].SetActive(true);
                //���õǾ� �ִ� ����� "�⵿!" �ؽ�Ʈ
                if(SelectedCrewIndex == i)
                {
                    _crewStatusText[i].text = CrewButton._selectedCrew;
                    //_crewStatusBtn[i].GetComponent<Image>().color = CollectionsManager.Instance.SelectedBtnColor;
                    _crewStatusBtn[i].GetComponent<Image>().sprite = CollectionsManager.Instance.SelectedImg;
                }
                //������ ����� "�⵿ ���" �ؽ�Ʈ
                else
                {
                    _crewStatusText[i].text = CrewButton._ownedCrew;
                    //_crewStatusBtn[i].GetComponent<Image>().color = CollectionsManager.Instance.DefaultBtnColor;
                    _crewStatusBtn[i].GetComponent<Image>().sprite = CollectionsManager.Instance.DefaultImg;
                }
            }
            else
                _crewList[i].SetActive(false);
        }
    }
    public void SetSeleteCrewUI()
    {
        //�ؽ�Ʈ, ��ư �ʱ�ȭ
        _crewStatusBtn = new List<GameObject>();
        _crewStatusText = new List<TMP_Text>();
        for (int i = 0; i < _crewList.Count; i++)
        {
            _crewStatusBtn.Add(_crewList[i].transform.Find("Button").gameObject);
            _crewStatusText.Add(_crewStatusBtn[i].transform.Find("Text (TMP)").gameObject.GetComponent<TMP_Text>());
            
            var button = _crewStatusBtn[i].GetComponent<Button>();
            int crewNumber = i; //Ŭ���� ����
            button.onClick.AddListener(() => OnSelectPet(crewNumber));
        }
    }

    //Ž���� ����
    public void OnSelectPet(int selectIndex)
    {
        Debug.Log("SelectPet");
        //�̹� ���õ� ����Դϴ�.
/*        if (CollectionsManager.Instance.Collections.SelectedCrew == selectIndex)
        {
            StartCoroutine(ViewAlreadySeletedCrew_co());
        }*/
        //else
        {
            //����� "�⵿ ���" -> "�⵿!" ����, ���� "�⵿!" -> "�⵿ ���" ���·� ����
            if (_crewStatusText[selectIndex].text.Equals(CrewButton._ownedCrew))
            {

                //"�⵿!" ������ Ž���� "�⵿!" -> "�⵿ ���" 
                _crewStatusBtn[SelectedCrewIndex].GetComponent<Image>().sprite = CollectionsManager.Instance.DefaultImg;
                _crewStatusText[SelectedCrewIndex].text = CrewButton._ownedCrew;
                //"�⵿ ���" -> "�⵿!"
                _crewStatusText[selectIndex].text = CrewButton._selectedCrew;
                _crewStatusBtn[selectIndex].GetComponent<Image>().sprite = CollectionsManager.Instance.SelectedImg;
                //_crewStatusBtn[selectIndex].GetComponent<Image>().color = CollectionsManager.Instance.SelectedBtnColor;

                //�󼼺��� ��ư ����
                SelectedCrewIndex = selectIndex;
                //���õ� ��� ������ �ݿ�
                CollectionsManager.Instance.OnSelectPet(selectIndex);
            }
        }
    }
}
