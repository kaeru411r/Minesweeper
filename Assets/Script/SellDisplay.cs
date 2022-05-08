using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SellDisplay : MonoBehaviour
{
    [SerializeField] int _row;
    [SerializeField] int _col;
    [SerializeField] BoardManager _boardManager;
    Sell _sell;
    TextMeshProUGUI _text;


    // Start is called before the first frame update
    void Start()
    {
        _text = GetComponentInChildren<TextMeshProUGUI>();
        _sell = _boardManager.Field[_row, _col];
        _boardManager.OnUpdate += Transcription;
    }


    private void Transcription()
    {
        if (_sell.State == SellState.Nomal)
        {
            if (_sell.Bomb)
            {
                _text.text = "bomb";
            }
            else
            {
                Debug.Log(_sell.ToString());
                _text.text = $"{_sell.Number}";
            }
        }
        else if (_sell.State == SellState.Flag)
        {
            _text.text = "flag";
        }
        else if (_sell.State == SellState.Dug)
        {
            _text.text = "dug";
        }
        else
        {
            _text.text = "?";
        }
    }
}
