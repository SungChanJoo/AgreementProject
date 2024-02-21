using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

enum ButtonType
{
    First,
    Second
}

public class GuGUDan_Fnc : MonoBehaviour
{
    [SerializeField] private ButtonType buttonType;
    //±¸±¸´Ü °ÔÀÓ ±â´É °ü·Ã ½ºÅ©¸³Æ®
    //°ö¼À ¼ıÀÚ 1¹ø,2¹ø¿¡ ·£´ıÀ¸·Î ¼ıÀÚ°¡ ¹ß»ıÇØ¼­ ±¸±¸´ÜÀ» ÁøÇà 
    //°á°ú´Â ¸ÂÃß´ø ¸ø¸ÂÃß´ø °ø°³ÇÏ°í ¸ÂÃâ°æ¿ì O , Æ²¸±°æ¿ì X Ç¥½Ã?
    //Á¤´äÀº ¹öÆ° 1 ~ 9 ¸¦ ´­·¯¼­ ÀÔ·Â 

    //º¯°æ»çÇ× 
    //clear ¹öÆ° Ãß°¡ ¹× 0~9¹öÆ°À» Á¦¿ÜÇÑ ¿µ¿ªÀ» ´­·¶À»¶§µµ clear È¿°ú ¹ß»ıÇØ¾ßÇÔ
    //xyz ¿¡¼­ z°ª(Á¤´ä)ÀÌ °íÁ¤ÀÌ ¾Æ´Ï¶ó , ¿ª»ê±â´Éµµ ³Ö¾î¼­ x,yµµ ·£´ıÇÏ°Ô Á¤´äÀÏ °æ¿ì°¡ ÀÖ¾î¾ßÇÔ
    //2~9´ÜÀÌ ¾Æ´Ï¶ó 2~ 19´Ü±îÁö 
    //level & step ´Ü°è Ãß°¡
    //¿À´ä Ã³¸®½Ã ½Ã°£°¨¼Ò

    [SerializeField] private TextMeshProUGUI First_num;  // Ã¹¹øÂ° Ä­¿¡ µé¾î°¥ ¼ıÀÚ
    [SerializeField] private TextMeshProUGUI Second_num; // µÎ¹øÂ° Ä­¿¡ µé¾î°¥ ¼ıÀÚ.
    [SerializeField] private TextMeshProUGUI Answer_num; // Á¤´äÀ» ÀÔ·Â¹ŞÀ» Ä­ 

    [SerializeField] private Button[] buttons; // 0~9 ¹öÆ° 

    [SerializeField] Canvas canvas;

    #region º¯¼ö & »óÅÂ °ü¸®
    public int Level;
    public int Step;

    //°ÔÀÓ ÁøÇà ¼ø¼­ 
    bool isStart = false;
    bool isFirst_Click = true;
    bool isSecond_Click = false;
    bool isThird_Click = false;
    bool isGameOver = false;

    //Á¤´äÀÇ ÀÚ¸´¼ö È®ÀÎ
    int FirstNumDigit;
    int SecondNumDigit;
    int AnswerNumDigit;
    //Á¤´ä ÀÚ¸´¼ö¿¡ ¸Â°Ô ÀÔ·ÂÇß´ÂÁö È®ÀÎÇÏ´Â ÀåÄ¡
    int Click_Count;

    //È®ÀÎ¹öÆ°À» ´­·¶´ÂÁö ¾È´­·¶´ÂÁö È®ÀÎ
    bool isAnswerCheck = false;
    //È®ÀÎ¹öÆ°À» ´©¸¥°ÍÀÌ Á¤´äÀÎÁö ¾Æ´ÑÁö È®ÀÎ
    bool isAnswerCorrect = false;

    //Á¤´ä ¸ÂÃá °¹¼ö
    private int TrueAnswerCount = 0;
    //¹®Á¦ ÃâÁ¦ ÈÄ ¹İÀÀ¼Óµµ ½Ã°£ Àç±â Á¤´ä¸¸
    float trueReactionTime = 0f; //Á¤´äÀ» ¸ÂÃèÀ»¶§ÀÇ ¹İÀÀ¼Óµµ
    float totalReactionTime = 0f;
    string buttonText;

    //½ºÅÇº° ÄÉÀÌ½º ¼±ÅÃÁö ¹øÈ£ È®ÀÎ
    int CaseNum;
    #endregion

    private void Awake()
    {
        First_num.text = "0";
        Second_num.text = "";
        Answer_num.text = "??";
        buttonText = "";
        
    }


    private void Start()
    {
        GameStart();
    }

