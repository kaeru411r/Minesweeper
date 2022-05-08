using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] BoardManager _boardManager;
    /// <summary>�Q�[���̐i�s���</summary>
    bool _isPlay = false;

    /// <summary>�Q�[���̐i�s���</summary>
    public bool IsPlay { get => _isPlay; }

    private void Start()
    {
        _boardManager.OnExplosion += Explosion;
        _boardManager.SetUp();
    }

    /// <summary>
    /// �Q�[���̊J�n
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
