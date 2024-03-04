using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchManager : MonoBehaviour
{
    public static TouchManager Instance = null;
    [SerializeField] private GameObject[] effect_obj;
    [SerializeField] private GameObject beganEffect_obj;
    public Canvas uiCanvas; // UI 캔버스
    //public Camera uiCamera; // UI 전용 카메라  

    ParticleSystem[] particles;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    private void Start()
    {
        particles = new ParticleSystem[effect_obj.Length];
        for (int i = 0; i < 10; i++)
        {
            particles[i] = effect_obj[i].GetComponent<ParticleSystem>();
        }
    }
    private void Update()
    {       
        TouchEffect();
    }
    private void TouchEffect()
    {
        if (Input.touchCount ==0)
        {
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].Stop();
            }
        }
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            if (i < particles.Length)
            {
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(uiCanvas.GetComponent<RectTransform>(), touch.position, Camera.main, out localPoint);                

                effect_obj[i].transform.localPosition = localPoint;
                if (touch.phase == TouchPhase.Began)
                {                    
                    GameObject beganTouch = Instantiate(beganEffect_obj, uiCanvas.transform);
                    beganTouch.transform.position = touch.position;
                }
                else if (touch.phase == TouchPhase.Moved)
                {
                    
                    if (!particles[i].isPlaying)
                    {
                        particles[i].Play();
                    }
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    particles[i].Stop();
                }
            }
        }
    }   
}

