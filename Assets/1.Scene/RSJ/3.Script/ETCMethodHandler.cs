using System.Collections;
using System.Collections.Generic;

public class ETCMethodHandler : IETCMethodHandler
{
    // string 끝에 Finish가 있으면 제거
    public void RemoveFinish(List<string> dataList, List<string> endCheck)
    {
        dataList.RemoveAt(dataList.Count - 1);
        endCheck.RemoveAt(endCheck.Count - 1);

        string fixLastIndexInList = null;

        for (int i = 0; i < endCheck.Count; i++)
        {
            fixLastIndexInList += $"{endCheck[i]}|";
        }

        dataList.Add(fixLastIndexInList);
    }
}
