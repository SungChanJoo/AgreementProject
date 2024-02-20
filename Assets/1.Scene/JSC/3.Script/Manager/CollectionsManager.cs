using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System;
using kcp2k;

public class Crew
{
    public GameObject DetailCrew;
    public Text CrewName;
    public Text CrewDescript;
}

//���� ���� ��Ÿ���� ������ �����ϴ� �Ŵ���
public class CollectionsManager : MonoBehaviour
{
    const string _selectedCrew = "�⵿!";
    const string _ownedCrew = "�⵿ ���";
    const byte _selectedR = 83;
    const byte _selectedG = 88;
    const byte _selectedB = 101;
    const byte _defaultR = 126;
    const byte _defaultG = 163;
    const byte _defaultB = 255;
    const byte _deniedPurchaseR = 150;
    const byte _deniedPurchaseG = 150;
    const byte _deniedPurchaseB = 150;

    public static CollectionsManager Instance = null;

    [Header("Collections")]
    public ExpenditionCrew collections;
    public List<GameObject> CrewList;
    private List<TMP_Text> _crewStatusText;
    private List<GameObject> _crewStatusBtn;

    [Header("��")]
    private int _money = 200;
    [SerializeField] private TMP_Text moneyText;

    [Header("ViewDetails")]
    public GameObject DetailWindow;
    public List<Crew> CrewInfo;


    public Action OnCheckPurchasePossibility; //�� ���ŵɶ����� ȣ��

