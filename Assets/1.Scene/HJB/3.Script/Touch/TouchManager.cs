using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public interface ITouchEffect
{
    //UI Particle의 색상 및 특징 받기    
    public void TouchSoundCheck(bool answerCheck);

}
public class TouchManager : MonoBehaviour
{
    public static TouchManager Instance = null;
    [SerializeField] private GameObject[] effect_obj;
    [SerializeField] private GameObject beganEffect_obj;
    [SerializeField] private GameObject beganEffect_vene;
    [SerializeField] private GameObject beganEffect_gugu;
    public Canvas uiCanvas; // UI 캔버스
    //public Camera uiCamera; // UI 전용 카메라  
    ParticleSystem[] particles;

    [SerializeField] private AudioClip audioClip;
    private AudioSource source;

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
        source = GetComponent<AudioSource>();
    }
    private void Start()
    {
        particles = new ParticleSystem[effect_obj.Length];
        for (int i = 0; i < effect_obj.Length; i++)
        {
            particles[i] = effect_obj[i].transform.GetChild(0).GetComponent<ParticleSystem>();
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

            //RectTransformUtility.ScreenPointToLocalPointInRectangle(uiCanvas.GetComponent<RectTransform>(), touch.position, Camera.main, out localPoint);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (touch.phase == TouchPhase.Began)
            {
                GameObject beganTouch = TouchCheck(SceneManager.GetActiveScene().buildIndex);
                beganTouch.transform.position = touch.position;
                //hit.collider.tag == "Untagged"
                if (!Physics.Raycast(ray, out hit)&&(hit.collider == null))
                {
                    
                    source.PlayOneShot(audioClip);                    
                }                
            }            
            if (i < particles.Length)
            {
                effect_obj[i].transform.position = touch.position;
                if (touch.phase == TouchPhase.Moved)
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

    private GameObject TouchCheck(int scene_index)
    {
        switch (scene_index)
        {
            case 0:
            case 1:
            case 2:
                return Instantiate(beganEffect_obj, uiCanvas.transform);
            case 3:
                return Instantiate(beganEffect_gugu, uiCanvas.transform);
            case 4:
                return Instantiate(beganEffect_vene, uiCanvas.transform);            
            default:
                return Instantiate(beganEffect_obj, uiCanvas.transform); ;
        }        
    }
}