    private void Update()
    {
        if (!isGameOver)
        {
            Reaction_speed();
        }

        GameOver();

        Click();

        print(Click_Count);
    }


    //·£´ı¼ıÀÚ »ı¼º ¸Ş¼­µå
    public int Random_Num()
    {
        int num = Random.Range(1, 10);
        return num;
    }

    //Ç¥±âÇØ ÁÖ´Â À§Ä¡¸¦ ·£´ıÀ¸·Î Ç¥½Ã ÇØÁÙ ¸Ş¼­µå
    // ½ºÅÇ 1 ~ 3  / 4 ~ 6 ¿¡¼­ÀÇ ³­ÀÌµµ´Â µ¿ÀÏÇÔ
    private void Random_ShowText()
    {
        //¹®Á¦ »ı¼º => x , y¸¦ »ı¼ºÇÑÈÄ °ö¼À 
        //´Ü , UI¿¡ Ç¥±âÇÏ´Â °ÍÀº ·£´ı * ¿ª»ê ½Ã½ºÅÛÀÌ ÀÖ¾î¾ßÇÔ ÇØ´ç ¸Ş¼­µå´Â Ç¥±â ¼ø¼­¸¸ º¯°æ
        // 1. x °¡ ºñ¾îÀÖÀ» °æ¿ì ?? x Y = z  
        // 2. Y°¡ ºñ¾îÀÖÀ» °æ¿ì X x ?? = z
        // 3. z°¡ ºñ¾îÀÖÀ» °æ¿ì X x Y = ??
        int First = int.Parse(First_num.text);
        int Second = int.Parse(Second_num.text);
        int result = int.Parse(First_num.text) * int.Parse(Second_num.text);

        //Á¤´äÀÌ µÉ ¼ıÀÚ À§Ä¡ ·£´ıÀ¸·Î ¼±ÅÃ.
        switch (Random.Range(0, 3))
        {
            case 0: // x ÂÊÀ» ¿ª»ê
                First_num.text = "?";
                Second_num.text = Second.ToString();
                Answer_num.text = result.ToString();
                CaseNum = 0;
                break;
            case 1: // yÂÊÀ» ¿ª»ê
                First_num.text = First.ToString();
                Second_num.text = "?";
                Answer_num.text = result.ToString();
                CaseNum = 1;
                break;
            case 2: // ±âº» ¿¬»ê
                First_num.text = First.ToString();
                Second_num.text = Second.ToString();
                Answer_num.text = "??";
                CaseNum = 2;
                break;
            default:
                break;
        }
    }
    #region Level , Stepº° ¹®Á¦ Á¦ÀÛ ¸Ş¼­µå 
    private void Lv1_RandomNum()
    {
        if (Step < 4)
        {
            First_num.text = Random.Range(2, 7).ToString();
            Second_num.text = Random.Range(1, 10).ToString();
        }
        else
        {
            First_num.text = Random.Range(2, 10).ToString();
            Second_num.text = Random.Range(1, 10).ToString();
        }
    }
    private void Lv2_RandomNum()
    {
        if (Step < 4)
        {
            First_num.text = Random.Range(2, 10).ToString();
            Second_num.text = Random.Range(1, 15).ToString();
        }
        else
        {
            First_num.text = Random.Range(2, 10).ToString();
            Second_num.text = Random.Range(1, 20).ToString();
        }
    }
    private void Lv3_RandomNum()
    {
        if (Step < 4)
        {
            First_num.text = Random.Range(2, 15).ToString();
            Second_num.text = Random.Range(1, 20).ToString();
        }
        else
        {
            First_num.text = Random.Range(2, 20).ToString();
            Second_num.text = Random.Range(1, 20).ToString();
        }
    }
    #region lvº° °ÔÀÓ
    //lvº° ·ÎÁ÷
    private void Lv1()
    {
        Lv1_RandomNum();
        Random_ShowText();
    }
    private void Lv2()
    {
        Lv2_RandomNum();
        Random_ShowText();
    }
    private void Lv3()
    {
        Lv3_RandomNum();
        Random_ShowText();
    }
    #endregion
    #endregion
    //

