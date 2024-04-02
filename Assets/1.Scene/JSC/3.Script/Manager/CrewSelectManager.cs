using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using LitJson;
using System.IO;

//Ž������ �����ϰ� ��Ÿ���� ������ �����ϴ� Ŭ����
public class CrewSelectManager : MonoBehaviour
{
    public static CrewSelectManager Instance = null;
    [SerializeField] private GameObject crewContents;
    private List<GameObject> crewList = new List<GameObject>();
    [SerializeField] private GameObject crewSelectCanvas;
    [SerializeField] private GameObject enterMetaWorldCanvas;
    [SerializeField] private TMP_InputField metaWorldIpText;
    private List<TMP_Text> crewStatusText;
    private List<GameObject> crewStatusBtn;
    public int SelectedCrewIndex = 0;
    string path = string.Empty;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        if (path.Equals(string.Empty))
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                path = Application.persistentDataPath + "/License";
            }
            else
                path = Application.dataPath + "/License";
        }
    }
    private void Start()
    {
        SetSeleteCrewUI();
    }
    public void LoadMetaWorld(string sceneName)
    {
        SettingIp(path);
        LoadingSceneController.LoadScene(sceneName);
        //��Ÿ�� ����� �뷡 ���߱�
        if(AudioManager.Instance != null)
           AudioManager.Instance.BGM_Stop();
        //���� ������ ������ ����� �Ѱ��ֱ� ���� 
        DontDestroyOnLoad(gameObject);
        if(SettingManager.Instance != null)
            SettingManager.Instance.IsMetaWorld = true;
    }
    private void SettingIp(string path)
    {
        List<Item> item = new List<Item>();
        item.Add(new Item($"{Type.Client}", metaWorldIpText.text, "7777"));

        JsonData data = JsonMapper.ToJson(item);
        File.WriteAllText(path + "/License.json", data.ToString());
    }

    public void OnViewSelectCrewUI()
    {
        crewSelectCanvas.SetActive(!crewSelectCanvas.activeSelf);
        SelectedCrewIndex = CollectionsManager.Instance.Collections.SelectedCrew;
        for (int i = 0; i < crewList.Count; i++)
        {
            //�������� ����� �ƴҰ�� ���ӿ�����Ʈ Ȱ��ȭ -> ��Ȱ��ȭ, ��Ȱ��ȭ -> Ȱ��ȭ
            if (CollectionsManager.Instance.Collections.OwnedCrew[i])
            {
                crewList[i].SetActive(true);
                //���õǾ� �ִ� ����� "�⵿!" �ؽ�Ʈ
                if(SelectedCrewIndex == i)
                {
                    crewStatusText[i].text = CrewButton._selectedCrew;
                    //_crewStatusBtn[i].GetComponent<Image>().color = CollectionsManager.Instance.SelectedBtnColor;
                    crewStatusBtn[i].GetComponent<Image>().sprite = CollectionsManager.Instance.SelectedImg;
                }
                //������ ����� "�⵿ ���" �ؽ�Ʈ
                else
                {
                    crewStatusText[i].text = CrewButton._ownedCrew;
                    //_crewStatusBtn[i].GetComponent<Image>().color = CollectionsManager.Instance.DefaultBtnColor;
                    crewStatusBtn[i].GetComponent<Image>().sprite = CollectionsManager.Instance.DefaultImg;
                }
            }
            else
                crewList[i].SetActive(false);
        }
    }
    public void SetSeleteCrewUI()
    {
        //Ž���� ������ �ʱ�ȭ
        for (int i = 0; i < crewContents.transform.childCount; i++)
        {
            crewList.Add(crewContents.transform.GetChild(i).gameObject);
        }
        //�ؽ�Ʈ, ��ư �ʱ�ȭ
        crewStatusBtn = new List<GameObject>();
        crewStatusText = new List<TMP_Text>();
        for (int i = 0; i < crewList.Count; i++)
        {
            crewStatusBtn.Add(crewList[i].transform.Find("Button").gameObject);
            crewStatusText.Add(crewStatusBtn[i].transform.Find("Text (TMP)").gameObject.GetComponent<TMP_Text>());
            
            var button = crewStatusBtn[i].GetComponent<Button>();
            int crewNumber = i; //Ŭ���� ����
            button.onClick.AddListener(() => OnSelectPet(crewNumber));
        }
    }

    //Ž���� ����
    public void OnSelectPet(int selectIndex)
    {
        //����� "�⵿ ���" -> "�⵿!" ����, ���� "�⵿!" -> "�⵿ ���" ���·� ����
        if (crewStatusText[selectIndex].text.Equals(CrewButton._ownedCrew))
        {
            //"�⵿!" ������ Ž���� "�⵿!" -> "�⵿ ���" 
            crewStatusBtn[SelectedCrewIndex].GetComponent<Image>().sprite = CollectionsManager.Instance.DefaultImg;
            crewStatusText[SelectedCrewIndex].text = CrewButton._ownedCrew;
            //"�⵿ ���" -> "�⵿!"
            crewStatusText[selectIndex].text = CrewButton._selectedCrew;
            crewStatusBtn[selectIndex].GetComponent<Image>().sprite = CollectionsManager.Instance.SelectedImg;

            //�󼼺��� ��ư ����
            SelectedCrewIndex = selectIndex;
            //���õ� ��� ������ �ݿ�
            CollectionsManager.Instance.OnSelectPet(selectIndex);
        }
    }
    //��Ÿ���� Ip�Է�â ����
    public void OnViewEnterCanvas()
    {

        enterMetaWorldCanvas.SetActive(!enterMetaWorldCanvas.activeSelf);
    }
}
