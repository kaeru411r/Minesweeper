using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

/// <summary>
/// ゲームボードの管理をするクラス
/// </summary>
public class BoardManager : SingletonMonoBehaviour<BoardManager>
{
    [Tooltip("爆弾の数")]
    [SerializeField] int _bomb;
    [Tooltip("")]
    [SerializeField] int _row;
    [SerializeField] int _col;
    [SerializeField] int _row2;
    [SerializeField] int _col2;
    [SerializeField] bool _dig = false;
    [SerializeField] bool _flag = false;

    Sell[,] _field;

    public Sell[,] Field { get => _field; }

    public event Action OnSetUp;
    public event Action OnUpdate;
    public event Action OnExplosion;

    // Start is called before the first frame update
    void Start()
    {
        SetUp();
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < _row; i++)
        {
            for (int l = 0, m = _col; l < m; l++)
            {
                if (_field[i, l].Bomb)
                {
                    sb.Append("b");
                }
                else
                {
                    sb.Append(_field[i, l].Number);
                }
            }
            sb.AppendLine();
        }
        Debug.Log(sb);
    }

    public void SetUp()
    {
        SetField(_row, _col);
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < _row; i++)
        {
            for (int l = 0, m = _col; l < m; l++)
            {
                if (_field[i, l].Bomb)
                {
                    sb.Append("b");
                }
                else
                {
                    sb.Append(_field[i, l].Number);
                }
            }
            sb.AppendLine();
        }
        Debug.Log(sb);

        for (int i = 0; i < _bomb; i++)
        {
            int r = UnityEngine.Random.Range(0, _row - 1);
            int c = UnityEngine.Random.Range(0, _col - 1);
            Debug.Log($"{_field.GetLength(0)}, {_field.GetLength(1)}, {r}, {c}");
            _field[r, c].Bomb = true;
            for (int l = r - 1 >= 0 ? r - 1 : 0; l < _field.GetLength(0) && l <= r + 1; l++)
            {
                for (int m = c - 1 >= 0 ? c - 1 : 0; m < _field.GetLength(1) && m <= c + 1; m++)
                {
                    _field[l, m].Number++;
                }
            }
        }

        OnSetUp();
        OnUpdate();
    }

    public void SetUp(int row, int col)
    {
        SetField(_row, _col);

        for (int i = 0; i < _bomb; i++)
        {
            int r = UnityEngine.Random.Range(0, _row);
            int c = UnityEngine.Random.Range(0, _col);

            if (_field[r, c].Bomb && r != row && c != col)
            {
                i--;
            }
            else
            {
                _field[r, c].Bomb = true;
                for (int l = r - 1 >= 0 ? r - 1 : 0; l < _field.GetLength(0) && l <= r + 1; l++)
                {
                    for (int m = c - 1 >= 0 ? c - 1 : 0; m < _field.GetLength(1) && m <= c + 1; m++)
                    {
                        _field[l, m].Number++;
                    }
                }
            }
        }
        OnSetUp();
        OnUpdate();
    }

    void SetField(int row, int col)
    {
        _field = new Sell[row, col];

        for (int i = 0; i < _field.GetLength(0); i++)
        {
            for (int k = 0; k < _field.GetLength(1); k++)
            {
                _field[i, k] = new Sell();
            }
        }
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
                    if (_field[i, l].State == SellState.Nomal)
                    {
                        if (_field[i, l].Bomb)
                        {
                            sb.Append("b");
                        }
                        else
                        {
                            sb.Append(_field[i, l].Number);
                        }
                    }
                    else if (_field[i, l].State == SellState.Flag)
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
                    if (_field[i, l].State == SellState.Nomal)
                    {
                        if (_field[i, l].Bomb)
                        {
                            sb.Append("b");
                        }
                        else
                        {
                            sb.Append(_field[i, l].Number);
                        }
                    }
                    else if (_field[i, l].State == SellState.Flag)
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
        if (r >= 0 && r < _row && c >= 0 && c < _col)
        {
            if (_field[r, c].State == SellState.Nomal)
            {
                if (_field[r, c].Bomb)
                {
                    Explosion();
                }
                else if (_field[r, c].Number == 0)
                {
                    _field[r, c].State = SellState.Dug;
                    for (int i = r - 1 >= 0 ? r - 1 : 0; i < _field.GetLength(0) && i <= r + 1; i++)
                    {
                        for (int l = c - 1 >= 0 ? c - 1 : 0; l < _field.GetLength(1) && l <= c + 1; l++)
                        {
                            if (_field[i, l].State == SellState.Nomal)
                            {
                                Dig(i, l);
                            }
                        }
                    }
                }
                else
                {
                    _field[r, c].State = SellState.Dug;
                }
            }
        }
        OnUpdate();
    }

    public void Flag(int r, int c)
    {
        if (_field[r, c].State == SellState.Flag)
        {
            _field[r, c].State = SellState.Nomal;
        }
        else if (_field[r, c].State == SellState.Nomal)
        {
            _field[r, c].State = SellState.Flag;
        }
        OnUpdate();
    }

    void Explosion()
    {
        Debug.Log("bomb");
        OnExplosion();
        SetUp();
    }
}

/// <summary>
/// マスの情報を記録する
/// </summary>
public class Sell
{
    int number;
    SellState state;
    bool bomb;

    public int Number { get => number; set => number = value; }
    public SellState State { get => state; set => state = value; }
    public bool Bomb { get => bomb; set => bomb = value; }

    public Sell()
    {
        number = 0;
        state = SellState.Nomal;
        bomb = false;
    }
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
