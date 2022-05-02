using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

/// <summary>
/// �Q�[���{�[�h�̊Ǘ�������N���X
/// </summary>
public class BoardManager : SingletonMonoBehaviour<BoardManager>
{
    [Tooltip("���e�̐�")]
    [SerializeField] int _bomb;
    [Tooltip("�s��")]
    [SerializeField] int _row;
    [Tooltip("��")]
    [SerializeField] int _col;
    [SerializeField] int _row2;
    [SerializeField] int _col2;
    [SerializeField] bool _dig = false;
    [SerializeField] bool _flag = false;

    /// <summary>�{�[�h�S�̂�Sell���i�[</summary>
    Sell[,] _field;


    /// <summary>�{�[�h�S�̂�Sell���i�[</summary>
    public Sell[,] Field { get => _field; }

    /// <summary>���������ɌĂяo��</summary>
    public event Action OnSetUp;
    /// <summary>�X�V���ɌĂяo��</summary>
    public event Action OnUpdate;
    /// <summary>�����������ɌĂяo��</summary>
    public event Action OnExplosion;

    // Start is called before the first frame update
    void Start()
    {
        SetUp();
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < _row; i++)
        {
            for (int l = 0, m = _col; l < m; l++)
            {
                if (_field[i, l].Bomb)
                {
                    sb.Append("b");
                }
                else
                {
                    sb.Append(_field[i, l].Number);
                }
            }
            sb.AppendLine();
        }
        Debug.Log(sb);
    }

    public void SetUp()
    {
        SetField(_row, _col);
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < _row; i++)
        {
            for (int l = 0, m = _col; l < m; l++)
            {
                if (_field[i, l].Bomb)
                {
                    sb.Append("b");
                }
                else
                {
                    sb.Append(_field[i, l].Number);
                }
            }
            sb.AppendLine();
        }
        Debug.Log(sb);

        for (int i = 0; i < _bomb; i++)
        {
            int r = UnityEngine.Random.Range(0, _row - 1);
            int c = UnityEngine.Random.Range(0, _col - 1);
            _field[r, c].Bomb = true;
            //���e�̎��͂̃}�X�̔��e���{�P
            for (int l = r - 1 >= 0 ? r - 1 : 0; l < _field.GetLength(0) && l <= r + 1; l++)
            {
                for (int m = c - 1 >= 0 ? c - 1 : 0; m < _field.GetLength(1) && m <= c + 1; m++)
                {
                    _field[l, m].Number++;
                }
            }
        }
        CallOnSetUp();
    }

    public void SetUp(int row, int col)
    {
        SetField(_row, _col);

        for (int i = 0; i < _bomb; i++)
        {
            int r = UnityEngine.Random.Range(0, _row);
            int c = UnityEngine.Random.Range(0, _col);

            //���e�̔z�u�\��ӏ����z�u�֎~��������Ē��I
            if (_field[r, c].Bomb && r != row && c != col)
            {
                i--;
            }
            else
            {
                //���e�̎��͂̃}�X�̔��e���{�P
                _field[r, c].Bomb = true;
                for (int l = r - 1 >= 0 ? r - 1 : 0; l < _field.GetLength(0) && l <= r + 1; l++)
                {
                    for (int m = c - 1 >= 0 ? c - 1 : 0; m < _field.GetLength(1) && m <= c + 1; m++)
                    {
                        _field[l, m].Number++;
                    }
                }
            }
        }
        CallOnSetUp();
        CallOnUpdate();
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

    private void Update()
    {
        if (_dig)
        {
            _dig = false;
            Dig(_row2, _col2);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < _row; i++)
            {
                for (int l = 0, m = _col; l < m; l++)
                {
                    if (_field[i, l].State == SellState.Nomal)
                    {
                        if (_field[i, l].Bomb)
                        {
                            sb.Append("b");
                        }
                        else
                        {
                            sb.Append(_field[i, l].Number);
                        }
                    }
                    else if (_field[i, l].State == SellState.Flag)
                    {
                        sb.Append("f");
                    }
                    else
                    {
                        sb.Append("x");
                    }
                }
                sb.AppendLine();
            }
            Debug.Log(sb);
        }
        if (_flag)
        {
            _flag = false;
            Flag(_row2, _col2);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < _row; i++)
            {
                for (int l = 0, m = _col; l < m; l++)
                {
                    if (_field[i, l].State == SellState.Nomal)
                    {
                        if (_field[i, l].Bomb)
                        {
                            sb.Append("b");
                        }
                        else
                        {
                            sb.Append(_field[i, l].Number);
                        }
                    }
                    else if (_field[i, l].State == SellState.Flag)
                    {
                        sb.Append("f");
                    }
                    else
                    {
                        sb.Append("x");
                    }
                }
                sb.AppendLine();
            }
            Debug.Log(sb);
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
                    for (int i = r - 1 >= 0 ? r - 1 : 0; i < _field.GetLength(0) && i <= r + 1; i++)
                    {
                        for (int l = c - 1 >= 0 ? c - 1 : 0; l < _field.GetLength(1) && l <= c + 1; l++)
                        {
                            if (_field[i, l].State == SellState.Nomal)
                            {
                                Dig(i, l);
                            }
                        }
                    }
                }
                else
                {
                    _field[r, c].State = SellState.Dug;
                }
            }
        }
        CallOnUpdate();
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

    /// <summary>
    /// ����
    /// </summary>
    void Explosion()
    {
        Debug.Log("bomb");
        OnExplosion();
        SetUp();
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
