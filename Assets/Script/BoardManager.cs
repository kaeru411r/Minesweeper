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
    [Tooltip("爆弾の数")]
    [SerializeField] int _bomb;
    [Tooltip("行数")]
    [SerializeField] int _row;
    [Tooltip("列数")]
    [SerializeField] int _col;

    /// <summary>ボード全体のSellを格納</summary>
    Sell[,] _field;


    /// <summary>ボード全体のSellを格納</summary>
    public Sell[,] Field 
    {
        get
        {
            if(_field == null)
            {
                SetField(_row, _col);
            }
            return _field;
        }
    }

    /// <summary>初期化時に呼び出し</summary>
    public event Action OnSetUp;
    /// <summary>更新時に呼び出し</summary>
    public event Action OnUpdate;
    /// <summary>爆発した時に呼び出し</summary>
    public event Action OnExplosion;

    // Start is called before the first frame update
    void Start()
    {
        OnUpdate += Log;
    }

    void Log()
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < _row; i++)
        {
            for (int l = 0, m = _col; l < m; l++)
            {
                if (_field[i, l].State == SellState.Nomal)
                {
                    sb.Append("〇");
                }
                else if(_field[i, l].State == SellState.Dug)
                {
                    if (_field[i, l].Number != 0)
                    {
                        sb.Append($"{_field[i, l].Number} ");
                    }
                    else
                    {
                        sb.Append("× ");
                    }
                }
                else if (_field[i, l].State == SellState.Flag)
                {
                    sb.Append("Ｆ");
                }
            }
            sb.AppendLine();
        }
        Debug.Log(sb);
    }

    public void SetUp()
    {
        _bomb = _bomb <= _row * _col ? _bomb : _row * _col;
        SetField(_row, _col);
        int failure = 0;
        int failureLimit = _row * _col * 2;

        for (int i = 0; i < _bomb; i++)
        {
            int r = UnityEngine.Random.Range(0, _row - 1);
            int c = UnityEngine.Random.Range(0, _col - 1);
            //爆弾の配置予定箇所に爆弾があったら
            if (_field[r, c].Bomb)
            {
                i--;
                failure++;
            }
            else
            {
                BombSet(r, c);
            }

            if (failure > failureLimit)
            {
                failure = 0;
                Debug.LogWarning("爆弾の配置可能箇所が見つかりませんでした");
                _bomb--;
            }
        }

        CallOnSetUp();
        CallOnUpdate();
    }

    public void SetUp(int row, int col)
    {
        _bomb = _bomb <= _row * _col ? _bomb : _row * _col;
        SetField(_row, _col);
        int failure = 0;
        int failureLimit = _row * _col * 2;

        for (int i = 0; i < _bomb; i++)
        {
            int r = UnityEngine.Random.Range(0, _row);
            int c = UnityEngine.Random.Range(0, _col);
            Debug.Log($"{r}, {row}, {Mathf.Abs(r - row) <= 1}, {c}, {col}, {Mathf.Abs(c - col) <= 1}");
            //爆弾の配置予定箇所が指定セルの周囲１マスだったら再抽選
            if (_field[r, c].Bomb || Mathf.Abs(r - row) <= 1 && Mathf.Abs(c - col) <= 1)
            {
                i--;
                failure++;
            }
            else
            {
                BombSet(r, c);
            }

            if (failure > failureLimit)
            {
                failure = 0;
                Debug.LogWarning("爆弾の配置可能箇所が見つかりませんでした");
                _bomb--;
            }
        }
        CallOnSetUp();
        CallOnUpdate();
    }

    void BombSet(int row, int col)
    {
        _field[row, col].Bomb = true;
        //爆弾の周囲のマスの爆弾数＋１
        for (int l = row - 1 >= 0 ? row - 1 : 0; l < _field.GetLength(0) && l <= row + 1; l++)
        {
            for (int m = col - 1 >= 0 ? col - 1 : 0; m < _field.GetLength(1) && m <= col + 1; m++)
            {
                _field[l, m].Number++;
            }
        }
    }

    /// <summary>
    /// _field配列をrow*colで初期化
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
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


    /// <summary>
    /// 指定したセルを掘る
    /// </summary>
    /// <param name="r"></param>
    /// <param name="c"></param>
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
                    for (int i = r - 1; i <= r + 1; i++)
                    {
                        for (int l = c - 1; l <= c + 1; l++)
                        {
                            Dig(i, l);
                        }
                    }
                }
                else
                {
                    _field[r, c].State = SellState.Dug;
                    CallOnUpdate();
                }
            }
        }
    }

    /// <summary>
    /// 指定したセルに旗を立てる
    /// </summary>
    /// <param name="r"></param>
    /// <param name="c"></param>
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
        CallOnUpdate();
    }

    /// <summary>
    /// 爆発
    /// </summary>
    void Explosion()
    {
        Debug.Log("bomb");
        CallOnExplosion();
    }

    /// <summary>
    /// OnUpdateの呼び出し
    /// </summary>
    void CallOnUpdate()
    {
        if (OnUpdate != null)
        {
            OnUpdate();
        }
    }

    /// <summary>
    /// OnSetUpの呼び出し
    /// </summary>
    void CallOnSetUp()
    {
        if (OnSetUp != null)
        {
            OnSetUp();
        }
    }

    /// <summary>
    /// OnExplosionの呼び出し
    /// </summary>
    void CallOnExplosion()
    {
        if (OnExplosion != null)
        {
            OnExplosion();
        }
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

    /// <summary>/// 周囲１マスの爆弾の数/// </summary>
    public int Number { get => number; set => number = value; }
    /// <summary>現在のマスの状態</summary>
    public SellState State { get => state; set => state = value; }
    /// <summary>爆弾の有無</summary>
    public bool Bomb { get => bomb; set => bomb = value; }

    public Sell()
    {
        number = 0;
        state = SellState.Nomal;
        bomb = false;
    }

    public override string ToString()
    {
        return $"{{{state}, {number}, {bomb}}}";
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
