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

    /// <summary>今立っている旗の数</summary>
    int _flagNum = 0;
    /// <summary>有効なセルの数</summary>
    int _activeCellNumber;

    RectTransform _tr;

    RectTransform Tr
    {
        get
        {
            if (!_tr)
            {
                _tr = GetComponent<RectTransform>();
                if (!_tr)
                {
                    Debug.LogError($"{_tr.GetType()}が{name}についていません");
                }
            }
            return _tr;
        }
    }

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
        OnUpdate += Log;
        OnUpdate += FieldCheck;
        SetField();
    }


    /// <summary>
    /// ログを出力する
    /// </summary>
    void Log()
    {
        Debug.Log(this);
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
            _field[row, col].SetBomb();
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
                float height = Tr.position.y + k * spase - (float)(_field.GetLength(1) - 1) / 2 * spase;
                float width = Tr.position.x + i * spase - (float)(_field.GetLength(0) - 1) / 2 * spase;
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
                    _activeCellNumber++;
                }
            }
        }
        CallOnSetUp();
        CallOnUpdate();
    }

    private void ResetField()
    {
        _activeCellNumber = 0;
        _flagNum = 0;
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
            _bomb = Mathf.Min(_bomb, _activeCellNumber);
            int failure = 0;
            int failureLimit = _activeCellNumber * 2;
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
                    bool b = false;
                    for (int k = 0; k < _field.GetLength(0); k++)
                    {
                        for (int m = 0; m < _field.GetLength(1); m++)
                        {
                            if (BombSet(k, m))
                            {
                                b = true;
                                break;
                            }
                        }
                        if (b) break;
                    }
                    if (!b)
                    {
                        Debug.LogWarning("爆弾の配置可能箇所が見つかりませんでした");
                        _bomb--;
                    }
                }
            }
            CallOnUpdate();
        }
        else
        {
            Debug.LogError("フィールドのセットアップがされていません");
        }
    }

    /// <summary>
    /// 指定セル及びその周囲1マスを除くフィールドに地雷を敷設する
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    /// <returns></returns>
    public bool MineLaying(Vector2Int area)
    {
        if (_field != null)
        {
            if (EreaCheck(area))
            {
                _bomb = Mathf.Min(_bomb, _field.Length);
                int failure = 0;
                int failureLimit = _field.Length * 2;

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
                        bool b = false;
                        for (int k = 0; k < _field.GetLength(0); k++)
                        {
                            for (int m = 0; m < _field.GetLength(1); m++)
                            {
                                if (BombSet(k, m))
                                {
                                    b = true;
                                    break;
                                }
                            }
                            if (b) break;
                        }
                        if (!b)
                        {
                            Debug.LogWarning("爆弾の配置可能箇所が見つかりませんでした");
                            _bomb--;
                        }
                    }
                }
                CallOnSetUp();
                CallOnUpdate();
                return true;
            }
        }
        else
        {
            Debug.LogError("フィールドのセットアップがされていません");
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
            Dictionary<Vector2Int, int> cells = new Dictionary<Vector2Int, int>();
            if (_field[row, col].Bomb)
            {
                Explosion();
                return;
            }
            else if (_field[row, col].Number == 0)
            {
                _field[row, col].State = CellState.WillDig;
                cells.Add(new Vector2Int(row, col), 0);
                AroundDig(row, col, 0, cells);
            }
            else
            {
                cells.Add(new Vector2Int(row, col), 0);
            }
            StartCoroutine(ChainDig(cells));
        }
    }


    /// <summary>
    /// フィールドを連鎖的に解放していく
    /// </summary>
    /// <param name="cells">keyが座標valueが道のり</param>
    /// <returns></returns>
    IEnumerator ChainDig(Dictionary<Vector2Int, int> cells)
    {
        _openTime = Mathf.Max(0, _openTime);
        float time = 0;

        foreach (var d in cells.OrderBy(c => c.Value))
        {
            while (d.Value * _openTime >= time)
            {
                yield return null;
                time += Time.deltaTime;
            }
            _field[d.Key.x, d.Key.y].Dig();
            CallOnUpdate();
        }
    }


    /// <summary>
    /// 指定セルの周囲を掘る
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    void AroundDig(int row, int col, int distance, Dictionary<Vector2Int, int> cells)
    {
        for (int i = row - 1; i <= row + 1; i++)
        {
            for (int k = col - 1; k <= col + 1; k++)
            {
                if (EreaCheck(i, k))
                {
                    Vector2Int point = new Vector2Int(i, k);
                    int dis = distance + Mathf.Abs(row - i) + Mathf.Abs(col - k);
                    if (_field[i, k].State == CellState.Nomal || _field[i, k].State == CellState.WillDig)
                    {
                        if (cells.ContainsKey(point))
                        {
                            if (cells[point] > dis)
                            {
                                cells[point] = dis;
                                if (_field[i, k].Number == 0)
                                {
                                    AroundDig(i, k, dis, cells);
                                }
                            }
                        }
                        else
                        {
                            cells.Add(point, dis);
                            if (_field[i, k].Number == 0)
                            {
                                AroundDig(i, k, dis, cells);
                            }
                        }
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
            if (_field[r, c].State == CellState.Nomal)
            {
                if (_flagNum < _bomb)
                {
                    _flagNum += _field[r, c].RiseFlag() ? 1 : 0;
                    Debug.Log(1);
                }
            }
            else if (_field[r, c].State == CellState.Flag)
            {
                _flagNum -= _field[r, c].RemovalFlag() ? 1 : 0;
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
        CallOnClear();
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


    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        for (int i = _field.GetLength(0) - 1; i >= 0; i--)
        {
            for (int k = 0; k < _field.GetLength(1); k++)
            {
                if (_field[k, i].Bomb)
                {
                    sb.Append("B");
                }
                else if (_field[k, i].State == CellState.Nomal)
                {
                    sb.Append("■");
                }
                else if (_field[k, i].State == CellState.Dug)
                {
                    if (_field[k, i].Number != 0)
                    {
                        sb.Append($"{_field[k, i].Number} ");
                    }
                    else
                    {
                        sb.Append("Ｘ");
                    }
                }
                else if (_field[k, i].State == CellState.Flag)
                {
                    sb.Append("Ｆ");
                }
                else if (_field[k, i].State == CellState.Null)
                {
                    sb.Append("□");
                }
            }
            sb.AppendLine();
        }
        sb.AppendLine(_field.Length.ToString());
        return sb.ToString();
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
