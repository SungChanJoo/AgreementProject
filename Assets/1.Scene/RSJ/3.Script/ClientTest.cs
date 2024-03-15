using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ClientTest : MonoBehaviour
{
    private UserData testUserData;
    private Player_DB testPlayer_DB;
    private AnalyticsData testAnalyticsData;
    private RankData testRankData;
    private ExpenditionCrew testExpenditionCrew;
    private LastPlayData testLastPlayData;
    private AnalyticsProfileData testAnalyticsProfileData;

    public void OnClickClientRequestLoadDataToDB()
    {
        TestLoadUserData();
    }

    #region TestLoad
    private void TestLoadUserData()
    {
        testUserData = Client.instance.AppStart_LoadUserDataFromDB();
        Debug.Log($"[ClientTest] use Client.instance.AppStart_LoadUserDataFromDB(), Current time : {System.DateTime.Now}");
        TestLoadPlayerDB();
    }

    private void TestLoadPlayerDB()
    {
        testPlayer_DB = Client.instance.AppStart_LoadAllDataFromDB();
        Debug.Log($"[ClientTest] use Client.instance.AppStart_LoadAllDataFromDB(), Current time : {System.DateTime.Now}");
        TestLoadAnalyticsData();
    }

    private void TestLoadAnalyticsData()
    {
        testAnalyticsData = Client.instance.AppStart_LoadAnalyticsDataFromDB();
        Debug.Log($"[ClientTest] use Client.instance.AppStart_LoadAnalyticsDataFromDB(), Current time : {System.DateTime.Now}");
        TestLoadRankData();
    }

    private void TestLoadRankData()
    {
        testRankData = Client.instance.AppStart_LoadRankDataFromDB();
        Debug.Log($"[ClientTest] use Client.instance.AppStart_LoadRankDataFromDB(), Current time : {System.DateTime.Now}");
        TestLoadExpenditionCrew();
    }

    private void TestLoadExpenditionCrew()
    {
        testExpenditionCrew = Client.instance.AppStart_LoadExpenditionCrewFromDB();
        Debug.Log($"[ClientTest] use Client.instance.AppStart_LoadExpenditionCrewFromDB(), Current time : {System.DateTime.Now}");
        TestLoadLastPlayData();
    }

    private void TestLoadLastPlayData()
    {
        testLastPlayData = Client.instance.AppStart_LoadLastPlayFromDB();
        Debug.Log($"[ClientTest] use Client.instance.AppStart_LoadLastPlayFromDB(), Current time : {System.DateTime.Now}");
        TestLoadAnalyticsProfileData();
    }

    private void TestLoadAnalyticsProfileData()
    {
        testAnalyticsProfileData = Client.instance.AppStart_LoadAnalyticsProfileDataFromDB();
        Debug.Log($"[ClientTest] use Client.instance.AppStart_LoadAnalyticsProfileDataFromDB(), Current time : {System.DateTime.Now}");

        Debug.Log("[ClientTest] Complete Load All Datas From DB!!");
    }
    #endregion

    public void OnClickCheckDatas()
    {
        // UserData
        Debug.Log($"[UserData] Charactor Count : {testUserData.createdCharactorCount}");
        for (int i = 0; i < testUserData.user.Count; i++)
        {
            Debug.Log($"[UserData] {i}'th CharactorNumber : {testUserData.user[i].CharactorNumber}");
            Debug.Log($"[UserData] {i}'th CharactorName : {testUserData.user[i].Name}");
            Debug.Log($"[UserData] {i}'th CharactorProfile : {testUserData.user[i].Profile}");
        }

        // Player_DB
        Debug.Log($"[PlayerDB] playerName : {testPlayer_DB.playerName}");
        Debug.Log($"[PlayerDB] image : {testPlayer_DB.image}");
        Debug.Log($"[PlayerDB] day : {testPlayer_DB.Day}");
        Debug.Log($"[PlayerDB] BirthDay : {testPlayer_DB.BirthDay}");
        Debug.Log($"[PlayerDB] TotalAnswers : {testPlayer_DB.TotalAnswers}");
        Debug.Log($"[PlayerDB] TotalTime : {testPlayer_DB.TotalTime}");

        int count = 0;
        ICollection<Data_value> allvalues = testPlayer_DB.Data.Values;
        foreach (Data_value datavalue in allvalues)
        {
            count++;
            Debug.Log($"[PlayerDB] count : {count}, ReactionRate : {datavalue.ReactionRate}");
            Debug.Log($"[PlayerDB] count : {count}, AnswersCount : {datavalue.AnswersCount}");
            Debug.Log($"[PlayerDB] count : {count}, Answers : {datavalue.Answers}");
            Debug.Log($"[PlayerDB] count : {count}, PlayTime : {datavalue.PlayTime}");
            Debug.Log($"[PlayerDB] count : {count}, TotlaScore : {datavalue.TotalScore}");
            Debug.Log($"[PlayerDB] count : {count}, StarCount : {datavalue.StarCount}");
        }

        // AnalyticsData
        // 가장 최근날짜, Venezia_kor, Level1
        Debug.Log($"[AnalyticsData] testAnalyticsData.Data[(1, Game_Type.A, 1)].day : {testAnalyticsData.Data[(1, Game_Type.A, 1)].day}");
        Debug.Log($"[AnalyticsData] testAnalyticsData.Data[(1, Game_Type.A, 1)].reactionRate : {testAnalyticsData.Data[(1, Game_Type.A, 1)].reactionRate}");
        Debug.Log($"[AnalyticsData] testAnalyticsData.Data[(1, Game_Type.A, 1)].answerRate : {testAnalyticsData.Data[(1, Game_Type.A, 1)].answerRate}");

        // RankData
        try
        {
            Debug.Log($"[RankData] testRankData.rankdata_score[0].userProfile : {testRankData.rankdata_score[0].userProfile}");
            Debug.Log($"[RankData] testRankData.rankdata_score[0].userName : {testRankData.rankdata_score[0].userName}");
            Debug.Log($"[RankData] testRankData.rankdata_score[0].totalScore : {testRankData.rankdata_score[0].totalScore}");
            Debug.Log($"[RankData] Client testRankData.rankdata_score[5].userProfile : {testRankData.rankdata_score[5].userProfile}");
            Debug.Log($"[RankData] Client testRankData.rankdata_score[5].userName : {testRankData.rankdata_score[5].userName}");
            Debug.Log($"[RankData] Client testRankData.rankdata_score[5].totalScore : {testRankData.rankdata_score[5].totalScore}");
            Debug.Log($"[RankData] Client testRankData.rankdata_score[5].scorePlace : {testRankData.rankdata_score[5].scorePlace}");
            Debug.Log($"[RankData] Client testRankData.rankdata_score[5].highestScorePlace : {testRankData.rankdata_score[5].highestScorePlace}");

        }
        catch
        {
            Debug.Log($"[RankData] try-catch error");
        }
        

        // ExpenditionCrew
        Debug.Log($"[ExpendtionCrew] testExpenditionCrew.SelectedCrew : {testExpenditionCrew.SelectedCrew}");
        for (int i = 0; i < testExpenditionCrew.OwnedCrew.Count; i++)
        {
            Debug.Log($"[ExpendtionCrew] testExpenditionCrew.OwnedCrew[{i}] : {testExpenditionCrew.OwnedCrew[i]}");
        }

        // LastPlayData
        // Venezia_Kor, Level1
        Debug.Log($"[LastPlayData] testLastPlayData.Step[(Game_Type.A,1)] : {testLastPlayData.Step[(Game_Type.A, 1)]}");

        // AnalyticsProfileData
        // [0] -> level1, [1] -> level2, [2] -> level3
        // Item1 = 게임명, Item2 = ReactionRate, Item3 = AnswerRate
        Debug.Log($"[AnalyticsProfileData] testAnalyticsProfileData.Data[0].Item1 : {testAnalyticsProfileData.Data[0].Item1}");
        Debug.Log($"[AnalyticsProfileData] testAnalyticsProfileData.Data[0].Item2 : {testAnalyticsProfileData.Data[0].Item2}");
        Debug.Log($"[AnalyticsProfileData] testAnalyticsProfileData.Data[0].Item3 : {testAnalyticsProfileData.Data[0].Item3}");
        

    }

    public void OnClickSaveProfileTest()
    {
        string base64 = "/9j/4AAQSkZJRgABAQAAAQABAAD/2wCEAAkGBxMSERUTExMVFhUVFRcXGBgXFRUVGhodFhUXGBUXFxgYHSggGBolGxcaIjEhJSkrLi4uFx8zODMtNygtLisBCgoKDg0OFxAQFy0eHyUtLS0tLS0tLS0tLS0tLS0tLS0tNy0tLS0rLS0tKystKy03Ky0rKy0tLS0rLS0tLS0tK//AABEIAK8BIAMBIgACEQEDEQH/xAAcAAEBAAIDAQEAAAAAAAAAAAAAAQUGAwQHAgj/xAA+EAABAgQEAgYJBAIBAwUAAAABABECITFBAwRRYTJCBQYScZHwBxMiYoGCoaLBUnKxshQj0TNDkiREU+Hx/8QAFgEBAQEAAAAAAAAAAAAAAAAAAAEC/8QAGxEBAQEAAwEBAAAAAAAAAAAAAAERAiFBMRL/2gAMAwEAAhEDEQA/APanm/NoglSZNdkm7c2qDat90EFGHDcoaMeGxQUlw3CGjnhsEFO8mpujzfm0Q7/Lt5kk3bmsUAG4rcKCVJg1Oioe3FcqDal0Czcuqp3kBTdSz8uip3pZAJua2CPN+a4Qu8+KxSbsOK58/BAEqTeuylm5dVRt8d1LPy6IBoxpYqkvWRFBqoaT4bBUvetkB5vzaIC1Jk12SbtzaoNq3QQUYcNylm5dUFJcNwho54bBBTOsmpujzfmsEO/y7eZJN25rHz8UAG4rcKDaYNdlQ9uK5UG1LoFm5dUO8gKbpZ+XRDvSyCk3NbBHm/Nohe/FYoHdua58/BAEqTeuylm5dVRt82/mago44bhANGPDYqmdZEU3UNJ8Ngqd62QHm/NogLTEzcaJdubVA9q3QQUYTFylm5dUFJcNwln5dEFa33Iz7NfVSXyp305UFd5021R712Tv4rJ3cV0Cm7/Tz+Ea33KDb5vPikvlugrPKjX1Ss6NbVQtfhsqd62QN/tSm720T+3n8INq3QGtV76I1vrr5/Cga3DdJX4befFBa7N9Ue/2od/l8+Cf2sgUnV7aIzSq99EG3FdQNal0Fa33Izyo19VJfKh3pZBXedGtqm/2p38Vk7uK6BTd/p5/CNb7vPmag2+bz4pL5b+fBBWeVGvqlZ0a2qkr8NlTvWyBv9qUnV7aJ/bz+EG1boDNKr30Rrfd58yUDW4bpL5befFBa7N9fP5R7/aod/l8+CvfxWQSk6vbRVmlV76J3cV1BtS6Ctb7kZ5Ua+qkvlQtelkFrOjW1R7/AGp38Vk/sgj35dEdqzBpsq83vogLUm9dkDY1sU2HFcqAMGEwalCJNbVArSTV3R78uipnWTU3R5vfRBHaZoaBUykZk0OiAtMTJqNFBKQmDU6ILtzaoJ0kRXdSzW1QzrICm6ADcUFQj35bBUmbmooEeb30QDKs3psm3NqglSb12UaTW1QWshUVKgLzEgKjVDMMZAUOqpLzMiKDVBHvy6I7TMwabKvN76IC0xMmo0QKSNTQptzaqAMGEwalGk1tUFE6Sau6j35dFTOsmpujze+iCO0zQ0CplIzJpsgLTFTUaKEgA6XOmqC7c2qCchIiu66fRnSmBmIO1gY2Hi4YLGPDjhjAIsWMiu2ZyMgKHVABeYoKhHvy6LW+t/W7DybQBo8zGHgw3lCP/kxTywO+8RDC5H31HwscYEWNj40WNiY+IY2JlBCwhhhghEoR7LsP1fFBsJlWb02V25tUEqTeuyjSa2qC1kKipUd5iQFd0IcMZAUKpLzMiKboI9+XRCWmZg0Girze+iAtMTJqNECkjMmhTbm1UEgwmDU6I0mtqgs395BtW6Na/wCpAH2a+qCCkuG6Gk+GyCc6AW1Te2miCnf5fPgk396yENu/08/hGtf9SAHtxXUG1LqgPKjX1QTnRraoJb3fP5VO9LKb2/SqZbvbRAL34rJN5cV/PgjWqTfRGtfXXz+EAbfMpb3U1s1d+9YTN9cMhhk9rN4IIrAIxH4iB/4QZs0nw2VO9bLW4OvnRx/91hnZo5eMK7eW61ZGMtDnMvETriwAj4EumDM395BtW64MHN4ccoMSCL3oYoT/AAVzgPSTX1QQUlw3S0+GytZ0Atqpvb9KCnf5fPgk3963nxQy3f6efwjWvr58zQA9uK61v0jY8UHRebOHfBMBm0oyIIj/AOMRXH091+yOUiOHHjdvEhLGHCHrIg1oiCIYTsS68+68elLBzWUxcvg4GJD6wAGKMwgMIgSwhJclm+KLGq9WOnIsnD67CjhGIOKGZEYeUMUI4herizVGzZr0xZnEhAgwcPCIE64pFnDsG7wdO/zCCIEku0qLs4EcQiBHtH9Ni9mFUhXYHTWMMePHMfrI8Q9qKKMCMkyme4MBowC/SvVHMjFyOWjgrFgYZnLkAiBGrrw/qN0Bh58ZvAjBw8WH1cWGTIwxe04INiAxld9FvforzGYwcWPJZiIcBxIRN4IoI+ziQ7guC9HhPxrOvSxt83nxUtLhuqJ7N9fP5U3t+lRQ0nw2VO9bKGU6g20VIaVXvogTf3kD2rdGtf8AUgDyo19UEFJcN0t7qCc6NbVN7fpQGty6p30FN0e/LolK0NNkF3PFYJuOK4TY8Vimw4rlBBteu3maNbl1QbWrv5mj35dEAixpYqnetlCRU0sFTvWyBvzaINqmuybc2q4sxjwwQRRxRCCGCExRxGQAhDxEnQAEoPuOIQgzAgrESWb42WndYfSNlMuDDhn18QoICOy/vYlG/a68i65ddsbpHGiAiOHlREfV4QkCAZR4n6ozVjKGguTrxxQysgz3WTrpnM44xsY9g/8Abg9jD7jCJxfMStcijK5IWquPFCqHr91yDG7l0IykBLpq475jB08F2Mtn8TDnh4kcH7IooP6ldHDjC5hF8POqaYzmX6352AyzeY+ONiRf2KyOT9I3SMBlmYov3Q4cf1MLrTxiTYr5xcWckR6XkvSvnIXEQwI3qTBED9sYH0WL6wekbOZjDOEY4cOCLiGDCYDENDEYjEB3EP3LTcPEdMtlYsWMwQTLE+Ac/BkVxxYsLMIQAFxYGHFHF2YQ5NAvnEDON1n+pQh/yITECTNmBkd1Ksa5G8BIIYizXFVzZLpCLCjGJCxiGocTDEEdy9I6U6vZfGjMcUJc8QhLCLcsp0b1by8EQ7GEH1JMZG7xH+Fj9Rq8WQ9G3RWKYoukMRoAcPsQ4cIZwDCxE5ACAAVftLdehsnlMfNjO4J/29kwx9mL2SIgQ5E2jBAHwKxWcy2ahwoRhYEcQAYMD9QJ/RbD1Q6Liw8P12OD6yMkkaBgIZasLres/npsQc8Umo3nuV3PFYITremybHisVENxxXCg2oa7K7DiuVBtQV3QGty6oRrSyPfl0QnWhogp1PFYJvzaIdDxWKbc2qA976IC2720Sb+8g2rdBBKVQb6JtbVBSXDdDSfDZBSX2am/n8o976Id/l8+CTf3rIALTqTbRQSlV76Kh7cV1BtS6Btb9S8z9O/Txwsnh5WAscxGe018PDYxD4xRQd47QXplvd8/lfnH009Lev6VjgB9nLwQYI0fjjPjG3yoNQOIwdBirrxAkFgTc7AVJ0CsBmkadsRL5iiXz8F84hVR9M/ll8YYcrJdAZYYuLBA04ogHnLWi4OsHRWJlMxHhRvI+yW4oTwxDzYp4jjw4A9Vy3LaLpgrngxGv/CiuWMAwuupDNc4is9lwYQYlB2sCJge9bP6P8t6zNxaCFz8ZfFargAMSV6T6JcsDDjYpFYhCD3Bz/K1ErBdN9Ssf10XqoDFCSSG/OhWy9UOrBy8PbxA2JEG7NeyP+VuscIBkuHNUks1WDzGXPa71n+rPRAMbmgLn4W/gfErHQQB3W4dX8JoO0QwoPopx4+nLl4y5N6EW1R730Qvfisk396/nwVQEt3rt5/Cm1tVRt83nxUtLhugGcqAX1VJedGtqoaT4bKnetkB730QFp1e2iX95A9q3QQSlUG+ibW/UgpLhulvdQVptfVAHpJq7qNbl1RnrICm6AC4cUFkJk9tFdzxWCbjiuEAyrN6bI02vqozUm9dka3LqgoDyEiKnVQTmJAVGqM8jQUKpnMyIoNUHxi4ohhMcRaCEEnQAVP0dfkLpTO+vzGLjn/u4seJ3esjMTfVe2+mrrnDg4UWRwj/ALsaD/cRTDw4uX90YDNaEk3C8JhhdFj0X0I9GDH6QjiigEWHBl8QROHD4hhhhBF3Ha8Fy9NdQhls7iwmMDB7XbgAYHsxe0IXOjt8F6J6Her3+L0fDiRBsTNNiRm4hn6mH/xPa74yvrrhlxiY0QABAgENtH/P0UHhHTGNDHiRdiGGAOwhDGknJuV0uybjwXYz+W7GLHAQzREMQy4KUMwqr6yGaiwoxFDWEghepdNdDQ9K5TDxICBjQAMbF+KGLaS8lxcV56L1H0Z5xsrFJ3xC3whH5WpWbHnHSHRmJgYsWHiAiOH4ysRsuGTOV6t1u6rYubMGLhYcRiA7LgGc5LG9FeirNRgnFg7Pao8UMLd83+izWo8+wYniXEYarfOlfRrmME0iA1A7Y8RT4rH4fVIQn/ZFEdZM+yzq4wHRHRmLmYxh4YncmgFHK9l6r5GDK4MOBDEIjDxENMmZOy866V6UGUh9Tlx2DGHijebUYHVYvojprEwYnERmXM6rcZr3fHaTBdDMRrr5TpHt4UMQuAfHvXxiPIXKlRkehsmcXEnOETi7hX/j4rd4YRCBKVhosd0BlBh4YI4jbby6yYlMTJrsgENIzJodEabX1UAaQoalGty6oKJ0k1d1Hk9tEM6yam6u/NoghLBzMGgVIaRmTQ6JSYqahRmkJg12QVptfVAHkJEVOqjW5dUIeRkBQ6oAmHEgKjVHk9tFazMiKBN+bRBJfKnfTlVe/wBqO2720QO/isndxXRmlV76I1vqg8n9OmbzWD/i4mBi4uHhPHDGcOOKD/Z7Jw+12SH9kRtaRWQ9F/pCGah/xs3HCMaEezEWh9cPoPWC4FRMCRW89O9EYWcy+Jl8aH2Iwx1esMUJtECAQV+Y+tvVXMdHYpw8eEmAn/XigexiCzaRawmY3DE0fqLOdIYOFCYsbFw4IBeOOGADvJIXmXXj0vYWHAcPIEYuLT1xh/1wbwAt6yLSXZvOh8MYCbDwC7/RXRePm4xBl8GPFiNoIXA/dFwwjckKLjp5nMRYkcUccUUccZMUUUReKImZJJqVvnor6hx5/FGPjQtlcOIE9of9Yg8EOsP6jS1abT1L9DUMMQxOkIoYjIw4MBJgF/8AZHLtftEtyvXcHBhhhEEAEEMAAhAAAYSAAFAhrqdJ9IQ4MMmmC0Pdc7CS03Hx4Iie0TFEbsDP4UWR69kwxYWIAQCIoCO5oh9O14LCHNhgBLWX1U0xq/XXqucaH1uE3rAJhm7QGlnXmOJARF2ezEI37LMXJdmAudl7PBmu1idkEs7RRNvOV1vvQ3VTKZfEONh4cJxYh/1ixiYjlNIX2QeX9Q/RHHiRQ4+f9mHiGX5otPWkH2R7ombtML2bK5TDw4BBhQQwYcIbswwiEAaACi56yo19Ud50a2qqH9VDvSyr3+1HadXtogd/FZfMeGIpEAxXcAhfTNKr30Rrfcg8c9PnQ0PZy2YwsMA9qLCjMMOo7cBLftjXkMEWy/VXWnJeuy8TCeH7Y3YF2+V/FeaZToHK9v1nqIO0Z0l3tRTWnf6GypwMnAYxMYcL3mRTxK7mTLtEsvk8mMeCLDPNCQNj2fZPwP8ACwWSiML4cQIihJBGhBmrfqT49E6OI9XC1WXZG1brodD4j4YH1XfrKjX1VrMQNbhukvlt58VXedGtqj3+1RUO/wAvnwV7+KyU3f6efwjW+qB3cV1BtS6M8qNfVV3nRraoJL5ULXpZV7/ajtOr20QDvxWT+yUlV76Jt9yBN/e0QbVvsjTbm1QTpIiu6CCkuG5Q0nw2KCjjhuENHPDYIKd/l38yXDm8rBiwmDFggjB5I4RFCe8GS5jKs3psjTbm1QYLD6m9HQl4chlO1f8A9PhFu5wszgYMMEPZw4RDALACEDuAouQCwrcqCcxICo1QLNy6od6WSz8uiplWYNNkGH625M4uVxA3twjtwj9kz9O0PitF6HHbDRORMVsvUiLGtitAzmQ/xsxFAB7LvD+2L/gy+Cmd6u9Y17O5L/CxxhxEnDxPawYzcfpPvB5/DVbT0d0pGIA2IWFA7jwXLmhDiYYw8UCPDkTDEHDihBrCdwQV28h1WysUIjg9bDDeH1kUXgYnLfFXMN1meic767DeJvZLONWB/grvF71suLL4EGHABDCBBYD+S9TuuUhpGZNDoiE397RBtW6NNubVBOkiK7oIKS4blLT4bFBRxS4Sz8uiBGHlF8N+9eZerMGJHhxVgjihroZNsvTjKs3pstG61ZQwZvtSaOGGKtSPZMvgFKsZjq2falVisb12yPq4hmcPhLQ4mx5YvjT4Bd/q7hkR1Z6P+0n8FZrpPKeuwcTDEu1CR8awn4FiryiSsZ1ZxHgYmTT+lFnHBrQU/wDtaL0Tm8fLwmHGwooSAwi7JY9xEj4rPdF53ExCD2D2XnEQRC161Pcr9TuM6XvxWCTf3rjz8EIsa2KNNua5UUG3zbeZqCkuG5VE6Sau6ln5dEA0nw2Kp3rZQ0c8NgqZVmTTZAm/vaIHtW6NNubVAHkJEVOqCCkuG5SzcuqCjiQuEs/LogNa2qVrJqbpL5U76cqCu86HTVHvfRO/isndxXQSlJvXZGtbVO75vPikvlugM8jIC+qtZmRFBqpK/DZU71sgPe+iUpN6jRP7efwg2rdBGtUG+i6nSPRuHjgQxuOzwxgsQ9WNPhOi7crcN0lfht58UGs4fVnEEf8A1x2LPB7RGlWJ3+i2DJ5cYcEMIc9kNOZO5/8Axc53+Xz4J/ZAdpiZNtFGaQmDU6K93FdQNal0BrW1RnkZNQ6pL5Ve+lkEd5mRFtVXvfRO/isndxXQSlJvXZa/1xy/+uDEE+xExO0cv5A8VsI2+bz4r5xIIYgQQDAQ0QM3exCDVslnhCBDf4v8FnOjc76zdgCCL6ArrYfVvLwxdpo2sPWRkfy6yeXy8GGOzDDDD+kQgAfRXUxyve+iO0xMmo0T+3n8INq3UVGaVQb6I1rapK3DdJfLbz4oFayam6r3vood/l8+CvfxWQR2mJk20RmkJvU6K93FdO6l0Ea1tUZ5GQFDqkvlSV6WQHeZkRbVV730Tv4rJ/ZAe9tFHas3psj35tEBalTXZBWtU66I1r6qUkKXKbHhsUFrSTV3Ue9tEJetqbo9+bRAdp1BtorSRmTQ6KO0xU1CCUhQ1QXa+qVpJq7qbcuqVrQUQHvQC2qPe2miPc1FAj35rhBaVm9Nka19VAWpeuybcuqC1lQi+qO8xJqjVQzkaChQl5moogPe2irtMzeg0Ue/NogLTFTVBaSqTfRGtfVSkhQ1KbcuqC1pJq7qPe2iEvW1N0e/NogrtOoNtEpKr0OijtMVNQlJChqgu19UrKjVOqm3LqlZGgogrvOgFtVHvbRHeZqKBHvzaIFKzemyrWvqo7UvXZNuXVBWeVCL6qO8xJqjVKyNBQoS8zUUQV720UdpmYNtEe/NogLTFTVBaSMyb6I1r6qCUhQ1KbcuqD//2Q==";
        byte[] profile = Convert.FromBase64String(base64);

        Client.instance.RegisterCharactorProfile_SaveDataToDB(profile);
    }


}