    //°ÔÀÓ ½ÃÀÛ½Ã ·£´ı ¼ıÀÚ ºÎ¿©
    //¼±ÅÃ Level , Step¿¡ µû¶ó ¼ıÀÚÀÇ ¹üÀ§ Á¦ÇÑ
    private void GameStart()
    {
        //°ÔÀÓ ½ÃÀÛÀ» ¾Ë¸²
        isStart = true;
        switch (Level)
        {
            case 1: //·¹º§ 1 ¼±ÅÃ
                Lv1();
                break;
            case 2: //·¹º§ 2 ¼±ÅÃ
                Lv2();
                break;
            case 3: //·¹º§ 3 ¼±ÅÃ
                Lv3();

                break;
            default:
                break;
        }
    }
    //·ÎÁ÷Àº µ¿ÀÏÇÏÁö¸¸ ½ÃÀÛ - ÁøÇà - Á¾·á¸¦ ³ª´©±âÀ§ÇØ ¸Ş¼­µå ºĞ°³
    private void GameProgress()
    {
        //Á¤´äÀ» ¸ÂÃá°æ¿ì¿¡´Â ¹®Á¦ ´Ù½Ã»ı¼º
        if (isAnswerCorrect)
        {
            switch (Level)
            {
                case 1:
                    Lv1();
                    break;
                case 2:
                    Lv2();
                    break;
                case 3:
                    Lv3();
                    break;
                default:
                    break;
            }
        }
        //Å¬¸¯ ÃÊ±âÈ­
        isFirst_Click = true;

        //Á¤´äÈ®ÀÎ ÃÊ±âÈ­
        isAnswerCheck = false;
    }

    private void GameOver()
    {
        //Å¸ÀÓ½½¶óÀÌ´õÀÇ Value °ªÀÌ 0 ÀÏ°æ¿ì °ÔÀÓ ³¡.
        if(TimeSlider.Instance.slider.value == 0)
        {
            //Todo : °ÔÀÓ Á¾·á ·ÎÁ÷ ±¸ÇöÇØÁÖ¼¼¿ä > °ÔÀÓÈ­¸é Á¾·áÇÏ°í °á°úÃ¢? ¶ç¿öÁÙµí
            //¹İÀÀ¼Óµµ «n
            print(trueReactionTime / TrueAnswerCount);
            isGameOver = true;
            totalReactionTime = 0;
        }
        else
        {
            return;
        }  
    }

    //Á¤´ä ÆÇ´Ü ÇÔ¼ö
    public void AnswerCheck()
    {
        if (TimeSlider.Instance.slider.value == 0) return; // Å¸ÀÓ¿À¹ö½Ã ¸®ÅÏ
        //Á¤´äÀÏ°æ¿ì
        int resultx = int.Parse(Answer_num.text) / int.Parse(Second_num.text);
        int resulty = int.Parse(Answer_num.text) / int.Parse(First_num.text);
        int resultz = int.Parse(First_num.text) * int.Parse(Second_num.text);
        if (CaseNum == 0) //x ÂÊ ¿ª»ê
        {          
            if (First_num.text == $"{resultx}")
            {
                Get_Score();
                TrueAnswerCount++; //Á¤´ä °¹¼ö Áõ°¡ -> Á¤´ä·ü ¹İ¿µ¿¡ »ç¿ëÇÒ°Å
                isAnswerCheck = true;
                isAnswerCorrect = true;
                //´©Àû½Ã°£ º¯¼ö¿¡ ÀúÀå.
            }
            else //¿À´äÀÏ°æ¿ì
            {
                //Todo: ±âÈ¹ÆÀ¿¡¼­ ¾î¶»°Ô ÇÒ °èÈ¹ÀÎÁö ¸»ÇØÁÖ¸é ±×³É ¸®ÅÏÇÏ´øÁö
                // ¿À´äÀÏ°æ¿ì Á¦ÇÑ½Ã°£°¨¼Ò 
                //´©ÀûµÈ ½Ã°£ ³¯¸®±â
                isAnswerCheck = true;
                isAnswerCorrect = false;
                TimeSlider.Instance.DecreaseTime_Item(5);
            }
        }
        else if(CaseNum == 1) // yÂÊÀ» ¿ª»ê
        {
            if (Second_num.text == $"{resulty}")
            {
                Get_Score();
                TrueAnswerCount++; //Á¤´ä °¹¼ö Áõ°¡ -> Á¤´ä·ü ¹İ¿µ¿¡ »ç¿ëÇÒ°Å
                isAnswerCheck = true;
                isAnswerCorrect = true;
                //´©Àû½Ã°£ º¯¼ö¿¡ ÀúÀå.
            }
            else //¿À´äÀÏ°æ¿ì
            {
                //Todo: ±âÈ¹ÆÀ¿¡¼­ ¾î¶»°Ô ÇÒ °èÈ¹ÀÎÁö ¸»ÇØÁÖ¸é ±×³É ¸®ÅÏÇÏ´øÁö
                // ¿À´äÀÏ°æ¿ì Á¦ÇÑ½Ã°£°¨¼Ò 
                //´©ÀûµÈ ½Ã°£ ³¯¸®±â
                isAnswerCheck = true;
                isAnswerCorrect = false;
                TimeSlider.Instance.DecreaseTime_Item(5);
            }
        }
        else if (CaseNum == 2)
        {
            if (Answer_num.text == $"{resultz}")
            {
                Get_Score();
                TrueAnswerCount++; //Á¤´ä °¹¼ö Áõ°¡ -> Á¤´ä·ü ¹İ¿µ¿¡ »ç¿ëÇÒ°Å
                isAnswerCheck = true;
                isAnswerCorrect = true;
                //´©Àû½Ã°£ º¯¼ö¿¡ ÀúÀå.
            }
            else //¿À´äÀÏ°æ¿ì
            {
                //Todo: ±âÈ¹ÆÀ¿¡¼­ ¾î¶»°Ô ÇÒ °èÈ¹ÀÎÁö ¸»ÇØÁÖ¸é ±×³É ¸®ÅÏÇÏ´øÁö
                // ¿À´äÀÏ°æ¿ì Á¦ÇÑ½Ã°£°¨¼Ò 
                //´©ÀûµÈ ½Ã°£ ³¯¸®±â
                isAnswerCheck = true;
                isAnswerCorrect = false;
                TimeSlider.Instance.DecreaseTime_Item(5);
            }
        }

        //ÃÊ±âÈ­ ÇÊ¿ä
        Clear_btn();
        
        if (!isGameOver) GameProgress();
    }

