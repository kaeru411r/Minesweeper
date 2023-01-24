using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    static public GameManager Instance;

    [SerializeField] ResultDisplay _resultDisplay;



    /// <summary>ゲームの進行状態</summary>
    bool _isPlay = false;
    /// <summary>ゲームの進行時間</summary>
    float _playTime = 0;


    /// <summary>ゲームの進行状態</summary>
    public bool IsPlay { get => _isPlay; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        BoardManager.Instance.OnExplosion += Explosion;
        BoardManager.Instance.OnClear += Clear;
        BoardManager.Instance.CreateField();
        BoardManager.Instance.SetField();
    }

    private void Update()
    {
        if (_isPlay)
        {
            _playTime += Time.deltaTime;
        }
    }

    /// <summary>
    /// ゲームの開始
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    public void GameStart(Vector2Int point)
    {
        _playTime = 0;
        if (BoardManager.Instance.MineLaying(point))
        {
            _isPlay = true;
            BoardManager.Instance.Dig(point);
        }
    }

    public void Explosion()
    {
        if (_resultDisplay)
        {
            _resultDisplay.FailedDisplay();
        }
        GameEnd();
    }

    public void Clear()
    {
        if (_resultDisplay)
        {
            _resultDisplay.ClearDisplay();
        }
        GameEnd();

        Debug.Log($"ClearTime{_playTime}");
    }

    void GameEnd()
    {
        if (_resultDisplay)
        {
            _resultDisplay.OnDisplayEnd(() =>
            {
                _isPlay = false;
                FieldReset();
            }, true);
        }
    }

    void FieldReset()
    {
        BoardManager.Instance.ResetField();
        BoardManager.Instance.SetField();
    }
}
