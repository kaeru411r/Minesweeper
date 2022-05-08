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
        _boardManager.SetUp();
    }

    /// <summary>
    /// ゲームの開始
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    public void GameStart(int row, int col)
    {
        if (_boardManager.SetUp(row, col))
        {
            _isPlay = true;
            _boardManager.Dig(row, col);
        }
    }

    public void Explosion()
    {
        _isPlay = false;
        _boardManager.SetUp();
    }
}
