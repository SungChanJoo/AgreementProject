using System.Collections;
using System.Collections.Generic;

public interface IETCMethodHandler
{
    // �Ű������� �޴� �������� �����͸� ó���ϴ°��̹Ƿ� �������� ������ ������ �������� �Ѵ�. (DeepCopy X)
    void RemoveFinish(List<string> dataList, List<string> endcheck);
    void AddClientAnalyticsDataValue(List<string> filterDataList, AnalyticsData analyticsData);
    void AddAnalyticsColumnValueInDB(List<AnalyticsColumnValue>[][] valueList_ColumnArray_TableArray, List<List<string>>[] valuesInColumnsInTable_Array, int i);
}
