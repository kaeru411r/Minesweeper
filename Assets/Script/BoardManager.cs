using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

/// <summary>
/// ゲームボードの管理をするクラス
/// </summary>
public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { get; private set; }

    [SerializeField] int _bomb;
    [SerializeField] int _row;
    [SerializeField] int _col;
    [SerializeField] int _row2;
    [SerializeField] int _col2;
    [SerializeField] bool _dig = false;
    [SerializeField] bool _flag = false;

    Sell[,] _field;

    public Sell[,] Field { get => _field;}

    private void Awake()
    {
        BoardManager.Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        _field = new Sell[_row, _col];

        for (int i = 0; i < _bomb; i++)
        {
            int r = UnityEngine.Random.Range(0, _row);
            int c = UnityEngine.Random.Range(0, _col);
            _field[r, c].bomb = true;
            for (int l = r - 1 >= 0 ? r - 1 : 0; l < _field.GetLength(0) && l <= r + 1; l++)
            {
                for (int m = c - 1 >= 0 ? c - 1 : 0; m < _field.GetLength(1) && m <= c + 1; m++)
                {
                    _field[l, m].number++;
                }
            }
        }

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < _row; i++)
        {
            for (int l = 0, m = _col; l < m; l++)
            {
                if(_field[i, l].bomb)
                {
                    sb.Append("b");
                }
                else
                {
                    sb.Append(_field[i, l].number);
                }
            }
            sb.AppendLine();
        }
        Debug.Log(sb);

    }

    private void Update()
    {
        if (_dig)
        {
            _dig = false;
            Dig(_row2, _col2);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < _row; i++)
            {
                for (int l = 0, m = _col; l < m; l++)
                {
                    if (_field[i, l].state == SellState.Nomal)
                    {
                        if (_field[i, l].bomb)
                        {
                            sb.Append("b");
                        }
                        else
                        {
                            sb.Append(_field[i, l].number);
                        }
                    }
                    else if (_field[i, l].state == SellState.Flag)
                    {
                        sb.Append("f");
                    }
                    else
                    {
                        sb.Append("x");
                    }
                }
                sb.AppendLine();
            }
            Debug.Log(sb);
        }
        if (_flag)
        {
            _flag = false;
            Flag(_row2, _col2);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < _row; i++)
            {
                for (int l = 0, m = _col; l < m; l++)
                {
                    if (_field[i, l].state == SellState.Nomal)
                    {
                        if (_field[i, l].bomb)
                        {
                            sb.Append("b");
                        }
                        else
                        {
                            sb.Append(_field[i, l].number);
                        }
                    }
                    else if(_field[i, l].state == SellState.Flag) 
                    {
                        sb.Append("f");
                    }
                    else
                    {
                        sb.Append("x");
                    }
                }
                sb.AppendLine();
            }
            Debug.Log(sb);
        }
    }

    public void Dig(int r, int c)
    {
        if(r >= 0 && r < _row && c >= 0 && c < _col)
        {
            if (_field[r, c].state == SellState.Nomal)
            {
                if (_field[r, c].bomb)
                {
                    Explosion();
                }
                else if (_field[r, c].number == 0)
                {
                    _field[r, c].state = SellState.Dug;
                    for (int i = r - 1 >= 0 ? r - 1 : 0; i < _field.GetLength(0) && i <= r + 1; i++)
                    {
                        for (int l = c - 1 >= 0 ? c - 1 : 0; l < _field.GetLength(1) && l <= c + 1; l++)
                        {
                            if (_field[i, l].state == SellState.Nomal)
                            {
                                Dig(i, l);
                            }
                        }
                    }
                }
                else
                {
                    _field[r, c].state = SellState.Dug;
                }
            }
        } 
    }

    public void Flag(int r, int c)
    {
        if (_field[r, c].state == SellState.Flag)
        {
            _field[r, c].state = SellState.Nomal;
        }
        else if(_field[r, c].state == SellState.Nomal)
        {
            _field[r, c].state = SellState.Flag;
        }
    }

    void Explosion()
    {
        Debug.Log("bomb");
    }
}

/// <summary>
/// マスの情報を記録する
/// </summary>
public struct Sell
{
    public int number;
    public SellState state;
    public bool bomb;
}

/// <summary>
/// マスの状態
/// </summary>
 public enum SellState
{
    /// <summary>何もしてない</summary>
    Nomal,
    /// <summary>掘った</summary>
    Dug,
    /// <summary>旗を立てた</summary>
    Flag,
}
