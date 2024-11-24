using UnityEngine;

public class RecordTransformMarker : MonoBehaviour
{
    [SerializeField] private Transform marker;
    [SerializeField] private TransformMarker markerType;

    private void Start()
    {
        if (markerType == TransformMarker.Player)
            CurrentGameState.UpdateState(gs => gs.playerTransform = marker);
        if (markerType == TransformMarker.SpyDestination)
            CurrentGameState.UpdateState(gs => gs.spyDestination = marker);
        if (markerType == TransformMarker.HandlerCoverDestination)
            CurrentGameState.UpdateState(gs => gs.coverDestination = marker);
        if (markerType == TransformMarker.HandlerCoverLookPoint)
            CurrentGameState.UpdateState(gs => gs.coverLookPoint = marker);
    }
}
