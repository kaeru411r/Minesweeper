using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestController : MonoBehaviour
{
    [SerializeField] Vector2Int _point;
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
                _boardManager.Dig(_point);
            }
            else
            {
                _gameManager.GameStart(_point);
            }
            _dig = false;
            Debug.Log(_gameManager.IsPlay);
        }
        if (_flag)
        {
            _flag = false;
            _boardManager.Flag(_point);
        }
    }
}
