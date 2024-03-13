using System.Collections;
using System.Collections.Generic;

public class UserData
{
    // 한 유저가 가지고 있는 모든 캐릭터들의 데이터를 저장하고 불러오기 위한 클래스
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
