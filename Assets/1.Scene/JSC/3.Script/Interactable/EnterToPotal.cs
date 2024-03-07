using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class EnterToPotal : NetworkBehaviour
{
    public GameObject MoveButton;
    public ObjInteractor player;
    public Transform ExitPotal;
    public bool IsMove;
    public GameObject InteractableParticle;

    private void Awake()
    {
        //��ư�� ���������� ����
        if (MoveButton.activeSelf)
            MoveButton.SetActive(false);
        IsMove = false;


    }
    //��Ż�ȿ� ������ ��
    private void OnTriggerEnter(Collider other)
    {
/*        //��ư�� ���������� Ű��
        if (!MoveButton.activeSelf)
            MoveButton.SetActive(true);*/
        player = other.GetComponent<ObjInteractor>();
        if (player != null)
            player.InteractableUI(MoveButton, true);//��ư�� ���������� Ű��

    }
    private void OnTriggerStay(Collider other)
    {
        if (!IsMove) return;
        //�÷��̾ �ⱸ�� �̵���Ű�� �޼���
        if (_currentPlayerMovecoroutine == null)
        {
            _currentPlayerMovecoroutine = MovePlayer(other.gameObject);
            StartCoroutine(_currentPlayerMovecoroutine);
        }


    }
    //��Ż�� ������ ��
    private void OnTriggerExit(Collider other)
    {
/*        //��ư�� ���������� ����
        if (MoveButton.activeSelf)
            MoveButton.SetActive(false);*/
        if (player != null)
            player.InteractableUI(MoveButton, false);//��ư�� ���������� ����
        player = null; //�ʱ�ȭ
    }

    public void OnMove()
    {
        IsMove = true;
    }

    //�÷��̾� � �̵�
    #region Bezier Curve
    [Header("Bezier Curve")]
    [SerializeField] private Transform[] routes;

    private float tParam; // Bezier Curve�� t�Ű�����
    private Vector3 playerPosition;
    public float speedModifier;

    private void Start()
    {
        tParam = 0f;
    }
    //�ڷ�ƾ �ѹ��� ����
    IEnumerator _currentPlayerMovecoroutine = null;
    //Bezier Curve�� ���� �÷��̾� Ʈ������ ����
    IEnumerator MovePlayer(GameObject player)
    {
        Debug.Log("MovePlayerCoroutine");
        CmdPlayerJumpAnim(player);
        while (tParam <1)
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
        CmdPlayerJumpEndAnim(player);
        _currentPlayerMovecoroutine = null;
    }

    [Command(requiresAuthority = false)]
    public void CmdPlayerJumpAnim(GameObject player)
    {
        RpcPlayerJumpAnim(player);
    }
    [ClientRpc]
    public void RpcPlayerJumpAnim(GameObject player)
    {
        var playerAnim = player.GetComponent<PlayerMovement>().Anim;
        playerAnim.SetTrigger("Jump");
        playerAnim.SetBool("JumpEnd", false);
    }
    [Command(requiresAuthority = false)]
    public void CmdPlayerJumpEndAnim(GameObject player)
    {
        RpcPlayerJumpEndAnim(player);
    }
    [ClientRpc]
    public void RpcPlayerJumpEndAnim(GameObject player)
    {
        var playerAnim = player.GetComponent<PlayerMovement>().Anim;
        playerAnim.SetBool("JumpEnd", true);
    }
    //Bezier Curve�� ���� �׸���
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
