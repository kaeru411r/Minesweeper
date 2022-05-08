using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

/// <summary>
/// �Q�[���{�[�h�̊Ǘ�������N���X
/// </summary>
public class BoardManager : MonoBehaviour
{
    [Tooltip("���e�̐�")]
    [SerializeField] int _bomb;
    [Tooltip("�s��")]
    [SerializeField] int _row;
    [Tooltip("��")]
    [SerializeField] int _col;

    /// <summary>�{�[�h�S�̂�Sell���i�[</summary>
    Sell[,] _field;


    /// <summary>�{�[�h�S�̂�Sell���i�[</summary>
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

    /// <summary>���������ɌĂяo��</summary>
    public event Action OnSetUp;
    /// <summary>�X�V���ɌĂяo��</summary>
    public event Action OnUpdate;
    /// <summary>�����������ɌĂяo��</summary>
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
                    sb.Append("�Z");
                }
                else if(_field[i, l].State == SellState.Dug)
                {
                    if (_field[i, l].Number != 0)
                    {
                        sb.Append($"{_field[i, l].Number} ");
                    }
                    else
                    {
                        sb.Append("�~ ");
                    }
                }
                else if (_field[i, l].State == SellState.Flag)
                {
                    sb.Append("�e");
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
            //���e�̔z�u�\��ӏ��ɔ��e����������
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
                Debug.LogWarning("���e�̔z�u�\�ӏ���������܂���ł���");
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
            //���e�̔z�u�\��ӏ����w��Z���̎��͂P�}�X��������Ē��I
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
                Debug.LogWarning("���e�̔z�u�\�ӏ���������܂���ł���");
                _bomb--;
            }
        }
        CallOnSetUp();
        CallOnUpdate();
    }

    void BombSet(int row, int col)
    {
        _field[row, col].Bomb = true;
        //���e�̎��͂̃}�X�̔��e���{�P
        for (int l = row - 1 >= 0 ? row - 1 : 0; l < _field.GetLength(0) && l <= row + 1; l++)
        {
            for (int m = col - 1 >= 0 ? col - 1 : 0; m < _field.GetLength(1) && m <= col + 1; m++)
            {
                _field[l, m].Number++;
            }
        }
    }

    /// <summary>
    /// _field�z���row*col�ŏ�����
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
    /// �w�肵���Z�����@��
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
    /// �w�肵���Z���Ɋ��𗧂Ă�
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
    /// ����
    /// </summary>
    void Explosion()
    {
        Debug.Log("bomb");
        CallOnExplosion();
    }

    /// <summary>
    /// OnUpdate�̌Ăяo��
    /// </summary>
    void CallOnUpdate()
    {
        if (OnUpdate != null)
        {
            OnUpdate();
        }
    }

    /// <summary>
    /// OnSetUp�̌Ăяo��
    /// </summary>
    void CallOnSetUp()
    {
        if (OnSetUp != null)
        {
            OnSetUp();
        }
    }

    /// <summary>
    /// OnExplosion�̌Ăяo��
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
/// �}�X�̏����L�^����
/// </summary>
public class Sell
{
    int number;
    SellState state;
    bool bomb;

    /// <summary>/// ���͂P�}�X�̔��e�̐�/// </summary>
    public int Number { get => number; set => number = value; }
    /// <summary>���݂̃}�X�̏��</summary>
    public SellState State { get => state; set => state = value; }
    /// <summary>���e�̗L��</summary>
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
/// �}�X�̏��
/// </summary>
public enum SellState
{
    /// <summary>�������ĂȂ�</summary>
    Nomal,
    /// <summary>�@����</summary>
    Dug,
    /// <summary>���𗧂Ă�</summary>
    Flag,
}
