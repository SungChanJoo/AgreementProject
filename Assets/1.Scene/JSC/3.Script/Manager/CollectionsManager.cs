using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System;
using LitJson;
public class Crew
{
    public string CrewName { get; private set; }
    public string CrewDescript { get; private set; }
    public Crew(string crewName, string crewDescript)
    {
        CrewName = crewName;
        CrewDescript = crewDescript;
    }
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
    private ExpenditionCrew _collections;
    [SerializeField]  private List<GameObject> _crewList;
    private List<TMP_Text> _crewStatusText;
    private List<GameObject> _crewStatusBtn;

    [Header("Purchase")]
    [SerializeField] private GameObject _purchaseWindow;
    [SerializeField] private TMP_Text _purchaseText;
    private int _money = 200;
    [SerializeField] private TMP_Text _moneyText;

    [Header("ViewDetails")]
    [SerializeField] private GameObject _detailWindow;
    [SerializeField] private TextAsset _crewDataList;
    private List<Crew> _crewInfo;
    [SerializeField] private List<GameObject> _crewModel;
    [SerializeField] private TMP_Text _crewName;
    [SerializeField] private TMP_Text _crewDescript;
    [SerializeField] private GameObject _detailSelectBtn;

    private int _seletedDetailModel;
    private GameObject _ModelSpace;
    private bool _isDrag = false;
    [SerializeField] private float _rotateSpeedModifier = 1f;
    public Action OnCheckPurchasePossibility; //�� ���ŵɶ����� ȣ��

