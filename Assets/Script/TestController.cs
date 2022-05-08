using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestController : MonoBehaviour
{
    [SerializeField] int _row;
    [SerializeField] int _col;
    [SerializeField] bool _dig = false;
    [SerializeField] bool _flag = false;
    [SerializeField] BoardManager _boardManager;
    [SerializeField] GameManager _gameManager;
    private void Update()
    {
        if (_dig)
        {
            if (_gameManager.IsPlay)
            {
                _boardManager.Dig(_row, _col);
            }
            else
            {
                _gameManager.GameStart(_row, _col);
            }
            _dig = false;
        }
        if (_flag)
        {
            _flag = false;
            _boardManager.Flag(_row, _col);
        }
    }
}
