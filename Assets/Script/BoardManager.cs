using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Linq;
using UnityEngine.EventSystems;

/// <summary>
/// ゲームボードの管理をするクラス
/// </summary>
public class BoardManager : MonoBehaviour, IPointerClickHandler
{
    static public BoardManager Instance;

    [Tooltip("爆弾の数")]
    [SerializeField] int _bomb;
    [Tooltip("フィールドの配置")]
    [SerializeField] Block[] _fieldSettings;
    [Tooltip("連鎖的に解放する時の一回の時間")]
    [SerializeField] float _openTime;
    [Tooltip("セルのプレハブ")]
    [SerializeField] Cell _cellPrefab;

    RectTransform _tr;

    /// <summary>ボード全体のCellを格納</summary>
    Cell[,] _field = new Cell[0, 0];


    /// <summary>ボード全体のCellを格納</summary>
    public Cell[,] Field
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
    /// <summary>クリア時に呼び出し</summary>
    public event Action OnClear;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        _tr = GetComponent<RectTransform>();
        OnUpdate += Log;
        OnUpdate += FieldCheck;
        SetField();

        //List<List<Vector2Int>> cells = new List<List<Vector2Int>>();
        //AroundDig(1, 1, cells);
        //StartCoroutine(ChainDig(cells));
    }

    void Log()
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < _field.GetLength(0); i++)
        {
            for (int k = 0; k < _field.GetLength(1); k++)
            {
                if (_field[i, k].Bomb)
                {
                    sb.Append("B");
                }
                else if (_field[i, k].State == CellState.Nomal)
                {
                    sb.Append("■");
                }
                else if (_field[i, k].State == CellState.Dug)
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
                else if (_field[i, k].State == CellState.Flag)
                {
                    sb.Append("Ｆ");
                }
                else if (_field[i, k].State == CellState.Null)
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
        return EreaCheck(point.x, point.y);
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
            if (_field[row, col].State != CellState.Null)
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
        if (!(_field[row, col].Bomb || _field[row, col].State == CellState.Null))
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
        ResetField();
        //xの両端を格納する配列のリスト
        List<int[]> x = new List<int[]>();
        //Array.ForEach(_fieldSettings, f => x.Add(new int[] { f.Area.x + f.Origin.x, f.Origin.x }));
        x.Add(new int[] { _fieldSettings[0].Area.x + _fieldSettings[0].Origin.x, _fieldSettings[0].Origin.x });
        //yの両端を格納する配列のリスト
        List<int[]> y = new List<int[]>();
        y.Add(new int[] { _fieldSettings[0].Area.y + _fieldSettings[0].Origin.y, _fieldSettings[0].Origin.y });
        //xとyそれぞれの最小値
        Vector2Int min = new Vector2Int(x[0].Min(), y[0].Min());
        //xとyそれぞれの最大値
        Vector2Int max = new Vector2Int(x[0].Max(), y[0].Max());
        //if(min.y < 0)
        //{
        //    min += new Vector2Int(min.y * -1, 0);
        //    max += new Vector2Int(min.y * -1, 0);
        //    //foreach()
        //}
        //else if (min.x < 0)
        //{
        //    min += new Vector2Int(0, min.x * -1);
        //    max += new Vector2Int(0, min.x * -1);
        //}
        //xとyそれぞれの最小、最大値を決める
        for (int i = 1; i < _fieldSettings.Length; i++)
        {
            x.Add(new int[] { _fieldSettings[i].Area.x + _fieldSettings[i].Origin.x, _fieldSettings[i].Origin.x });
            y.Add(new int[] { _fieldSettings[i].Area.y + _fieldSettings[i].Origin.y, _fieldSettings[i].Origin.y });
            if (y[i].Min() < min.y || y[i].Max() > max.y || x[i].Min() < min.x || x[i].Max() > max.x)
            {
                min = new Vector2Int(Mathf.Min(x[i].Min(), min.x), Mathf.Min(y[i].Min(), min.y));
                max = new Vector2Int(Mathf.Max(x[i].Max(), max.x), Mathf.Max(y[i].Max(), max.y));
            }
        }
        //fieldのサイズを決定し、配列を用意
        _field = new Cell[max.x - min.x, max.y - min.y];
        float spase = _cellPrefab.GetComponent<RectTransform>().rect.height;
        //配列内の各要素をインスタンス化
        for (int i = 0; i < _field.GetLength(0); i++)
        {
            for (int k = 0; k < _field.GetLength(1); k++)
            {
                float height = _tr.position.y + k * spase - (float)(_field.GetLength(1) - 1) / 2 * spase;
                float width = _tr.position.x - i * spase + (float)(_field.GetLength(0) - 1) / 2 * spase;
                _field[i, k] = Instantiate(_cellPrefab, new Vector2(width, height), Quaternion.identity, transform);
                _field[i, k].Position = new Vector2Int(i, k);
                _field[i, k].SetUp();
            }
        }

        //エリアに指定されているマスをNomalに
        for (int i = 0; i < _fieldSettings.Length; i++)
        {
            for (int k = x[i].Min() - min.x; k < x[i].Max() - min.x; k++)
            {
                for (int m = y[i].Min() - min.y; m < y[i].Max() - min.y; m++)
                {
                    _field[k, m].State = CellState.Nomal;
                }
            }
        }
        CallOnSetUp();
        CallOnUpdate();
    }

    private void ResetField()
    {
        for (int i = 0; i < _field.GetLength(0); i++)
        {
            for (int k = 0; k < _field.GetLength(1); k++)
            {
                _field[i, k].Delete();
                Destroy(_field[i, k].gameObject);
            }
        }
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
                if ((Mathf.Abs(r - area.x) <= 1 && Mathf.Abs(c - area.y) <= 1))
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
        Dig(point.x, point.y);
    }
    /// <summary>
    /// 指定したセルを掘る
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    public void Dig(int row, int col)
    {
        if (_field[row, col].State == CellState.Nomal)
        {
            List<List<Vector2Int>> cells = new List<List<Vector2Int>>();
            if (_field[row, col].Bomb)
            {
                Explosion();
                return;
            }
            else if (_field[row, col].Number == 0)
            {
                _field[row, col].State = CellState.WillDig;
                cells.Add(new List<Vector2Int>());
                cells[0].Add(new Vector2Int(row, col));
                AroundDig(row, col, cells);
            }
            else
            {
                cells.Add(new List<Vector2Int>());
                cells[0].Add(new Vector2Int(row, col));
            }
            StartCoroutine(ChainDig(cells));
        }
    }

    void DigErea(List<List<Vector2Int>> cells, Vector2Int point)
    {
        if (EreaCheck(point))
        {
            var o = cells[0][0];
            int d = (point.x - o.x) + (point.y + o.y);
            cells[d].Add(point);
        }
    }

    /// <summary>
    /// フィールドを連鎖的に解放していく
    /// </summary>
    /// <param name="_openTime"></param>
    /// <param name="cells"></param>
    /// <returns></returns>
    IEnumerator ChainDig(List<List<Vector2Int>> cells)
    {
        _openTime = 0 > _openTime ? 0 : _openTime;
        int i = 0;
        float time = 0;

        if (_openTime > 0)
        {
            for (; i < cells.Count;)
            {
                for (; i <= time / _openTime && i < cells.Count; i++)
                {
                    foreach (var v in cells[i])
                    {
                        _field[v.x, v.y].State = CellState.Dug;
                    }
                }
                CallOnUpdate();
                yield return null;
                time += Time.deltaTime;
            }
        }
        else
        {
            foreach (var v in cells)
            {
                foreach (var v2 in v)
                {
                    _field[v2.x, v2.y].State = CellState.Dug;
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
    void AroundDig(int row, int col, List<List<Vector2Int>> cells)
    {
        if (cells[0].Count == 0) return;
        for (int i = row - 1; i <= row + 1; i++)
        {
            for (int k = col - 1; k <= col + 1; k++)
            {
                if (EreaCheck(i, k) && _field[i, k].State == CellState.Nomal)
                {
                    _field[i, k].State = CellState.WillDig;
                    int dis = Mathf.Abs((i - cells[0][0].x) + (k - cells[0][0].y));
                    while (dis > cells.Count - 1)
                    {
                        cells.Add(new List<Vector2Int>());
                    }
                    cells[dis].Add(new Vector2Int(i, k));
                    if (_field[i, k].Number == 0)
                    {
                        AroundDig(i, k, cells);
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
            if (_field[r, c].State == CellState.Flag)
            {
                _field[r, c].State = CellState.Nomal;
            }
            else if (_field[r, c].State == CellState.Nomal)
            {
                _field[r, c].State = CellState.Flag;
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
        Flag(point.x, point.y);
    }

    /// <summary>
    /// 爆発
    /// </summary>
    void Explosion()
    {
        Debug.Log("bomb");
        CallOnExplosion();
    }

    void Clear()
    {
        Debug.Log("clear");
        OnClear();
    }

    /// <summary>
    /// OnUpdateの呼び出し
    /// </summary>
    void CallOnUpdate()
    {
        if (OnUpdate != null)
        {
            OnUpdate.Invoke();
        }
    }

    /// <summary>
    /// OnSetUpの呼び出し
    /// </summary>
    void CallOnSetUp()
    {
        if (OnSetUp != null)
        {
            OnSetUp.Invoke();
        }
    }

    /// <summary>
    /// OnExplosionの呼び出し
    /// </summary>
    void CallOnExplosion()
    {
        if (OnExplosion != null)
        {
            OnExplosion.Invoke();
        }
    }

    /// <summary>
    /// OnExplosionの呼び出し
    /// </summary>
    void CallOnClear()
    {
        if (OnClear != null)
        {
            OnClear.Invoke();
        }
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        Cell cell = eventData.pointerCurrentRaycast.gameObject.transform.parent.GetComponent<Cell>();
        if (cell && cell.State != CellState.Null)
        {
            foreach (var f in _field)
            {
                if (f == cell)
                {
                    if (GameManager.Instance.IsPlay)
                    {
                        if (eventData.button == PointerEventData.InputButton.Right)
                        {
                            Flag(cell.Position);
                        }
                        if (eventData.button == PointerEventData.InputButton.Left)
                        {
                            Dig(cell.Position);
                        }
                    }
                    else
                    {
                        Debug.Log(2);
                        GameManager.Instance.GameStart(cell.Position);
                    }
                }
            }
        }
    }

    void FieldCheck()
    {
        foreach (var f in _field)
        {
            if (f.State == CellState.Nomal || f.State == CellState.WillDig)
            {
                return;
            }
        }
        Clear();
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
public enum CellState
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