    private void Awake()
    {
        if (Instance == null)
            Instance = new CollectionsManager();
        else
            Destroy(gameObject);
        //todo 0220 DB���� �÷��̾� �� �޾ƿͼ� �Ҵ�����
        _moneyText.text = $"{_money}";
        SetCollections();
        DetailCrewData();
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

    public void OnViewPurchase(int crewNumber)
    {
        //����â ������
        if (!_purchaseWindow.activeSelf)
        {
            _purchaseText.text = $"[{_crewInfo[crewNumber].CrewName}]�� �����Ͻðڽ��ϱ�?";
        }
        _purchaseWindow.SetActive(!_purchaseWindow.activeSelf);
    }


    #region ����󼼺���
    //����󼼺����ư �̺�Ʈ
    public void OnViewDetails(int crewNumber)
    {
        Debug.Log($"crewNumber : {crewNumber}");
        //�󼼺��Ⱑ ���� ��
        if (!_detailWindow.activeSelf)
        {
            var btnText = _detailSelectBtn.transform.GetChild(0).GetComponent<TMP_Text>();
            btnText.text = _crewStatusText[crewNumber].text;
            SetupCrewSelectionBtn(crewNumber, _detailSelectBtn, btnText);
            if (_crewInfo[crewNumber] != null)
            {
                _crewModel[crewNumber].SetActive(true);
                _crewName.text = $"����̸� : {_crewInfo[crewNumber].CrewName}";
                _crewDescript.text = $"������� : {_crewInfo[crewNumber].CrewDescript}";
                _seletedDetailModel = crewNumber;

            }
            else
            {
                _crewName.text = $"����̸� : ���� {crewNumber}";
                _crewDescript.text = $"������� : ���� {crewNumber}";
            }
            //�� ȸ�� �ڷ�ƾ ����
            if (currentRotateCrewModel_co == null)
            {
                currentRotateCrewModel_co = RotateCrewModel_co();
                StartCoroutine(currentRotateCrewModel_co);
            }
        }
        //�󼼺��Ⱑ ���� ��
        else
        {
            if (_crewInfo[_seletedDetailModel] != null)
            {
                _crewModel[_seletedDetailModel].SetActive(false);
                _crewName.text = string.Empty;
                _crewDescript.text = string.Empty;
                _detailSelectBtn.GetComponent<Button>().onClick.RemoveAllListeners();
            }
            //�� ȸ�� �ڷ�ƾ ����
            if (currentRotateCrewModel_co != null)
            {
                currentRotateCrewModel_co = null;
            }
        }

        _detailWindow.SetActive(!_detailWindow.activeSelf);

    }
    IEnumerator currentRotateCrewModel_co = null;
    //Model�� �ִ� �������� �巡�� �� �� ���� �¿�� ȸ��
    IEnumerator RotateCrewModel_co()
    {
        while (currentRotateCrewModel_co != null)
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    //Model�� �ִ� �����Ǵ�
                    _ModelSpace = EventSystem.current.currentSelectedGameObject;
                    if(_ModelSpace != null && _ModelSpace.name.Equals("Crew_Model"))
                        _isDrag = true;
                }
                if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
                {
                    _isDrag = false;
                }
                //�巡�װ� �����ϴٸ� �� �巡�׿� ���� ȸ��
                if (_isDrag)
                {
                    float _modelRotationY = Input.GetTouch(0).deltaPosition.x * 1f;
                    _crewModel[_seletedDetailModel].transform.Rotate(Vector3.down, _modelRotationY);
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    //Model�� �ִ� �����Ǵ�
                    _ModelSpace = EventSystem.current.currentSelectedGameObject;
                    if(_ModelSpace != null && _ModelSpace.name.Equals( "Crew_Model"))
                        _isDrag = true;
                }
                if(Input.GetMouseButtonUp(0))
                {
                    _isDrag = false;
                }
                //�巡�װ� �����ϴٸ� �� �巡�׿� ���� ȸ��
                if (_isDrag)
                {
                    float _modelRotationY = Input.GetAxis("Mouse X") * 2f;
                    _crewModel[_seletedDetailModel].transform.Rotate(Vector3.down, _modelRotationY);
                }
            }
            yield return null;
        }
    } 
    #endregion
    //��ü ���, ���� ��� ��ȯ
    public void ToggleOwnedCrew()
    {
        for(int i =0; i < _crewList.Count; i++)
        {
            //���õȴ���� �ƴϰ�, �������� ����� �ƴҰ�� ���ӿ�����Ʈ Ȱ��ȭ -> ��Ȱ��ȭ, ��Ȱ��ȭ -> Ȱ��ȭ
            if (!_crewStatusText[i].text.Equals(_selectedCrew) && !_crewStatusText[i].text.Equals(_ownedCrew))
            {
                _crewList[i].SetActive(!_crewList[i].activeSelf);
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
        _collections = new ExpenditionCrew(selectedCrew, ownedCrew);

        //�ؽ�Ʈ, ��ư �ʱ�ȭ
        _crewStatusBtn = new List<GameObject>();
        _crewStatusText = new List<TMP_Text>();
        for (int i = 0; i < _crewList.Count; i++)
        {
            _crewStatusBtn.Add( _crewList[i].transform.Find("Button").gameObject);
            _crewStatusText.Add(_crewStatusBtn[i].transform.Find("Text (TMP)").gameObject.GetComponent<TMP_Text>());
        }

        for (int i=0; i< _crewList.Count; i++)
        {
            SetupCrewSelectionBtn(i);
        }
    }
    public void SetupCrewSelectionBtn(int i, GameObject btn = null, TMP_Text text = null)
    {
        Image buttonImg;
        Button button;
        TMP_Text buttonText;
        if (btn == null && text == null)
        {
            buttonImg = _crewStatusBtn[i].GetComponent<Image>();
            button = _crewStatusBtn[i].GetComponent<Button>();
            buttonText = _crewStatusText[i];
        }
        //�󼼺��� ��ư Setup
        else
        {
            buttonImg = btn.GetComponent<Image>();
            button = btn.GetComponent<Button>();
            buttonText = text;
        }
        int crewIndex = i; //Ŭ���� ����
        //���õǾ� �ִ� ����� "�⵿!" �ؽ�Ʈ
        if (_collections.SelectedCrew == i)
        {
            if (btn == null && text == null) 
                button.onClick.AddListener(() => OnSelectPet(crewIndex));
            else
                button.onClick.AddListener(() => OnSelectPet(crewIndex, btn, text));
            buttonText.text = _selectedCrew;
            SetBtnColor(buttonImg, new Color32(_selectedR, _selectedG, _selectedB, 255));
        }
        //������ ����� "�⵿ ���" �ؽ�Ʈ
        else if (_collections.OwnedCrew[i])
        {
            if (btn == null && text == null) 
                button.onClick.AddListener(() => OnSelectPet(crewIndex));
            else 
                button.onClick.AddListener(() => OnSelectPet(crewIndex, btn, text));
            buttonText.text = _ownedCrew;
            SetBtnColor(buttonImg, new Color32(_defaultR, _defaultG, _defaultB, 255));
        }
        //���԰����� ����� Cost �ؽ�Ʈ
        else
        {
            if (btn == null && text == null) 
                //button.onClick.AddListener(() => OnPurchaseCrew(crewIndex));
                button.onClick.AddListener(() => OnViewPurchase(crewIndex));
            else 
                //button.onClick.AddListener(() => OnPurchaseCrew(crewIndex, btn, text));
                button.onClick.AddListener(() => OnViewPurchase(crewIndex));
            //������ �ݾ׺��� Ž������ ����� �� ��θ� deniedPurchase
            if (_money < Convert.ToInt32(_crewStatusText[i].text))
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
    //Ž����� ����
    public void OnSelectPet(int selectIndex, GameObject btn = null, TMP_Text text = null)
    {
        Debug.Log("SelectPet");
        //����� "�⵿ ���" -> "�⵿!" ����, ���� "�⵿!" -> "�⵿ ���" ���·� ����
        if (_crewStatusText[selectIndex].text.Equals(_ownedCrew))
        {
            //"�⵿!" ã��
            for (int i = 0; i < _crewStatusText.Count; i++)
            {
                // �������̰�, "�⵿!" ������ Ž�����
                if (_collections.OwnedCrew[i] && _crewStatusText[i].text.Equals(_selectedCrew))
                {
                    //"�⵿!" -> "�⵿ ���" 
                    _crewStatusText[i].text = _ownedCrew;
                    SetBtnColor(_crewStatusBtn[i].GetComponent<Image>(), new Color32(_defaultR, _defaultG, _defaultB, 255));
                    break;
                }
            }
            _crewStatusText[selectIndex].text = _selectedCrew;
            SetBtnColor(_crewStatusBtn[selectIndex].GetComponent<Image>(), new Color32(_selectedR, _selectedG, _selectedB, 255));
            //�󼼺��� ��ư ����
            if (btn != null && text != null)
            {
                text.text = _selectedCrew;
                SetBtnColor(btn.GetComponent<Image>(), new Color32(_selectedR, _selectedG, _selectedB, 255));
            }
            SelectCrewManager.Instance.SelectedCrewIndex = selectIndex;
            //���õ� ��� DB�� ����
            _collections.SelectedCrew = selectIndex;
        }
    }
    //��� ����
    public void OnPurchaseCrew(int i, GameObject btn = null, TMP_Text text = null)
    {
        Debug.Log("PurchaseCrew :" + i);

        var crewCost = Convert.ToInt32(_crewStatusText[i].text);
        if (_money >= crewCost)
        {
            //�� ����
            _money -= crewCost;
            _moneyText.text = $"{_money}";

            //DB�� �ݿ�
            _collections.OwnedCrew[i] = true;
            //�ؽ�Ʈ ����
            _crewStatusText[i].text = _ownedCrew;
            //�󼼺��� ��ư ����
            if (btn != null && text != null)
            {
                text.text = _ownedCrew;
            }
            OnCheckPurchasePossibility?.Invoke();
            //��ư �̺�Ʈ �߰�
            var button = _crewStatusBtn[i].GetComponent<Button>();
            //���� ��ư PurchaseCrew �̺�Ʈ ������ -> ��ư SelectPet �̺�Ʈ �ο�
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnSelectPet(i));
            if (btn != null && text != null)
            {
                button = btn.GetComponent<Button>();
                //���� ��ư PurchaseCrew �̺�Ʈ ������ -> ��ư SelectPet �̺�Ʈ �ο�
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => OnSelectPet(i, btn, text));
            }
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
            if (_collections.OwnedCrew[i]) continue;
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
    //Ž����� ����(�̸�, ����) �ҷ�����
    public void DetailCrewData()
    {
        _crewInfo = new List<Crew>();
        JsonData CrewData = JsonMapper.ToObject(_crewDataList.text);
        for(int i =0; i < CrewData.Count; i++)
        {
            _crewInfo.Add(new Crew(CrewData[i]["name"].ToString(), 
                                  CrewData[i]["descript"].ToString()));
        }
    }
}
