using UnityEngine;

public static class UserSession
{
    public static string Token;
    public static string RoomId;

 
    public static void Clear()
    {
        Token = null;
    }
}