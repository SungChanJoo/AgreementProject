using System;

[Serializable]
public class Packet
{
    public string playerName;
    public int playerScore;

    // ��Ÿ �ʿ��� �����ͳ� �޼��� �߰�

    public Packet(string name, int score)
    {
        playerName = name;
        playerScore = score;
    }
}
