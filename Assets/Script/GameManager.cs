using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    bool _isPlay = false;

    public bool IsPlay { get => _isPlay;}

    public void GameStart(int row ,int col)
    {
        BoardManager.Instance.SetUp(row, col);
        _isPlay = true;
    }
}
