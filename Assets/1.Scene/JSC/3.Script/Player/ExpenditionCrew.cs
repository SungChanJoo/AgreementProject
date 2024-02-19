using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpenditionCrew
{
    public int SelectedCrew; //선택한 대원
    public List<bool> OwnedCrew = new List<bool>(); //보유한 대원 리스트

    public ExpenditionCrew(int selectedCrew, List<bool> ownedCrew)
    {
        SelectedCrew = selectedCrew;
        OwnedCrew = ownedCrew;
    }
}
