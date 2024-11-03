using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomSegmentSelector : MonoBehaviour
{
    [SerializeField] private RoomSegmentPool pool;
    [SerializeField] private RoomSegmentSlot slots;

    // Done - Rule 1: Spawn something in all the slots
    // Done - Rule 2: Don't spawn the same thing in a slot group
    // TODO - Enhancement - Rule 3: Try to mix up the obstacles in general
    // TODO - Enhancement - Rule 4: Try to mix up the Obstacle Types. Prefer not to make the same type of obstacle in the same group
    // TODO - Enhancement - Rule 5: Later, we'll factor difficulty into the mix

    private void Start()
    {
        SpawnAllSegments();
    }

    private RoomSegmentSlot[][] GetGroupedSlots()
    {
        var slotGroups = new Dictionary<int, List<RoomSegmentSlot>>();
        foreach (var slot in slots.GetComponentsInChildren<RoomSegmentSlot>())
        {
            var groupId = slot.roomGroupId;
            if (!slotGroups.ContainsKey(groupId))
            {
                slotGroups[groupId] = new List<RoomSegmentSlot>();
            }
            slotGroups[groupId].Add(slot);
        }
        return slotGroups.Select(x => x.Value.ToArray()).ToArray();
    }

    private void SpawnAllSegments()
    {
        // 1. Group the slots by room group id
        var slotGroups = GetGroupedSlots();

        // 2. Spawn Into Each Group
        foreach (var group in slotGroups)
        {
            var availableSegments = pool.GetAvailableSegments(group[0]);
            var usedSegments = new HashSet<RoomSegmentSpawnInfo>();
            
            // 3. Spawn Into Each Slot
            foreach (var slot in group)
            {
                // Filter out already used segments for this group
                var validSegments = availableSegments.Where(s => !usedSegments.Contains(s)).ToList();
                if (!validSegments.Any())
                {
                    // If we've used all segments, reset the used segments tracking
                    usedSegments.Clear();
                    validSegments = availableSegments.ToList();
                }

                // Select random segment from remaining valid options
                var selectedSegment = validSegments[Random.Range(0, validSegments.Count)];
                usedSegments.Add(selectedSegment);

                // Instantiate at slot position
                Instantiate(selectedSegment.prefab, slot.transform.position, slot.transform.rotation, slot.transform);
            }
        }
    }
}
