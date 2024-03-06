using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableName
{
    public List<string> list = new List<string>();

    // 생성자 - DB TableName 세팅
    public TableName()
    {
        list.Add("user_info");
        list.Add("rank");
        list.Add("achievement"); 
        list.Add("pet");

        // Game tablename
        string game_TableName;
        string[] game_Names = { "venezia_kor", "venezia_eng", "venezia_chn", "calculation", "gugudan" };
        int[] levels = { 1, 2, 3 };
        int[] steps = { 1, 2, 3, 4, 5, 6 };

        for (int i = 0; i < game_Names.Length; i++)
        {
            for (int j = 0; j < levels.Length; j++)
            {
                for (int k = 0; k < steps.Length; k++)
                {
                    string levelpart = $"level{levels[j]}";
                    if (game_Names[i] == "venezia_chn")
                    {
                        j = 2; // venezia_chn 게임은 level이 1개뿐이므로 한번만 돌아야함.
                        levelpart = "level";
                    }
                    game_TableName = $"{game_Names[i]}_{levelpart}_step{steps[k]}";
                    list.Add(game_TableName);
                }
            }
        }
    }
}

public class AnalyticsTableName
{
    public List<string> list = new List<string>();

    public AnalyticsTableName()
    {
        for(int i = 0; i < 13; i++)
        {
            if (i < 3) // venezia_kor
            {
                list.Add($"venezia_kor_level{i + 1}_anlaytics");
            }
            else if (i < 6) // venezia_eng
            {
                list.Add($"venezia_eng_level{i - 2}_anlaytics");
            }
            else if (i == 6) // venezia_chn
            {
                list.Add($"venezia_chn_anlaytics");
            }
            else if (i < 10) // calculation
            {
                list.Add($"calculation_level{i - 6}_anlaytics");
            }
            else // gugudan
            {
                list.Add($"gugudan_level{i - 9}_anlaytics");
            }
        }
    }
}

