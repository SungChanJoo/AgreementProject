using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ETCMethodHandler
{
    // Server -> List Index에 Finish가 있으면 제거
    public void RemoveFinish(List<string> dataList)
    {
        for(int i = 0;  i < dataList.Count; i++)
        {
            if(dataList[i].Contains("Finish"))
            {
                dataList.RemoveAt(i);
                break;
            }
        }
    }

    // Client -> string 끝에 Finish가 있으면 제거
    public void RemoveFinish(List<string> dataList, List<string> endCheck)
    {
        dataList.RemoveAt(dataList.Count - 1);
        endCheck.RemoveAt(endCheck.Count - 1);

        string fixLastIndexInList = null;

        for (int i = 0; i < endCheck.Count; i++)
        {
            fixLastIndexInList += $"{endCheck[i]}|";
        }

        // dataList와 endCheck가 Finish만 가지고 있다면 예외처리 해줘야함
        if(fixLastIndexInList == null)
        {
            return;
        }

        dataList.Add(fixLastIndexInList);
    }

    // Client -> AnalyticsData, Case에 따른 Data 처리
    public void AddClientAnalyticsDataValue(List<string> filterDataList, AnalyticsData clientAnalyticsData)
    {
        // dataList[1] = "day1|venezia_kor_level1_analytics|ReactionRate|AnswerRate|E|"
        // dataList[2] = "day1|venezia_kor_level2_analytics|ReactionRate|AnswerRate|E|"
        // dataList[3] = "day1|venezia_kor_level3_analytics|ReactionRate|AnswerRate|E|"
        // dataList[4] = "day1|venezia_eng_level1_analytics|ReactionRate|AnswerRate|E|"
        // dataList[5] = "day1|venezia_eng_level2_analytics|ReactionRate|AnswerRate|E|"
        // dataList[6] = "day1|venezia_eng_level3_analytics|ReactionRate|AnswerRate|E|"
        // dataList[7] = "day1|venezia_chn_analytics|ReactionRate|AnswerRate|E|"
        // dataList[8] = "day1|calculation_level1_anlaytics|ReactionRate|AnswerRate|E|"
        // dataList[9] = "day1|calculation_level2_anlaytics|ReactionRate|AnswerRate|E|"
        // dataList[10] = "day1|calculation_level3_anlaytics|ReactionRate|AnswerRate|E|"
        // dataList[11] = "day1|gugudan_level1_analytics|ReactionRate|AnswerRate|E|"
        // dataList[12] = "day1|gugudan_level2_analytics|ReactionRate|AnswerRate|E|"
        // dataList[13] = "day1|gugudan_level3_analytics|ReactionRate|AnswerRate|E|"

        for (int i = 1; i < filterDataList.Count; i++) // 0은 requestName
        {
            List<string> tempList = filterDataList[i].Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();

            string day = tempList[0];
            float reactionRate = float.Parse(tempList[2]);
            int answerRate = Int32.Parse(tempList[3]);
            AnalyticsData_Value analyticsData_value = new AnalyticsData_Value(day, reactionRate, answerRate);

            switch (i % 13)
            {
                case 1: // venezia_kor_level1
                    // Key : 일수,게임타입,레벨 / Value : day(날짜),reactionRate,answerRate
                    clientAnalyticsData.Data.Add(((i / 13 + 1), Game_Type.A, 1), analyticsData_value);
                    break;
                case 2: // venezia_kor_level2
                    clientAnalyticsData.Data.Add(((i / 13 + 1), Game_Type.A, 2), analyticsData_value);
                    break;
                case 3: // venezia_kor_level3
                    clientAnalyticsData.Data.Add(((i / 13 + 1), Game_Type.A, 3), analyticsData_value);
                    break;
                case 4: // venezia_eng_level1
                    clientAnalyticsData.Data.Add(((i / 13 + 1), Game_Type.B, 1), analyticsData_value);
                    break;
                case 5: // venezia_eng_level2
                    clientAnalyticsData.Data.Add(((i / 13 + 1), Game_Type.B, 2), analyticsData_value);
                    break;
                case 6: // venezia_eng_level3
                    clientAnalyticsData.Data.Add(((i / 13 + 1), Game_Type.B, 3), analyticsData_value);
                    break;
                case 7: // venezia_chn
                    clientAnalyticsData.Data.Add(((i / 13 + 1), Game_Type.C, 1), analyticsData_value);
                    break;
                case 8: // calculation_level1
                    clientAnalyticsData.Data.Add(((i / 13 + 1), Game_Type.D, 1), analyticsData_value);
                    break;
                case 9: // calculation_level2
                    clientAnalyticsData.Data.Add(((i / 13 + 1), Game_Type.D, 2), analyticsData_value);
                    break;
                case 10: // calculation_level3
                    clientAnalyticsData.Data.Add(((i / 13 + 1), Game_Type.D, 3), analyticsData_value);
                    break;
                case 11: // gugudan_level1
                    clientAnalyticsData.Data.Add(((i / 13 + 1), Game_Type.E, 1), analyticsData_value);
                    break;
                case 12: // gugudan_level2
                    clientAnalyticsData.Data.Add(((i / 13 + 1), Game_Type.E, 2), analyticsData_value);
                    break;
                case 0: // gugudan_level3
                    clientAnalyticsData.Data.Add(((i / 13 + 1), Game_Type.E, 3), analyticsData_value);
                    break;
                default:
                    //Debug.Log("[Client] Handle AnalyticsData default value come in");
                    break;
            }

            // AnalyticsData_Value analyticsData_value = new AnalyticsData_Value(day, reactionRate, answerRate);
        }

        //for (int j = 1; j < 4; j++) // 레벨
        //{
        //    string day = tempList[0];
        //    float reactionRate = float.Parse(tempList[2 + 2 * (j - 1)]);
        //    int answerRate = Int32.Parse(tempList[3 + 2 * (j - 1)]);
        //    AnalyticsData_Value analyticsData_value = new AnalyticsData_Value(day, reactionRate, answerRate);
        //    analyticsData.Data.Add(((i / 5 + 1), gameType, j), analyticsData_value);
        //
        //    if ((i % 5) == 3) break; // venezia_chn 예외처리
        //}
    }

    // DB -> CreateDB에서 Data처리
    public void AddAnalyticsColumnValueInDB(List<AnalyticsColumnValue>[][] valueList_ColumnArray_TableArray, List<List<string>>[] valuesInColumnsInTable_Array, int i)
    {
        // 중복해서 초기화되지 않도록 예외처리
        if (valueList_ColumnArray_TableArray[i / 6] == null)
        {
            valueList_ColumnArray_TableArray[i / 6] = new List<AnalyticsColumnValue>[valuesInColumnsInTable_Array[i].Count];
        }

        for (int j = 0; j < valuesInColumnsInTable_Array[i].Count; j++) // 테이블에 저장된 행수(유저와 캐릭터들)
        {
            // 중복해서 초기화되지 않도록 예외처리
            if (valueList_ColumnArray_TableArray[i / 6][j] == null)
            {
                valueList_ColumnArray_TableArray[i / 6][j] = new List<AnalyticsColumnValue>();
            }

            // 데이터가 있다면(0이 아니라면) 추가
            if (float.Parse(valuesInColumnsInTable_Array[i][j][2]) != 0 && Int32.Parse(valuesInColumnsInTable_Array[i][j][4]) != 0)
            {
                int _licenseNumber = Int32.Parse(valuesInColumnsInTable_Array[i][j][0]);
                int _charactor = Int32.Parse(valuesInColumnsInTable_Array[i][j][1]);
                float _reactionRate = float.Parse(valuesInColumnsInTable_Array[i][j][2]);
                int _answerRate = Int32.Parse(valuesInColumnsInTable_Array[i][j][4]);
                AnalyticsColumnValue data = new AnalyticsColumnValue(_licenseNumber, _charactor, _reactionRate, _answerRate);

                valueList_ColumnArray_TableArray[i / 6][j].Add(data);

                //valueList_ColumnArray_TableArray[i / 6][j].Add(new AnalyticsColumnValue(_licenseNumber, _charactor, _reactionRate, _answerRate));
            }
        }
    }



    // DB에서 테스트
    public void TestMethod(Wrapper wrapper)
    {
        wrapper.wrap = "after etcMethodHandler.TestMethod";
    }
}

public class Wrapper
{
    public string wrap;
}

