using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RoomSegmentPool : ScriptableObject
{
    public RoomSegmentSpawnInfo[] segments;

    public List<RoomSegmentSpawnInfo> GetAvailableSegments(RoomSegmentSlot slot)
    {
        return segments.Where(s => s.prefab.slot == slot.slotType).ToList();
    }
}
