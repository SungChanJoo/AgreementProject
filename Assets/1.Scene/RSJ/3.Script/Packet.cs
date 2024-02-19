using System;

[Serializable]
public class Packet
{
    public string playerName;
    public int playerScore;

    // 기타 필요한 데이터나 메서드 추가

    public Packet(string name, int score)
    {
        playerName = name;
        playerScore = score;
    }
}
