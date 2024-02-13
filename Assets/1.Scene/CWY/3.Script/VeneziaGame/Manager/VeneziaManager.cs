using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

enum Quest_Korean
{
    �ð�, ���
}

public class VeneziaManager : MonoBehaviour
{
    //�������� ���ġ�� ������ �����ϱ� ���� ��ũ��Ʈ
    //ex) �ð��� 0 �϶� ������ ���߰ų�, UI������ ���ְų� ���� Ȱ��

    //�����۰��� ��ȣ�ۿ뵵 ���⿡ ����

    //���� ������ �Ŵ������� ���� , ���� ������ �̹����� �̸��� �������� ������ ������ �Ǵ� �� �� �־����.
    [SerializeField] private Sprite[] sprites;

    private Dictionary<string, Sprite> QuestKorean = new Dictionary<string, Sprite>();


    private void Start()
    {
        QuestKorean.Add("�ð�", sprites[0]);
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
                }
                ObjectPooling.Instance.cubePool.Add(hit.collider.gameObject);
                hit.collider.gameObject.SetActive(false);
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

}
