using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Linq;

/// <summary>
/// ゲームボードの管理をするクラス
/// </summary>
public class BoardManager : MonoBehaviour
{
    [Tooltip("爆弾の数")]
    [SerializeField] int _bomb;
    [SerializeField] Block[] _fieldSettings;

    /// <summary>ボード全体のSellを格納</summary>
    Sell[,] _field;


    /// <summary>ボード全体のSellを格納</summary>
    public Sell[,] Field
    {
        get
        {
            if (_field == null)
            {
                SetField();
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
        SetField();
    }

    int count = 0;
    void Log()
    {
        Debug.Log($"Disp{++count}");
        Debug.Log($"{_field.GetLength(0)}, {_field.GetLength(1)}");
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < _field.GetLength(0); i++)
        {
            for (int k = 0; k < _field.GetLength(1); k++)
            {
                if (_field[i, k].State == SellState.Nomal)
                {
                    sb.Append("■");
                }
                else if (_field[i, k].State == SellState.Dug)
                {
                    if (_field[i, k].Number != 0)
                    {
                        sb.Append($"{_field[i, k].Number} ");
                    }
                    else
                    {
                        sb.Append("Ｘ");
                    }
                }
                else if (_field[i, k].State == SellState.Flag)
                {
                    sb.Append("Ｆ");
                }
                else if (_field[i, k].State == SellState.Null)
                {
                    sb.Append("□");
                }
            }
            sb.AppendLine();
        }
        Debug.Log(sb);
    }

    /// <summary>
    /// フィールドのセットアップ
    /// </summary>
    public void SetUp()
    {
        _bomb = Mathf.Min(_bomb, _field.GetLength(0) * _field.GetLength(1));
        SetField();
        int failure = 0;
        int failureLimit = _field.GetLength(0) * _field.GetLength(1) * 2;

        for (int i = 0; i < _bomb; i++)
        {
            int r = UnityEngine.Random.Range(0, _field.GetLength(0));
            int c = UnityEngine.Random.Range(0, _field.GetLength(1));
            //爆弾の配置予定箇所に爆弾があったら
            if (!BombSet(r, c))
            {
                i--;
                failure++;
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

    /// <summary>
    /// フィールドのセットアップ
    /// 指定した座標の周囲1マス以外に爆弾を指定数設置する
    /// 指定箇所がエリア外ならフィールドのセットアップのみをしてfalseを返す
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    /// <returns>指定座標がエリア内かどうか</returns>
    public bool SetUp(int row, int col)
    {
        count = 0;
        _bomb = Mathf.Min(_bomb, _field.GetLength(0) * _field.GetLength(1));
        SetField();

        if (EreaCheck(row, col))
        {

            int failure = 0;
            int failureLimit = _field.GetLength(0) * _field.GetLength(1) * 2;

            for (int i = 0; i < _bomb; i++)
            {
                int r = UnityEngine.Random.Range(0, _field.GetLength(0));
                int c = UnityEngine.Random.Range(0, _field.GetLength(1));
                Debug.Log($"{r}, {row}, {Mathf.Abs(r - row) <= 1}, {c}, {col}, {Mathf.Abs(c - col) <= 1}");
                //爆弾の配置予定箇所が指定セルの周囲１マスだったら再抽選
                if ((Mathf.Abs(r - row) <= 1 && Mathf.Abs(c - col) <= 1))
                {
                    i--;
                    failure++;
                }
                else
                {
                    if (!BombSet(r, c))
                    {
                        i--;
                        failure++;
                    }
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
            return true;
        }
        return false;
    }

    /// <summary>
    /// 指定座標がエリア内か否かを返す
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    /// <returns>指定座標がエリア内か否か</returns>
    bool EreaCheck(int row, int col)
    {
        if (row >= 0 && row < _field.GetLength(0) && col >= 0 && col < _field.GetLength(1))
        {
            if (_field[row, col].State != SellState.Null)
            {
                return true;
            }
            return false;
        }
        return false;
    }

    /// <summary>
    /// 爆弾の設置を試みる
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    /// <returns>設置の成否</returns>
    bool BombSet(int row, int col)
    {
        Debug.Log($"{row}, {col}, {_field[row, col]}");
        if (!(_field[row, col].Bomb || _field[row, col].State == SellState.Null))
        {
            _field[row, col].Bomb = true;
            //爆弾の周囲のマスの爆弾数＋１
            for (int i = row - 1 >= 0 ? row - 1 : 0; i < _field.GetLength(0) && i <= row + 1; i++)
            {
                for (int k = col - 1 >= 0 ? col - 1 : 0; k < _field.GetLength(1) && k <= col + 1; k++)
                {
                    _field[i, k].Number++;
                }
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// _field配列をrow*colで初期化
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    public void SetField()
    {
        //xの両端を格納する配列のリスト
        List<int[]> x = new List<int[]>();
        x.Add(new int[] { _fieldSettings[0].Col + _fieldSettings[0].Origin.x, _fieldSettings[0].Origin.x });
        //yの両端を格納する配列のリスト
        List<int[]> y = new List<int[]>();
        y.Add(new int[] { _fieldSettings[0].Row + _fieldSettings[0].Origin.y, _fieldSettings[0].Origin.y });
        //xとyそれぞれの最小値
        Vector2Int min = new Vector2Int(x[0].Min(), y[0].Min());
        //xとyそれぞれの最大値
        Vector2Int max = new Vector2Int(x[0].Max(), y[0].Max());
        //xとyそれぞれの最小、最大値を決める
        for (int i = 1; i < _fieldSettings.Length; i++)
        {
            x.Add(new int[] { _fieldSettings[i].Col + _fieldSettings[i].Origin.x, _fieldSettings[i].Origin.x });
            y.Add(new int[] { _fieldSettings[i].Row + _fieldSettings[i].Origin.y, _fieldSettings[i].Origin.y });
            if (x[i].Min() < min.x || x[i].Max() > max.x || y[i].Min() < min.y || y[i].Max() > max.y)
            {
                min = new Vector2Int(Mathf.Min(x[i].Min(), min.x), Mathf.Min(y[i].Min(), min.y));
                max = new Vector2Int(Mathf.Max(x[i].Max(), max.x), Mathf.Max(y[i].Max(), max.y));
            }
        }
        //fieldのサイズを決定し、配列を用意
        _field = new Sell[max.y - min.y + 1, max.x - min.x + 1];

        //配列内の各要素をインスタンス化
        for (int i = 0; i < _field.GetLength(0); i++)
        {
            for (int k = 0; k < _field.GetLength(1); k++)
            {
                Debug.Log($"{i}, {k}");
                _field[i, k] = new Sell();
            }
        }

        //エリアに指定されているマスをNomalに
        for (int i = 0; i < _fieldSettings.Length; i++)
        {
            for (int k = x[i].Min() - min.y; k <= x[i].Max() - min.y; k++)
            {
                for (int m = y[i].Min() - min.x; m <= y[i].Max() - min.x; m++)
                {
                    _field[k, m].State = SellState.Nomal;
                }
            }
        }
    }


    /// <summary>
    /// 指定したセルを掘る
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    public void Dig(int row, int col)
    {
        Debug.Log("dig");
        if (EreaCheck(row, col))
        {
            Debug.Log($"{row} {col}");
            if (_field[row, col].State == SellState.Nomal)
            {
                if (_field[row, col].Bomb)
                {
                    Explosion();
                }
                else if (_field[row, col].Number == 0)
                {
                    _field[row, col].State = SellState.Dug;
                    AroundDig(row, col);
                }
                else
                {
                    _field[row, col].State = SellState.Dug;
                }
            }
            CallOnUpdate();
        }
    }

    /// <summary>
    /// 指定セルの周囲を掘る
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    void AroundDig(int row, int col)
    {
        for (int i = row - 1; i <= row + 1; i++)
        {
            for (int k = col - 1; k <= col + 1; k++)
            {
                if (EreaCheck(i, k))
                {
                    if (Field[i, k].State == SellState.Nomal)
                    {
                        Dig(i, k);
                    }
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
        if (EreaCheck(r, c))
        {
            Debug.Log(1);
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
        state = SellState.Null;
        bomb = false;
    }

    public override string ToString()
    {
        return $"{{{state}, {number}, {bomb}}}";
    }
}

[Serializable]
public struct Block
{
    /// <summary>原点</summary>
    public Vector2Int Origin;
    /// <summary>行数</summary>
    public int Row;
    /// <summary>列数</summary>
    public int Col;
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
    /// <summary>無効なセル</summary>
    Null,
}
