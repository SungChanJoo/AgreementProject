using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
/*        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            // ��ġ�� ��ġ�� Ray���� (object �Ǻ�)
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            RaycastHit hit;

            // Ray�� � ������Ʈ�� �浹�ߴ��� Ȯ��
            if (Physics.Raycast(ray, out hit))
            {
                // �浹�� ������Ʈ ����
                Destroy(hit.collider.gameObject);
            }
        }*/

        // ���콺 Ŭ�� �Է� ����
        if (Input.GetMouseButtonDown(0))
        {
            // ���콺 Ŭ���� ��ġ�� Ray ���� (object �Ǻ�)
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Ray�� � ������Ʈ�� �浹�ߴ��� Ȯ��
            if (Physics.Raycast(ray, out hit))
            {
                // �浹�� ������Ʈ ����
                Destroy(hit.collider.gameObject);
            }

            Debug.DrawRay(ray.origin, ray.direction * 10f, Color.red, 1f);
        }
    }
}
