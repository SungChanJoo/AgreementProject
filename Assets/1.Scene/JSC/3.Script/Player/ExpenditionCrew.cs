using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpenditionCrew
{
    public int SelectedCrew; //������ ���
    public List<bool> OwnedCrew = new List<bool>(); //������ ��� ����Ʈ

    public ExpenditionCrew(int selectedCrew, List<bool> ownedCrew)
    {
        SelectedCrew = selectedCrew;
        OwnedCrew = ownedCrew;
    }
}
