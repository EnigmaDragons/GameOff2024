
using UnityEngine;

public enum RoomSlotType
{
    LargeSegment = 0,
    SmallSquare = 1,
}

public enum RoomSegmentType
{
    Obstacle = 0,
    Decor = 1,
    Blocker = 2,
    Empty = 3,
    Connector = 4,
}

public enum RoomObstacleType
{
    Walk = 0,
    Jump = 1,
    Slide = 2,
    Climb = 3,
    None = 4,
}

public class RoomSegment : MonoBehaviour
{
    public RoomSlotType slot;
    public RoomSegmentType segmentType;
    public RoomObstacleType obstacleType;
}
