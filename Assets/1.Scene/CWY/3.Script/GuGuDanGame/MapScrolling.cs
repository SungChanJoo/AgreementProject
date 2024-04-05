using UnityEngine;

public class MapScrolling : MonoBehaviour
{
    public Transform Target;
    public float Speed;
    public Vector3 Dir = Vector3.left;

    public RectTransform RectTransform;

    Vector3 savePosition;
    float initialScreenWidth;

    private void Start()
    {
        savePosition = Target.position;
        initialScreenWidth = Screen.width;
    }

    private void Update()
    {
        float screenWidthRatio = Screen.width / initialScreenWidth;

        transform.position += Dir * Speed * Time.deltaTime * screenWidthRatio;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        transform.position = new Vector3(savePosition.x-4f, savePosition.y, savePosition.z);
    }


}