    //numÆÄ¶ó¹ÌÅÍ¸¦ ¹Ş¾Æ¼­ ÀÔ·ÂÇÑ ¼ıÀÚ ÆÇÁ¤ÇÏ´Â ¸Ş¼­µå
    public void Clicked_NumberBtn(int num)
    {
        //Todo : ½º¿ÒµÈ À§Ä¡¸¦ ±âÁØÀ¸·Î Á¡¼ö¸¦ ÆÇÁ¤ ÇÒ ¼ö ÀÖµµ·Ï ·ÎÁ÷À» º¯°æÇØ¾ßÇÔ
        if (isGameOver) return; //°ÔÀÓ Á¾·á½Ã ÀÔ·Â ¹æÁö
        Check_AnswerNumDigit(); // Ã³À½ Å¬¸¯ ÇßÀ» ¶§ 

        switch (CaseNum)
        {
            case 0: //x Ä­ÀÌ ?? ·Î Ãâ·ÂµÇ´Â °æ¿ì Å¬¸¯½Ã xÂÊ¿¡ ´äÀÌ ÀÔ·ÂµÇ°í, ÀÚ¸´¼ö¿¡ ¸Â°Ô ´äÀ» ÀÔ·Â ÇØ¾ß ÇÔ.
                    //19´Ü ±îÁö¸¸ °í·ÁÇÏ±â ¶§¹®¿¡ x ,y ´Â ÃÖ´ë 2ÀÚ¸®  z´Â 3ÀÚ¸®±îÁö Ã¼Å©. µû¶ó¼­, ÀÔ·Âµµ ÃÖ´ë 3¹ø(19x19°¡ ÃÖ´ë 3ÀÚ¸®)
                if (isFirst_Click)
                {
                    First_num.text = num.ToString();
                    isFirst_Click = false;
                    isSecond_Click = true;
                    Click_Count++;
                }
                else if (isSecond_Click)
                {
                    First_num.text += num.ToString();
                    isSecond_Click = false;
                    Click_Count++;
                }
                break;
            case 1:
                if (isFirst_Click)
                {
                    Second_num.text = num.ToString();
                    isFirst_Click = false;
                    isSecond_Click = true;
                    Click_Count++;
                }
                else if (isSecond_Click)
                {
                    Second_num.text += num.ToString();
                    isSecond_Click = false;
                    Click_Count++;
                }
                break;
            case 2:
                if (isFirst_Click)
                {
                    Answer_num.text = num.ToString();
                    isFirst_Click = false;
                    isSecond_Click = true;
                    Click_Count++;
                }
                else if (isSecond_Click)
                { 
                    Answer_num.text += num.ToString();
                    isSecond_Click = false;
                    isThird_Click = true;
                    Click_Count++;
                }
                else if (isThird_Click)
                {
                    Answer_num.text += num.ToString();
                    isThird_Click = false;
                    Click_Count++;
                }
                break;
            default:
                break;
        }


        //Á¤´äÀÇ ÀÚ¸´¼ö¿Í Å¬¸¯ÀÇ È½¼ö°¡ ÀÏÄ¡ÇÏ¸é Á¤´äÃ¤Å© ÈÄ Å¬¸¯ Ä«¿îÆ® ÃÊ±âÈ­
        switch (CaseNum)
        {
            case 0:
                if (FirstNumDigit == Click_Count) AnswerCheck();
                break;
            case 1:
                if (SecondNumDigit == Click_Count) AnswerCheck();
                break;
            case 2:
                if (AnswerNumDigit == Click_Count)  AnswerCheck();
                break;
            default:
                break;
        }

    }

