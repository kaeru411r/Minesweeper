using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    /// <summary>�Q�[���̐i�s���</summary>
    bool _isPlay = false;

    /// <summary>�Q�[���̐i�s���</summary>
    public bool IsPlay { get => _isPlay; }

    private void Start()
    {
        BoardManager.Instance.OnExplosion += Explosion;
        BoardManager.Instance.SetUp();
    }

    /// <summary>
    /// �Q�[���̊J�n
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    public void GameStart(int row, int col)
    {
        _isPlay = true;
        BoardManager.Instance.SetUp(row, col);
        BoardManager.Instance.Dig(row, col);
    }

    public void Explosion()
    {
        _isPlay = false;
        BoardManager.Instance.SetUp();
    }
}
