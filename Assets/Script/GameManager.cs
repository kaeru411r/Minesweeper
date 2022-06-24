using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    static public GameManager Instance;

    /// <summary>ゲームの進行状態</summary>
    bool _isPlay = false;

    /// <summary>ゲームの進行状態</summary>
    public bool IsPlay { get => _isPlay; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        BoardManager.Instance.OnExplosion += Explosion;
        BoardManager.Instance.SetField();
        BoardManager.Instance.MineLaying();
    }

    /// <summary>
    /// ゲームの開始
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    public void GameStart(Vector2Int point)
    {
        BoardManager.Instance.SetField();
        if (BoardManager.Instance.MineLaying(point))
        {
            _isPlay = true;
            BoardManager.Instance.Dig(point)
;
        }
    }

    public void Explosion()
    {
        _isPlay = false;
        BoardManager.Instance.SetField();
    }
}
