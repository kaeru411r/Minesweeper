using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Cell : MonoBehaviour
{
    [SerializeField] int _number;
    [SerializeField] SellState _state;
    [SerializeField] bool _bomb;
    [SerializeField] int _row;
    [SerializeField] int _col;
    [SerializeField] float _scale;
    [SerializeField] Text _text;
    Image _image;
    RectTransform _tr;

    /// <summary>/// 周囲１マスの爆弾の数/// </summary>
    public int Number { get => _number; set => _number = value; }
    /// <summary>現在のマスの状態</summary>
    public SellState State { get => _state; set => _state = value; }
    /// <summary>爆弾の有無</summary>
    public bool Bomb { get => _bomb; set => _bomb = value; }
    public float Scale { get => _scale; set => _scale = value; }

    public override string ToString()
    {
        return $"{{{_state}, {_number}, {_bomb}}}";
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
        BoardManager.Instance.OnUpdate += Transcription;
    }

    public void SetScale(float size)
    {
        if (!_tr)
        {
            _tr = GetComponentInChildren<RectTransform>();
        }
        _tr.rect.Set(_tr.rect.x, _tr.rect.y, size, size);
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
        Debug.Log("1");
        if (_image == null)
        {
            _image = GetComponent<Image>();
        }
        if (_text == null) return;
        Debug.Log("2");
        if (_state == SellState.Dug)
        {
            _image.color = Color.white;
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
                    Debug.Log(ToString());
                    _text.text = $"{_number}";
                    _text.color = Color.blue;
                }
            }
        }
        else if (_state == SellState.Flag)
        {
            _text.text = "F";
            _text.color = Color.yellow;
            _image.color = Color.gray;
        }
        else
        {
            _text.text = "";
            _image.color = Color.gray;
        }
    }
}
