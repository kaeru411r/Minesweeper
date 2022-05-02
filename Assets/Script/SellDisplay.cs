using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SellDisplay : MonoBehaviour
{
    [SerializeField] int _row;
    [SerializeField] int _col;
    Sell _sell;
    BoardManager _boardManager = BoardManager.Instance;
    TextMeshProUGUI _text;


    // Start is called before the first frame update
    void Start()
    {
        _text = GetComponent<TextMeshProUGUI>();
        _sell = _boardManager.Field[_row, _col];
    }


    private void OnUpdate()
    {
        if (_sell.State == SellState.Nomal)
        {
            if (_sell.Bomb)
            {
                _text.text = "bomb";
            }
            else
            {
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
