using UnityEngine;

public class BackgroundPieceMover : MonoBehaviour
{
    [SerializeField] Transform _origin = null;
    [SerializeField] float _riseBeginDistance = 5000f;
    [SerializeField] float _riseEndDistance = 1000f;
    [SerializeField] float _maxDropDistance = 1000f;
    [SerializeField] float _riseCurve = 1.5f;

    public Transform GetOrigin() => _origin != null ? _origin : transform;

    void Update()
    {
        int pieceCount = BackgroundPiece.GetPieceCount();

        Vector3 origin = GetOrigin().position;

        for (int i = 0; i < pieceCount; i++)
        {
            var currentPiece = BackgroundPiece.GetPieceAt(i);
            if (currentPiece == null)
                continue;

            var initalPosition = currentPiece.InitialPosition;
            float dist = Mathf.Abs(initalPosition.z - origin.z);

            float ratio = (dist - _riseEndDistance) / (_riseBeginDistance - _riseEndDistance);
            ratio = Mathf.Clamp01(ratio);
            ratio = Mathf.Pow(ratio, _riseCurve);

            var newPosition = initalPosition;
            newPosition.y -= _maxDropDistance * ratio;

            currentPiece.transform.position = newPosition;
        }
    }
}
