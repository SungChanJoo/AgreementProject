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
    public Canvas UICanvas; // UI 캔버스

    [SerializeField] private GameObject[] effect_obj;
    [SerializeField] private GameObject beganEffect_obj;    
    [SerializeField] private AudioClip audioClip;
    
    ParticleSystem[] particles;

    private AudioSource source;


    /*
        파일이름 : ParticleEffectForUGUI-main
        UI Particle 라이브러리를 이용하여 3D의 Particle을 UI 화면상에 출력하도록 하였음
     */

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
        if (Input.touchCount == 0 )
        {
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].Stop();
            }
        }
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);            
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            //기본 터치 효과
            if (touch.phase == TouchPhase.Began)
            {
                GameObject beganTouch = Instantiate(beganEffect_obj, UICanvas.transform);
                beganTouch.transform.position = touch.position;                
                if (!Physics.Raycast(ray, out hit)&&(hit.collider == null))
                {                    
                    source.PlayOneShot(audioClip);                    
                }                
            }

            //길게 눌렀을 시 효과
            if (i < particles.Length)
            {
                effect_obj[i].transform.position = touch.position;
                if (touch.phase == TouchPhase.Moved&& !particles[i].isPlaying)
                {                    
                    particles[i].Play();
                    
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    //터치 입력이 끝났을 때
                    particles[i].Stop();
                }
            }
        }
    }   
    
}

