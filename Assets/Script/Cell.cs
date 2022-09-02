using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Cell : MonoBehaviour
{
    [SerializeField] int _number;
    [SerializeField] CellState _state;
    [SerializeField] bool _bomb;
    [SerializeField] float _scale;
    [SerializeField] Text _text;
    [SerializeField] Color _fieldColor = Color.gray;
    [SerializeField] Color _dugColor = Color.white;
    [SerializeField] Color _nullColor = Color.black;
    Vector2Int _position;
    Image _image;
    RectTransform _tr;

    /// <summary>/// 周囲１マスの爆弾の数/// </summary>
    public int Number
    {
        get => _number;
        set
        {
            _number = value;
            Transcription();
        }
    }
    /// <summary>現在のマスの状態</summary>
    public CellState State
    {
        get => _state;
        set
        {
            _state = value;
            Transcription();
        }
    }
    /// <summary>爆弾の有無</summary>
    public bool Bomb
    {
        get => _bomb;
        set
        {
            _bomb = value;
            Transcription();
        }
    }
    public float Scale
    {
        get => _scale;
        set
        {
            _scale = value;
            Transcription();
        }
    }

    public Vector2Int Position { get => _position; set => _position = value; }

    public override string ToString()
    {
        return $"{{{_state}, {_number}, {_bomb}}}";
    }

    public void SetUp()
    {
        _state = CellState.Null;
        _number = 0;
        _bomb = false;
        Start();
        Transcription();
        BoardManager.Instance.OnUpdate += Transcription;
    }



    // Start is called before the first frame update
    void Start()
    {
        _image = GetComponent<Image>();
        _text = GetComponentInChildren<Text>();
        _tr = GetComponentInChildren<RectTransform>();
        if (!_text)
        {
            Debug.LogError("子オブジェクトにTextが必要です");
        }
    }

    public void Delete()
    {
        BoardManager.Instance.OnUpdate -= Transcription;
    }

    public void SetScale(float size)
    {
        if (!_tr)
        {
            _tr = GetComponentInChildren<RectTransform>();
        }
        //_tr.rect.Set(_tr.rect.x, _tr.rect.y, size, size);
        _tr.sizeDelta = new Vector2(size, size);
        _scale = size;
        _text.fontSize = (int)_scale / 10;
    }

    private void OnValidate()
    {
        Transcription();
    }

    /// <summary>
    /// セルの表示を更新する
    /// </summary>
    private void Transcription()
    {
        if (_image == null)
        {
            _image = GetComponent<Image>();
        }
        if (_text == null) return;
        if (_state == CellState.Dug)
        {
            _image.color = _dugColor;
            if (_bomb)
            {
                _text.text = "X";
                _text.color = Color.red;
            }
            else
            {
                if (_number == 0)
                {
                    _text.text = "";
                }
                else
                {
                    _text.text = $"{_number}";
                    _text.color = Color.blue;
                }
            }
        }
        else if (_state == CellState.Nomal)
        {
            _text.text = "";
            _image.color = _fieldColor;
        }
        else if (_state == CellState.Flag)
        {
            _text.text = "F";
            _text.color = Color.yellow;
            _image.color =_fieldColor;
        }
        else if (_state == CellState.Null)
        {
            _text.text = "";
            _image.color = _nullColor;
        }
    }

    /// <summary>
    /// 掘る
    /// </summary>
    public void Dig()
    {
        _state = CellState.Dug;
        Transcription();
    }

    /// <summary>
    /// 旗を立てたり、おろしたり
    /// </summary>
    public bool Flag()
    {
        bool b = false;
        if(_state == CellState.Nomal)
        {
            _state = CellState.Flag;
            b = true;
        }
        else if(_state == CellState.Flag)
        {
            _state = CellState.Nomal;
            b = false;
        }
        Transcription();
        return b;
    }

    /// <summary>
    /// 旗を立てる
    /// </summary>
    /// <returns></returns>
    public bool RiseFlag()
    {
        if(_state == CellState.Flag || _state == CellState.Nomal)
        {
            _state = CellState.Flag;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 旗をはずす
    /// </summary>
    /// <returns></returns>
    public bool RemovalFlag()
    {
        if (_state == CellState.Flag || _state == CellState.Nomal)
        {
            _state = CellState.Nomal;
            return true;
        }
        return false;
    }


    public void SetBomb()
    {
        _bomb = true;
        Transcription();
    }
}
