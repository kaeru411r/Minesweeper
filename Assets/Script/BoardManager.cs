using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Linq;

/// <summary>
/// �Q�[���{�[�h�̊Ǘ�������N���X
/// </summary>
public class BoardManager : MonoBehaviour
{
    [Tooltip("���e�̐�")]
    [SerializeField] int _bomb;
    [SerializeField] Block[] _fieldSettings;

    /// <summary>�{�[�h�S�̂�Sell���i�[</summary>
    Sell[,] _field;
    /// <summary>�{�[�h�̃T�C�Y</summary>
    Vector2Int _fieldSize;


    /// <summary>�{�[�h�S�̂�Sell���i�[</summary>
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
        SetField();
    }

    void Log()
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < _fieldSize.y; i++)
        {
            for (int l = 0, m = _fieldSize.x; l < m; l++)
            {
                if (_field[i, l].Bomb)
                {
                    sb.Append("��");
                }
                else if (_field[i, l].State == SellState.Nomal)
                {
                    sb.Append("��");
                }
                else if (_field[i, l].State == SellState.Dug)
                {
                    if (_field[i, l].Number != 0)
                    {
                        sb.Append($"{_field[i, l].Number} ");
                    }
                    else
                    {
                        sb.Append("�w");
                    }
                }
                else if (_field[i, l].State == SellState.Flag)
                {
                    sb.Append("�e");
                }
                else if (_field[i, l].State == SellState.Null)
                {
                    sb.Append("��");
                }
            }
            sb.AppendLine();
        }
        Debug.Log(sb);
    }

    /// <summary>
    /// �t�B�[���h�̃Z�b�g�A�b�v
    /// </summary>
    public void SetUp()
    {
        _bomb = _bomb <= _fieldSize.y * _fieldSize.x ? _bomb : _fieldSize.y * _fieldSize.x;
        SetField();
        int failure = 0;
        int failureLimit = _fieldSize.y * _fieldSize.x * 2;

        for (int i = 0; i < _bomb; i++)
        {
            int r = UnityEngine.Random.Range(0, _fieldSize.y - 1);
            int c = UnityEngine.Random.Range(0, _fieldSize.x - 1);
            //���e�̔z�u�\��ӏ��ɔ��e����������
            if (!BombSet(r, c))
            {
                i--;
                failure++;
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

    /// <summary>
    /// �t�B�[���h�̃Z�b�g�A�b�v
    /// �w�肵�����W�̎���1�}�X�ȊO�ɔ��e���w�萔�ݒu����
    /// �w��ӏ����G���A�O�Ȃ�t�B�[���h�̃Z�b�g�A�b�v�݂̂�����false��Ԃ�
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    /// <returns>�w����W���G���A�����ǂ���</returns>
    public bool SetUp(int row, int col)
    {
        _bomb = _bomb <= _fieldSize.y * _fieldSize.x ? _bomb : _fieldSize.y * _fieldSize.x;
        SetField();

        if (EreaCheck(row, col))
        {

            int failure = 0;
            int failureLimit = _fieldSize.y * _fieldSize.x * 2;

            for (int i = 0; i < _bomb; i++)
            {
                int r = UnityEngine.Random.Range(0, _fieldSize.y);
                int c = UnityEngine.Random.Range(0, _fieldSize.x);
                Debug.Log($"{r}, {row}, {Mathf.Abs(r - row) <= 1}, {c}, {col}, {Mathf.Abs(c - col) <= 1}");
                //���e�̔z�u�\��ӏ����w��Z���̎��͂P�}�X��������Ē��I
                if ((Mathf.Abs(r - row) <= 1 && Mathf.Abs(c - col) <= 1))
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
                    Debug.LogWarning("���e�̔z�u�\�ӏ���������܂���ł���");
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
    /// �w����W���G���A�����ۂ���Ԃ�
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    /// <returns>�w����W���G���A�����ۂ�</returns>
    bool EreaCheck(int row, int col)
    {
        if(row >= 0 && row <= _fieldSize.y && col >= 0 && col <= _fieldSize.x)
        {
            if(_field[row, col].State != SellState.Null)
            {
                return true;
            }
            return false;
        }
        return false;
    }

    /// <summary>
    /// ���e�̐ݒu�����݂�
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    /// <returns>�ݒu�̐���</returns>
    bool BombSet(int row, int col)
    {
        if (!(_field[row, col].Bomb || _field[row, col].State == SellState.Null))
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
            return true;
        }
        return false;
    }

    /// <summary>
    /// _field�z���row*col�ŏ�����
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    public void SetField()
    {
        List<int[]> x = new List<int[]>();
        x.Add(new int[] { _fieldSettings[0].Col + _fieldSettings[0].Origin.x, _fieldSettings[0].Origin.x });
        List<int[]> y = new List<int[]>();
        y.Add(new int[] { _fieldSettings[0].Row + _fieldSettings[0].Origin.y, _fieldSettings[0].Origin.y });
        Vector2Int min = new Vector2Int(x[0].Min(), y[0].Min());
        Vector2Int max = new Vector2Int(x[0].Max(), y[0].Max());
        for (int i = 1; i < _fieldSettings.Length; i++)
        {
            x.Add(new int[] { _fieldSettings[i].Col + _fieldSettings[i].Origin.x, _fieldSettings[i].Origin.x });
            y.Add(new int[] { _fieldSettings[i].Row + _fieldSettings[i].Origin.y, _fieldSettings[i].Origin.y });
            if (x[i].Min() < min.x || x[i].Max() > max.x || y[i].Min() < min.y || y[i].Max() > max.y)
            {
                min = new Vector2Int(Mathf.Min(x[i].Min(), min.x), Mathf.Min(y[i].Min(), min.y));
                max = new Vector2Int(Mathf.Max(x[i].Max(), max.x), Mathf.Max(y[i].Max(), max.y));
            }
        }

        _field = new Sell[max.y - min.y, max.x - min.x];
        _fieldSize = new Vector2Int(max.y - min.y, max.x - min.x);

        for (int i = 0; i < _field.GetLength(0); i++)
        {
            for (int k = 0; k < _field.GetLength(1); k++)
            {
                _field[i, k] = new Sell();
            }
        }
        Debug.Log($"{_field.GetLength(0)}, {_field.GetLength(1)}");
        for (int i = 0; i < _fieldSettings.Length; i++)
        {
            StringBuilder sb = new StringBuilder();
            for (int k = x[i].Min() + min.y; k <= x[i].Max() - min.y - 1; k++)
            {
                for (int n = y[i].Min() + min.x; n <= y[i].Max() - min.x - 1; n++)
                {
                    sb.AppendLine($"{k}, {n}");
                    _field[k, n].State = SellState.Nomal;
                }
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
        if (r >= 0 && r < _fieldSize.y && c >= 0 && c < _fieldSize.x)
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
    /// <summary>���_</summary>
    public Vector2Int Origin;
    /// <summary>�s��</summary>
    public int Row;
    /// <summary>��</summary>
    public int Col;
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
    /// <summary>�����ȃZ��</summary>
    Null,
}
