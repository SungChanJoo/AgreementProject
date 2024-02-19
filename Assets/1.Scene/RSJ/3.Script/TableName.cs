using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableName
{
    public List<string> list = new List<string>();

    // ������ - DB TableName ����
    public TableName()
    {
        list.Add("user_info");
        list.Add("rank");
        list.Add("achievement"); 
        list.Add("pet");

        // Game tablename
        string game_TableName;
        string[] game_Names = { "calculation", "venezia_chn" };
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
                        j = 2; // venezia_chn ������ level�� 1�����̹Ƿ� �ѹ��� ���ƾ���.
                        levelpart = "level";
                    }
                    game_TableName = $"{game_Names[i]}_{levelpart}_step{steps[k]}";
                    list.Add(game_TableName);
                }
            }
        }
    }
}