    private void Awake()
    {
        if (Instance == null)
            Instance = new CollectionsManager();
        else
            Destroy(gameObject);
        //todo 0220 DB���� �÷��̾� �� �޾ƿͼ� �Ҵ�����
        moneyText.text = $"{_money}";
        SetCollections();
    }
    private void OnEnable()
    {
        OnCheckPurchasePossibility += CheckPurchasePossibility;
    }
    private void OnDisable()
    {
        OnCheckPurchasePossibility -= CheckPurchasePossibility;
    }
    public void LoadMetaWorld(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    //Ž����� ����
    public void SelectPet(int selectIndex)
    {
        //����� "�⵿ ���" ���¶�� "�⵿!" ����, ���� "�⵿!"�� "�⵿ ���" ���·� ����
        if (_crewStatusText[selectIndex].text.Equals(_ownedCrew))
        {
            //"�⵿!" ã��
            for (int i = 0; i < _crewStatusText.Count; i++)
            {
                //�������̰�, "�⵿!" ������ Ž�����
                if(collections.OwnedCrew[i] && _crewStatusText[i].text.Equals(_selectedCrew))
                {

                    _crewStatusText[i].text = _ownedCrew;
                    SetBtnColor(_crewStatusBtn[i].GetComponent<Image>(), new Color32(_defaultR, _defaultG, _defaultB, 255));
                    break;
                }
            }
            _crewStatusText[selectIndex].text = _selectedCrew;
            SelectCrewManager.Instance.SelectedCrewIndex = selectIndex;
            SetBtnColor(_crewStatusBtn[selectIndex].GetComponent<Image>(), new Color32(_selectedR, _selectedG, _selectedB, 255));
            //���õ� ��� DB�� ����
            collections.SelectedCrew = selectIndex;
        }
    }

    public void ViewDetails()
    {

    }
    //��ü ���, ���� ��� ��ȯ
    public void ToggleOwnedCrew()
    {
        for(int i =0; i < CrewList.Count; i++)
        {
            if (!_crewStatusText[i].text.Equals(_selectedCrew) && !_crewStatusText[i].text.Equals(_ownedCrew))
            {
                CrewList[i].SetActive(!CrewList[i].activeSelf);
            }

        }
    }
    //DB���� Ž���� �ݷ��� �ҷ�����
    public void SetCollections()
    {
        //todo 0219 DB�� ������ �޾Ƽ� ���õ� ����� ������ ��� ����Ʈ �޾ƿ���
        int selectedCrew = 0;
        List<bool> ownedCrew = new List<bool>();
        ownedCrew.Add(true);
        ownedCrew.Add(false);
        ownedCrew.Add(false);
        ownedCrew.Add(false);
        ownedCrew.Add(false);
        ownedCrew.Add(false);
        ownedCrew.Add(false);

        collections = new ExpenditionCrew(selectedCrew, ownedCrew);

        //�ؽ�Ʈ, ��ư �ʱ�ȭ
        _crewStatusBtn = new List<GameObject>();
        _crewStatusText = new List<TMP_Text>();
        for (int i = 0; i < CrewList.Count; i++)
        {
            _crewStatusBtn.Add( CrewList[i].transform.Find("Button").gameObject);
            _crewStatusText.Add(_crewStatusBtn[i].transform.Find("Text (TMP)").gameObject.GetComponent<TMP_Text>());
        }

        for (int i=0; i< CrewList.Count; i++)
        {
            var buttonImg = _crewStatusBtn[i].GetComponent<Image>();
            var button = _crewStatusBtn[i].GetComponent<Button>();
            int crewIndex = i; //Ŭ���� ����

            //���õǾ� �ִ� ����� "�⵿" �ؽ�Ʈ
            if (collections.SelectedCrew == i)
            {
                button.onClick.AddListener(() => SelectPet(crewIndex));
                _crewStatusText[i].text = _selectedCrew;
                SetBtnColor(buttonImg, new Color32(_selectedR, _selectedG, _selectedB, 255));
            }
            //������ ����� "�⵿ ���" �ؽ�Ʈ
            else if (collections.OwnedCrew[i])
            {
                button.onClick.AddListener(() => SelectPet(crewIndex));
                _crewStatusText[i].text = _ownedCrew;
                SetBtnColor(buttonImg, new Color32(_defaultR, _defaultG, _defaultB, 255));
            }
            //���԰����� ����� Cost �ؽ�Ʈ
            else
            {
                button.onClick.AddListener(() => PurchaseCrew(crewIndex));
                //������ �ݾ׺��� Ž������ ����� �� ��θ� deniedPurchase
                if (_money< Convert.ToInt32(_crewStatusText[i].text))
                {
                    SetBtnColor(buttonImg, new Color32(_deniedPurchaseR, _deniedPurchaseG, _deniedPurchaseB, 255));
                }
                //�ƴϸ� ���԰���
                else
                {
                    SetBtnColor(buttonImg, new Color32(_defaultR, _defaultG, _defaultB, 255));
                }
            }
        }
    }


    //��� ����
    public void PurchaseCrew(int index)
    {
        var crewCost = Convert.ToInt32(_crewStatusText[index].text);
        if (_money >= crewCost)
        {
            //�� ����
            _money -= crewCost;
            moneyText.text = $"{_money}";

            //DB�� �ݿ�
            collections.OwnedCrew[index] = true;
            //�ؽ�Ʈ ����
            _crewStatusText[index].text = _ownedCrew;
            OnCheckPurchasePossibility?.Invoke();
            //��ư �̺�Ʈ �߰�
            var button = _crewStatusBtn[index].GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => SelectPet(index));
        }
        else
        {
            Debug.Log("������ �� ����");
        }
    }
    //���Ű������� Ȯ��
    public void CheckPurchasePossibility()
    {
        for (int i = 0; i < _crewStatusText.Count; i++)
        {
            if (collections.OwnedCrew[i]) continue;
            //�������� �ƴϰ�, ������ ���� ���� ������ ��θ� ��ư �� ����
            if (_money < Convert.ToInt32(_crewStatusText[i].text))
            {
                SetBtnColor(_crewStatusBtn[i].GetComponent<Image>(), new Color32(_deniedPurchaseR, _deniedPurchaseG, _deniedPurchaseB, 255));
            }
            else
            {
                SetBtnColor(_crewStatusBtn[i].GetComponent<Image>(), new Color32(_defaultR, _defaultG, _defaultB, 255));
            }
        }
    }
    //��ư �� ����
    public void SetBtnColor(Image button, Color color)
    {
        button.color = color;
    }
}
