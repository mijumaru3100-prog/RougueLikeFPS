using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    public static DungeonManager Instance;

    void Awake() => Instance = this;

    [Header("部屋情報")]
    public GameObject[] battleRooms;
    public GameObject[] eliteRooms;
    public GameObject[] shopRooms;
    public GameObject[] treasureRooms;
    public GameObject[] bossRooms;

    [Header("現在の部屋")]
    public GameObject currentRoom;
    public Stage currentRoomScript;

    public enum roomType
    {
        battle,
        elite,
        shop,
        treasure,
        boss
    }

    public GameObject SelectRoom(roomType type)
    {
        GameObject[] targetRoom = null;

        switch (type)
        {
            case roomType.battle:
                targetRoom = battleRooms;
                break;
            case roomType.elite:
                targetRoom = eliteRooms;
                break;
            case roomType.shop:
                targetRoom = shopRooms;
                break;
            case roomType.treasure:
                targetRoom = treasureRooms;
                break;
            case roomType.boss:
                targetRoom = bossRooms;
                break;
        }

        if (targetRoom == null || targetRoom.Length == 0) return null;

        int idx = Random.Range(0, targetRoom.Length);
        currentRoom = targetRoom[idx];
        currentRoomScript = currentRoom.GetComponent<Stage>();
        return currentRoom;
    }
}