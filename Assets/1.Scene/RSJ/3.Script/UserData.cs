using System.Collections;
using System.Collections.Generic;

public class UserData
{
    // �� ������ ������ �ִ� ��� ĳ���͵��� �����͸� �����ϰ� �ҷ����� ���� Ŭ����
    public int createdCharactorCount;
    public List<UserData_Value> user;
}

public class UserData_Value
{
    public int CharactorNumber;
    public string Name;
    public byte[] Profile;
    public UserData_Value(int _charactorNumber, string _name, byte[] _profile)
    {
        CharactorNumber = _charactorNumber;
        Name = _name;
        Profile = _profile;
    }
}
