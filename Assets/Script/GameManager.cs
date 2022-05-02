using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    /// <summary>�Q�[���̐i�s���</summary>
    bool _isPlay = false;

    /// <summary>�Q�[���̐i�s���</summary>
    public bool IsPlay { get => _isPlay;}

    [SerializeField] int _row;
    [SerializeField] int _col;
    [SerializeField] bool _dig = false;
    [SerializeField] bool _flag = false;
    [SerializeField] bool _start = false;
    private void Start()
    {
        BoardManager.Instance.OnExplosion += Explosion;
    }
    private void Update()
    {
        if (_dig)
        {
            _dig = false;
            BoardManager.Instance.Dig(_row, _col);
        }
        if (_flag)
        {
            _flag = false;
            BoardManager.Instance.Flag(_row, _col);
        }
        if (_start)
        {
            _start = false;
            GameStart(_row, _col);
        }
    }

    /// <summary>
    /// �Q�[���̊J�n
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    public void GameStart(int row ,int col)
    {
        BoardManager.Instance.SetUp(row, col);
        _isPlay = true;
        BoardManager.Instance.Dig(row, col);
    }

    public void Explosion()
    {
        GameStart(_row, _col);
    }
}
