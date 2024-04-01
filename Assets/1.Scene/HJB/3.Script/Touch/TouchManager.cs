using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public interface ITouchEffect
{
    //UI Particle�� ���� �� Ư¡ �ޱ�    
    public void TouchSoundCheck(bool answerCheck);

}
public class TouchManager : MonoBehaviour
{
    public static TouchManager Instance = null;
    public Canvas UICanvas; // UI ĵ����

    [SerializeField] private GameObject[] effect_obj;
    [SerializeField] private GameObject beganEffect_obj;    
    [SerializeField] private AudioClip audioClip;
    
    ParticleSystem[] particles;

    private AudioSource source;


    /*
        �����̸� : ParticleEffectForUGUI-main
        UI Particle ���̺귯���� �̿��Ͽ� 3D�� Particle�� UI ȭ��� ����ϵ��� �Ͽ���
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

            //�⺻ ��ġ ȿ��
            if (touch.phase == TouchPhase.Began)
            {
                GameObject beganTouch = Instantiate(beganEffect_obj, UICanvas.transform);
                beganTouch.transform.position = touch.position;                
                if (!Physics.Raycast(ray, out hit)&&(hit.collider == null))
                {                    
                    source.PlayOneShot(audioClip);                    
                }                
            }

            //��� ������ �� ȿ��
            if (i < particles.Length)
            {
                effect_obj[i].transform.position = touch.position;
                if (touch.phase == TouchPhase.Moved&& !particles[i].isPlaying)
                {                    
                    particles[i].Play();
                    
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    //��ġ �Է��� ������ ��
                    particles[i].Stop();
                }
            }
        }
    }   
    
}

