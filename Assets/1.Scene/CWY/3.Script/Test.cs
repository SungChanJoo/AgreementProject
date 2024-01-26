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
            // 터치된 위치로 Ray생성 (object 판별)
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            RaycastHit hit;

            // Ray가 어떤 오브젝트와 충돌했는지 확인
            if (Physics.Raycast(ray, out hit))
            {
                // 충돌한 오브젝트 삭제
                Destroy(hit.collider.gameObject);
            }
        }*/

        // 마우스 클릭 입력 감지
        if (Input.GetMouseButtonDown(0))
        {
            // 마우스 클릭된 위치로 Ray 생성 (object 판별)
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Ray가 어떤 오브젝트와 충돌했는지 확인
            if (Physics.Raycast(ray, out hit))
            {
                // 충돌한 오브젝트 삭제
                Destroy(hit.collider.gameObject);
            }

            Debug.DrawRay(ray.origin, ray.direction * 10f, Color.red, 1f);
        }
    }
}
