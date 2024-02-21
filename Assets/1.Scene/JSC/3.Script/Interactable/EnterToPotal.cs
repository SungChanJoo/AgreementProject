using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.UI;

public class EnterToPotal : MonoBehaviour
{
    public GameObject MoveButton;
    public Transform ExitPotal;
    public bool IsMove;
    private void Awake()
    {
        //버튼이 켜져있으면 끄기
        if (MoveButton.activeSelf)
            MoveButton.SetActive(false);
        IsMove = false;


    }
    //포탈안에 들어왔을 때
    private void OnTriggerEnter(Collider other)
    {
        //버튼이 꺼져있으면 키기
        if (!MoveButton.activeSelf)
            MoveButton.SetActive(true);

    }
    private void OnTriggerStay(Collider other)
    {
        if (!IsMove) return;
        //플레이어를 출구로 이동시키는 메서드
        if (_currentPlayerMovecoroutine == null)
        {
            _currentPlayerMovecoroutine = MovePlayer(other.gameObject);
            StartCoroutine(_currentPlayerMovecoroutine);
        }


    }
    //포탈을 나갔을 때
    private void OnTriggerExit(Collider other)
    {
        //버튼이 켜져있으면 끄기
        if (MoveButton.activeSelf)
            MoveButton.SetActive(false);
    }

    public void OnMove()
    {
        IsMove = true;
    }

    //플레이어 곡선 이동
    #region Bezier Curve
    [Header("Bezier Curve")]
    [SerializeField] private Transform[] routes;

    private float tParam; // Bezier Curve의 t매개변수
    private Vector3 playerPosition;
    public float speedModifier;

    private void Start()
    {
        tParam = 0f;
    }
    //코루틴 한번만 수행
    IEnumerator _currentPlayerMovecoroutine = null;
    //Bezier Curve에 따라 플레이어 트랜스폼 변경
    IEnumerator MovePlayer(GameObject player)
    {
        Debug.Log("MovePlayerCoroutine");
        while(tParam <1)
        {
            tParam += Time.deltaTime * speedModifier;

            playerPosition = Mathf.Pow(1 - tParam, 3) * routes[0].position +
                3 * Mathf.Pow(1 - tParam, 2) * tParam * routes[1].position +
                3 * (1 - tParam) * Mathf.Pow(tParam, 2) * routes[2].position +
                Mathf.Pow(tParam, 3) * routes[3].position;

            player.transform.position = playerPosition;
            yield return null;
        }
        tParam = 0f;
        IsMove = false;
        _currentPlayerMovecoroutine = null;
    }

    //Bezier Curve의 라인 그리기
    private Vector3 gizomsPosition;
    private void OnDrawGizmos()
    {
        for (float t = 0; t <= 1; t += 0.05f)
        {
            gizomsPosition = Mathf.Pow(1 - t, 3) * routes[0].position +
                3 * Mathf.Pow(1 - t, 2) * t * routes[1].position +
                3 * (1 - t) * Mathf.Pow(t, 2) * routes[2].position +
                Mathf.Pow(t, 3) * routes[3].position;
            Gizmos.DrawSphere(gizomsPosition, 0.25f);
        }
        Gizmos.DrawLine(new Vector3(routes[0].position.x, routes[0].position.y, routes[0].position.z),
                        new Vector3(routes[1].position.x, routes[1].position.y, routes[1].position.z));
        Gizmos.DrawLine(new Vector3(routes[2].position.x, routes[2].position.y, routes[2].position.z),
                        new Vector3(routes[3].position.x, routes[3].position.y, routes[3].position.z));
    } 

    #endregion
}
