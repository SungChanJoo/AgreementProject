using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VeneziaManager : MonoBehaviour
{
    //�������� ���ġ�� ������ �����ϱ� ���� ��ũ��Ʈ
    //������ �����ϸ鼭 ������ �������ְ�, ������� ǥ������ ui�� ���Ƴ����� ���� �� ���� �� ��.

    //�����۰��� ��ȣ�ۿ뵵 ���⿡ ����

    //���� ������ �Ŵ������� ���� , ���� ������ �̹����� �̸��� �������� ������ ������ �Ǵ� �� �� �־����.
    [SerializeField] private Sprite[] sprites;   //�ѱ� �� ���� ������ ��� �� �̹��� sprite
    private string[] KorWord = { "�ð�" , "�Ƚð�"};

    private Dictionary<string, (Sprite, string)> QuestKorean = new Dictionary<string, (Sprite,string)>();

    public static bool isStop = false;
    private void Start()
    {
        for (int i = 0; i < sprites.Length; i++)
        {
            QuestKorean.Add(KorWord[i], (sprites[i], KorWord[i]));
        }
    }

    private void Update()
    {
        //  GameStop();
        Click_Obj();
    }
    //������Ʈ Ŭ���� �Է�ó��
    //Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began > ��ġ�Է�
    private void Click_Obj()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit) && !hit.collider.gameObject.CompareTag("Ground"))
            {
                print(hit.collider.gameObject.name);
                if(Cube.objectType == ObjectType.CorrectAnswer)
                {
                    Score.Instance.Get_FirstScore();
                    ObjectPooling.Instance.cubePool.Add(hit.collider.gameObject);
                    hit.collider.gameObject.SetActive(false);
                }
                

                // ������ ������Ʈ �Ǵ�
                ItemFnc item = hit.collider.gameObject.GetComponent<ItemFnc>();
                if (item != null && item.veneziaItem == VeneziaItem.Meteor) // veneziaItem�� ���׿��� ���
                {
                    print("���׿� Ŭ��!"); // �Ǵ� �ٸ� ������ ����
                    TimeSlider.Instance.DecreaseTime_Item(5);
                    hit.collider.gameObject.SetActive(false);
                }

                if(item != null && item.veneziaItem == VeneziaItem.Pause) // Pause
                {
                    print("����!");
                    StartCoroutine(Pause_co());
                }

            }
        }
    }


    private void GameStop()
    {
        if(TimeSlider.Instance.slider.value <= 0)
        {
            ObjectPooling.Instance.gameObject.SetActive(false);
        }
    }

    private void OnDrawGizmos()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red);
    }

    IEnumerator Pause_co()
    {
        isStop = true;
        yield return new WaitForSeconds(5f);
        isStop = false;
    }
}
