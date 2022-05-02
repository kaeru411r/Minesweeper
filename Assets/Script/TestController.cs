using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestController : MonoBehaviour
{
    [SerializeField] int _row;
    [SerializeField] int _col;
    [SerializeField] bool _dig = false;
    [SerializeField] bool _flag = false;
    private void Update()
    {
        if (_dig)
        {
            if (GameManager.Instance.IsPlay)
            {
                BoardManager.Instance.Dig(_row, _col);
            }
            else
            {
                GameManager.Instance.GameStart(_row, _col);
            }
            _dig = false;
        }
        if (_flag)
        {
            _flag = false;
            BoardManager.Instance.Flag(_row, _col);
        }
    }
}
