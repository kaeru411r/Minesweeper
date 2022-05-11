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
    [Tooltip("フィールドの配置")]
    [SerializeField] Block[] _fieldSettings;
    [Tooltip("連鎖的に解放する時の一回の時間")]
    [SerializeField] float _openTime;

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

        List<List<Vector2Int>> sells = new List<List<Vector2Int>>();
        for (int i = 0; i < 10; i++)
        {
            sells.Add(new List<Vector2Int>());
            for (int k = 0; k < 10; k++)
            {
                sells[i].Add(new Vector2Int(i, k));
            }
        }
        StartCoroutine(ChainDig(sells));
    }

    void Log()
    {
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
    /// 指定座標がエリア内か否かを返す
    /// </summary>
    /// <param name="point"></param>
    /// <returns>指定座標がエリア内か否か</returns>
    bool EreaCheck(Vector2Int point)
    {
        return EreaCheck(point.y, point.x);
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
    /// 爆弾の設置を試みる
    /// </summary>
    /// <param name="point"></param>
    /// <returns>設置の成否</returns>
    bool BombSet(Vector2Int point)
    {
        return BombSet(point.y, point.x);
    }

    /// <summary>
    /// _field配列を_fieldSettingsに合わせて初期化
    /// </summary>
    public void SetField()
    {
        //xの両端を格納する配列のリスト
        List<int[]> x = new List<int[]>();
        x.Add(new int[] { _fieldSettings[0].Area.x + _fieldSettings[0].Origin.x, _fieldSettings[0].Origin.x });
        //yの両端を格納する配列のリスト
        List<int[]> y = new List<int[]>();
        y.Add(new int[] { _fieldSettings[0].Area.y + _fieldSettings[0].Origin.y, _fieldSettings[0].Origin.y });
        //xとyそれぞれの最小値
        Vector2Int min = new Vector2Int(x[0].Min(), y[0].Min());
        //xとyそれぞれの最大値
        Vector2Int max = new Vector2Int(x[0].Max(), y[0].Max());
        //xとyそれぞれの最小、最大値を決める
        for (int i = 1; i < _fieldSettings.Length; i++)
        {
            x.Add(new int[] { _fieldSettings[i].Area.x + _fieldSettings[i].Origin.x, _fieldSettings[i].Origin.x });
            y.Add(new int[] { _fieldSettings[i].Area.y + _fieldSettings[i].Origin.y, _fieldSettings[i].Origin.y });
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
        CallOnSetUp();
        CallOnUpdate();
    }

    /// <summary>
    /// 地雷敷設
    /// </summary>
    public void MineLaying()
    {
        if (_field != null)
        {
            _bomb = Mathf.Min(_bomb, _field.GetLength(0) * _field.GetLength(1));
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
        }
        CallOnUpdate();
    }

    /// <summary>
    /// 指定セル及びその周囲1マスを除くフィールドに地雷を敷設する
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    /// <returns></returns>
    public bool MineLaying(Vector2Int area)
    {
        if (EreaCheck(area) && _field != null)
        {
            _bomb = Mathf.Min(_bomb, _field.GetLength(0) * _field.GetLength(1));
            int failure = 0;
            int failureLimit = _field.GetLength(0) * _field.GetLength(1) * 2;

            for (int i = 0; i < _bomb; i++)
            {
                int r = UnityEngine.Random.Range(0, _field.GetLength(0));
                int c = UnityEngine.Random.Range(0, _field.GetLength(1));
                //爆弾の配置予定箇所が指定セルの周囲１マスだったら再抽選
                if ((Mathf.Abs(r - area.y) <= 1 && Mathf.Abs(c - area.x) <= 1))
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
    /// 指定したセルを掘る
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    public void Dig(Vector2Int point)
    {
        Dig(point.y, point.x);
    }
    /// <summary>
    /// 指定したセルを掘る
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    public void Dig(int row, int col)
    {
        if (_field[row, col].State == SellState.Nomal)
        {
            List<List<Vector2Int>> sells = new List<List<Vector2Int>>();
            if (_field[row, col].Bomb)
            {
                Explosion();
                return;
            }
            else if (_field[row, col].Number == 0)
            {
                _field[row, col].State = SellState.WillDig;
                sells.Add(new List<Vector2Int>());
                sells[0].Add(new Vector2Int(col, row));
                AroundDig(row, col);
            }
            else
            {
                sells.Add(new List<Vector2Int>());
                sells[0].Add(new Vector2Int(col, row));
            }
            ChainDig(sells);
        }
    }

    void DigErea(List<List<Vector2Int>> sells, Vector2Int point)
    {
        if (EreaCheck(point))
        {
            var o = sells[0][0];
            int d = (point.x - o.x) + (point.y + o.y);
            sells[d].Add(point);
        }
    }

    /// <summary>
    /// フィールドを連鎖的に解放していく
    /// </summary>
    /// <param name="_openTime"></param>
    /// <param name="sells"></param>
    /// <returns></returns>
    IEnumerator ChainDig(List<List<Vector2Int>> sells)
    {
        _openTime = 0 > _openTime ? 0 : _openTime;
        int i = 0;
        float time = 0;

        if (_openTime > 0)
        {
            for (; i < sells.Count;)
            {
                for (; i <= time / _openTime && i < sells.Count; i++)
                {
                    foreach (var v in sells[i])
                    {
                        _field[v.x, v.y].State = SellState.Dug;
                    }
                }
                CallOnUpdate();
                yield return null;
                time += Time.deltaTime;
            }
        }
        else
        {
            foreach (var v in sells)
            {
                foreach (var v2 in v)
                {
                    _field[v2.x, v2.y].State = SellState.Dug;
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
                if (EreaCheck(i, k) && Field[i, k].State == SellState.Nomal)
                {
                    _field[i, k].State = SellState.WillDig;
                    AroundDig(i, k);
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
    /// 指定したセルに旗を立てる
    /// </summary>
    /// <param name="point"></param>
    public void Flag(Vector2Int point)
    {
        Flag(point.y, point.x);
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
    /// <summary>領域の広さ</summary>
    public Vector2Int Area;
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
    /// <summary>掘る予定</summary>
    WillDig,
    /// <summary>無効なセル</summary>
    Null,
}
