using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestController : MonoBehaviour
{
    [SerializeField] Vector2Int _point;
    [SerializeField] bool _dig = false;
    [SerializeField] bool _flag = false;
    private void Update()
    {
        if (_dig)
        {
            if (GameManager.Instance.IsPlay)
            {
                BoardManager.Instance.Dig(_point);
            }
            else
            {
                GameManager.Instance.GameStart(_point);
            }
            _dig = false;
            Debug.Log(GameManager.Instance.IsPlay);
        }
        if (_flag)
        {
            _flag = false;
            BoardManager.Instance.Flag(_point);
        }
    }
}
