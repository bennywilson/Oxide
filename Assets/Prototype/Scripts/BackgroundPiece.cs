using System.Collections.Generic;
using UnityEngine;

public class BackgroundPiece : MonoBehaviour
{
    static List<BackgroundPiece> _allPieces = new List<BackgroundPiece>();

    [SerializeField]
    GameObject _PlayerCamera;

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

    void Update()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, _PlayerCamera.transform.position.z + 100.0f);
    }
    void OnDisable()
    {
        _allPieces.Remove(this);
    }
}
