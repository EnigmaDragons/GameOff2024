
using UnityEngine;

public enum RoomSlotType
{
    LargeSegment,
    SmallSquare
}

public enum RoomSegmentType
{
    Decor,
    Blocker,
    Empty,
    Obstacle,
    Connector,
}

public enum RoomObstacleType
{
    None,
    Jump,
    Walk,
    Slide,
    Climb,
}

public class RoomSegment : MonoBehaviour
{
    public RoomSlotType slot;
    public RoomSegmentType segmentType;
    public RoomObstacleType obstacleType;
}
