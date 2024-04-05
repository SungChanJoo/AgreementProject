using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VeneGameOver : MonoBehaviour
{
    [SerializeField] private GameObject gameOverUI; // 게임오버 UI
    [SerializeField] private GameObject[] lobbyBot; // 1 bot 2 / 2 bot 2
    [SerializeField] private GameObject[] playerOneBtn; // Player Win , Lose 버튼 => 승자만 보여줄것
    [SerializeField] private GameObject[] playerTwoBtn;

    [SerializeField] GameObject Pooling;

    [SerializeField] private TextMeshProUGUI playerOneText;
    [SerializeField] private TextMeshProUGUI playerTwoText;

    //2인모드 게임종료시 승리/패배 표시해줄 애니메이션
    [Header("PlayerOneAnimation")]
    [SerializeField] private Animator playerOne_LoseAnim;
    [SerializeField] private Animator playerOne_WinAnim;
    [Header("PlayerTwoAnimation")]
    [SerializeField] private Animator playerTwo_Lose_Anim;
    [SerializeField] private Animator playerTwo_Win_Anim;

    private bool isGameover = false;

    private void Start()
    {
        playerOneText.text = "";
        playerTwoText.text = "";

        for (int i = 0; i < lobbyBot.Length; i++)
        {
            lobbyBot[i].SetActive(false);
        }

        for (int i = 0; i < 2; i++)
        {
            playerOneBtn[i].SetActive(false);
            playerTwoBtn[i].SetActive(false);
        }
        gameOverUI.SetActive(false);
    }

    private void Update()
    {
        veneCoupleGameOver();
    }

    //승패판단
    private void veneCoupleGameOver()
    {
        if(VeneziaManager.Instance.play_mode == PlayMode.Couple && !isGameover)
        {
            if (VeneziaManager.Instance.IsGameover)
            {
                gameOverUI.SetActive(true);
                Pooling.SetActive(false);
                if (TimeSlider.Instance.Slider.value <= 0)
                {
                    isGameover = true;
                    //Player 2 승
                    for (int i = 0; i < playerTwoBtn.Length; i++)
                    {
                        playerTwoBtn[i].SetActive(true);
                        playerOneText.text = "Lose";
                        playerTwoText.text = "Win";
                        // 1 2 > win lose / 3 4 win lose
                        lobbyBot[1].SetActive(true);
                        lobbyBot[2].SetActive(true);
                        playerTwo_Win_Anim.SetBool("isWin", true);
                        playerOne_LoseAnim.SetBool("isLose", true);
                    }
                }
                else if (TimeSlider.Instance.Slider_PlayerTwo.value <= 0)
                {
                    //Player1  승
                    isGameover = true;
                    for (int i = 0; i < playerOneBtn.Length; i++)
                    {
                        playerOneBtn[i].SetActive(true);
                        lobbyBot[0].SetActive(true);
                        lobbyBot[3].SetActive(true);
                        playerOneText.text = "Win";
                        playerTwoText.text = "Lose";
                        playerTwo_Lose_Anim.SetBool("isLose", true);
                        playerOne_WinAnim.SetBool("isWin", true);
                    }
                }

            }
        }
    }
}
