using System.Collections;
using System.Collections.Generic;

public interface IETCMethodHandler
{
    // 매개변수로 받는 변수들의 데이터를 처리하는것이므로 원본에도 영향이 가도록 얕은복사 한다. (DeepCopy X)
    void RemoveFinish(List<string> dataList, List<string> endcheck);
    void AddClientAnalyticsDataValue(List<string> filterDataList, AnalyticsData analyticsData);
    void AddAnalyticsColumnValueInDB(List<AnalyticsColumnValue>[][] valueList_ColumnArray_TableArray, List<List<string>>[] valuesInColumnsInTable_Array, int i);
}
