using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchManager : MonoBehaviour
{
        
    [SerializeField] private GameObject effect_obj;
    [SerializeField] private Camera mainCamera;

    private bool isDragging = false;
    private Vector3 offset;
    
    private void Update()
    {
        TouchEvent();
    }
    //private void TouchEvent()
    //{
    //    if (Input.GetMouseButtonDown(0))
    //    {
    //        Vector3 clickPos = Input.mousePosition;            

    //        effect_obj.transform.position = clickPos;
    //    }
    //}    
    private void TouchEvent()
    {

        if(Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector3 touchPosition = touch.position;

            if(touch.phase == TouchPhase.Began)
            {
                RaycastHit hit;
                Ray ray = mainCamera.ScreenPointToRay(touchPosition);

                if(Physics.Raycast(ray, out hit))
                {
                    if(hit.collider.gameObject == effect_obj)
                    {
                        isDragging = true;
                        offset = effect_obj.transform.position - mainCamera.ScreenToWorldPoint(new Vector3(touchPosition.x, touchPosition.y, effect_obj.transform.position.z));
                    }
                }
            }
            else if(touch.phase == TouchPhase.Moved && isDragging)
            {
                Vector3 newPosition = mainCamera.ScreenToWorldPoint(new Vector3(touchPosition.x, touchPosition.y, effect_obj.transform.position.z)) + offset;
                effect_obj.transform.position = newPosition;
            }
            else if(touch.phase == TouchPhase.Ended && isDragging)
            {
                isDragging = false;
            }
        }
    }
}
