using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] BoardManager _boardManager;
    /// <summary>ゲームの進行状態</summary>
    bool _isPlay = false;

    /// <summary>ゲームの進行状態</summary>
    public bool IsPlay { get => _isPlay; }

    private void Start()
    {
        _boardManager.OnExplosion += Explosion;
        _boardManager.SetField();
        _boardManager.MineLaying();
    }

    /// <summary>
    /// ゲームの開始
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    public void GameStart(Vector2Int point)
    {
        _boardManager.SetField();
        if (_boardManager.MineLaying(point))
        {
            _isPlay = true;
            _boardManager.Dig(point)
;
        }
    }

    public void Explosion()
    {
        _isPlay = false;
        _boardManager.SetField();
    }
}
