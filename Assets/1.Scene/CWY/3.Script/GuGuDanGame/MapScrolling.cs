using UnityEngine;

public class MapScrolling : MonoBehaviour
{
    public Transform target;
    public float speed;
    public Vector3 dir = Vector3.left;

    public RectTransform rectTransform;

    Vector3 savePosition;
    float initialScreenWidth;

    private void Start()
    {
        savePosition = target.position;
        initialScreenWidth = Screen.width;
    }

    private void Update()
    {
        float screenWidthRatio = Screen.width / initialScreenWidth;

        transform.position += dir * speed * Time.deltaTime * screenWidthRatio;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        transform.position = new Vector3(savePosition.x-4f, savePosition.y, savePosition.z);
    }


}