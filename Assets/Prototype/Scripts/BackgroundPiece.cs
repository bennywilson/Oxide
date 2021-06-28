using System.Collections.Generic;
using UnityEngine;

public class BackgroundPiece : MonoBehaviour
{
    static List<BackgroundPiece> _allPieces = new List<BackgroundPiece>();

    public static int GetPieceCount() => _allPieces.Count;
    public static BackgroundPiece GetPieceAt(int index) => _allPieces[index];

    //Currently just used as a tag to identify background pieces.
    bool _isInitialized;
    Vector3 _initialPosition;
    Bounds _initialBounds;

    public Vector3 InitialPosition => _initialPosition;
    public Bounds InitialBounds => _initialBounds;

    void OnEnable()
    {
        if (!_isInitialized)
        {
            _isInitialized = true;
            _initialPosition = transform.position;
        }

        if (!_allPieces.Contains(this))
        {
            _allPieces.Add(this);
        }
    }

    void OnDisable()
    {
        _allPieces.Remove(this);
    }
}
