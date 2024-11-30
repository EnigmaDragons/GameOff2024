#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;
using System.Linq;

public static class RoomSegmentEditorAssetUpdater
{
    [MenuItem("Tools/Update Room Segment Pools %j")]
    public static void UpdateRoomSegments()
    {
        // Find all RoomSegmentPool assets
        var guids = AssetDatabase.FindAssets("t:RoomSegmentPool");
        var pools = guids.Select(guid => AssetDatabase.LoadAssetAtPath<RoomSegmentPool>(AssetDatabase.GUIDToAssetPath(guid))).ToArray();

        if (pools.Length == 0)
        {
            Debug.LogWarning("No RoomSegmentPool assets found");
            return;
        }

        // Find all prefabs in the RoomSegments folder
        var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/RoomSegments" });
        var segments = prefabGuids
            .Select(guid => AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(prefab => prefab.GetComponent<RoomSegment>() != null)
            .Select(prefab => 
            {
                var difficultyMatch = Regex.Match(prefab.name, @"_D(\d+)_");
                int difficulty = difficultyMatch.Success ? int.Parse(difficultyMatch.Groups[1].Value) : 1;
                
                return new RoomSegmentSpawnInfo
                {
                    prefab = prefab.GetComponent<RoomSegment>(),
                    difficulty = difficulty
                };
            })
            .ToArray();

        if (segments.Length == 0)
        {
            Debug.LogWarning("No room segment prefabs found in Assets/Prefabs/RoomSegments");
            return;
        }

        // Update each pool
        foreach (var pool in pools)
        {
            Undo.RecordObject(pool, "Update Room Segments");
            pool.segments = segments;
            EditorUtility.SetDirty(pool);
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Updated {pools.Length} RoomSegmentPool(s) with {segments.Length} segments");
    }
}
#endif