    private void Check_AnswerNumDigit()
    {
        //¹®Á¦°¡ ÃâÁ¦ µÇ¾úÀ»¶§ Á¤´äÀÇ ÀÚ¸´¼ö¸¦ ¹Ş¾Æ¿À°í
        //Á¤´äÀÌ 1ÀÚ¸®¶ó¸é 1¹ø Å¬¸¯ , 2ÀÚ¸®¶ó¸é 2¹øÅ¬¸¯ 3ÀÚ¸®¶ó¸é 3¹øÅ¬¸¯ÇÑ °ªÀ» ÀÌ¿ëÇÒ°Í.
        //¹®Á¦ÀÇ ÀÚ¸®°¡ x y z ÀÏ °æ¿ì¸¦ °í·ÁÇØ¾ßÇÔ
        switch (CaseNum)
        {
            case 0: //x¿¡ µé¾î°¡´Â ÃÖ´ë ÀÚ¸´¼ö
                int firstnum = int.Parse(Answer_num.text) / int.Parse(Second_num.text);
                FirstNumDigit = firstnum.ToString().Length;
                break;
            case 1: //y¿¡ µé¾î°¡´Â ÃÖ´ë ÀÚ¸´¼ö
                int secondnum = int.Parse(Answer_num.text) / int.Parse(First_num.text);
                SecondNumDigit = secondnum.ToString().Length;
                break;
            case 2: //z¿¡ µé¾î°¡´Â ÃÖ´ë ÀÚ¸´¼ö
                int AnswerNum = int.Parse(First_num.text) * int.Parse(Second_num.text);
                AnswerNumDigit = AnswerNum.ToString().Length;
                break;

            default:
                break;
        }
    }

    public void Clear_btn()
    {
        //Todo : 2ÀÎ¸ğµå½Ã Å¬¸®¾î¹öÆ° ÃÊ±âÈ­ À§Ä¡ ÁöÁ¤ ÇÊ¿ä.
        switch (CaseNum)
        {
            case 0: First_num.text = "?";
                    break;
            case 1:
                Second_num.text = "?";
                break;
            case 2:
                Answer_num.text = "??";
                break;
            default:
                break;
        }
        isFirst_Click = true;
        isSecond_Click = false;
        isThird_Click = false;
        Click_Count = 0;
    }




    public void Reaction_speed()
    {
        //Á¤´äÀ» ¸Â­ŸÀ» ¶§¿¡¸¸ 
        totalReactionTime += Time.deltaTime;
        if (isAnswerCorrect)
        {
            trueReactionTime += totalReactionTime;
        }
        totalReactionTime = 0;
    }

    //¹öÆ°ÀÌ ¾Æ´Ñ ´Ù¸¥ ¿µ¿ªÀ» Å¬¸¯ÇÏ¸é ÃÊ±âÈ­ ½ÃÄÑÁÖ±â
    private void Click()
    {

        if (Input.GetMouseButtonDown(0))
        {
            //ÇöÀç °ÔÀÓÈ­¸é¿¡ ¸¶¿ì½º(¼Õ°¡¶ôÅÍÄ¡)¸¦ ÀÔ·Â½Ã ±× ÁÂÇ¥¸¦ °è»ê
            Vector2 mousePosition = Input.mousePosition;
            RectTransform canvasRectTransform = canvas.GetComponent<RectTransform>(); // YourCanvas¿¡´Â Äµ¹ö½º GameObject¸¦ ÇÒ´ç
            Vector2 canvasSize = canvasRectTransform.sizeDelta;
            Vector2 canvasPosition = canvasRectTransform.position;
            Vector2 canvasMousePosition = new Vector2(mousePosition.x / Screen.width * canvasSize.x, mousePosition.y / Screen.height * canvasSize.y) - (canvasSize / 2f);
            //print(mousePosition.y / Screen.height * canvasSize.y);
            //ÀÌ ¿µ¿ª À§·Î Å¬¸¯½Ã Clear
            //if (mousePosition.y / Screen.height * canvasSize.y > 255f) Clear_btn();
        }
    }

    public void Get_Score()
    {
        if(buttonType == ButtonType.First)
        {
            Score.Instance.Get_FirstScore();
        }
        else
        {
            Score.Instance.Get_SecondScore();
        }
    }
}
