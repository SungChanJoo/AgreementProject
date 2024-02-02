using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    void Start()
    {
        
    }
    void Update()
    {
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
