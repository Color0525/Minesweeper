using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Minesweeper : MonoBehaviour
{
    [SerializeField] Cell _cellPrefab = null;
    [SerializeField] GridLayoutGroup _cellLayoutGroup = null;
    [SerializeField] int _rows = 5;
    [SerializeField] int _columns = 5;
    [ContextMenuItem("推奨割合（Cell:Mine=6:1）", "RecommendedRatio")]
    [SerializeField] int _mineCount = 10;

    [SerializeField] Text _gameStatusText = null;
    [SerializeField] Text _timeText = null;
    [SerializeField] Text _flagCountText = null;

    /// <summary>ランキングシステムのプレハブ</summary>
    [SerializeField] GameObject m_rankingPrefab;

    Cell[,] _cells;
    bool _isPlaying = false;
    public bool IsPlaying { get { return _isPlaying; } }
    float _time = 0;
    int _flagCount = 0;

    private void OnValidate()
    {
        _rows = Mathf.Max(_rows, 1);
        _columns = Mathf.Max(_columns, 1);
        if (_columns < _rows)
        {
            _cellLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            _cellLayoutGroup.constraintCount = _columns;
        }
        else
        {
            _cellLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedRowCount;
            _cellLayoutGroup.constraintCount = _rows;
        }

        _mineCount = Mathf.Clamp(_mineCount, 0, _rows * _columns - 9);
    }

    void Start()
    {
        _cells = new Cell[_rows, _columns];
        for (int r = 0; r < _rows; r++)
        {
            for (int c = 0; c < _columns; c++)
            {
                Cell cell = Instantiate(_cellPrefab, _cellLayoutGroup.transform);
                _cells[r, c] = cell;
                //cell.Opened += 開ける関数(cell);//+=であれば上書きされない//eventにしておくとAddが呼ばれ、+=しかできない
            }
        }
    }

    void Update()
    {
        if (_isPlaying)
        {
            _time += Time.deltaTime;
            int minute = (int)_time / 60;
            float second = _time % 60;
            _timeText.text = $"{minute.ToString("00")}.{second.ToString("00.00")}";
        }
    }

    public void Play()
    {
        foreach (var cell in _cells)
        {
            Destroy(cell.gameObject);
        }
        Start();
        _gameStatusText.gameObject.SetActive(false);
        _time = 0;
        _timeText.text = "00.00.00";
        _flagCount = _mineCount;
        _flagCountText.text = $"Flag : {_flagCount}";
        _isPlaying = false;
    }

    public void SetMine(Cell firstSelectCell)
    {
        int selectR;
        int selectC;
        GetCellsIndex(firstSelectCell, out selectR, out selectC);

        for (int i = 0; i < _mineCount; i++)
        {
            int randomR = Random.Range(0, _rows);
            int randomC = Random.Range(0, _columns);
            if (_cells[randomR, randomC].CellState != CellState.Mine 
                && (randomR < selectR - 1 || selectR + 1 < randomR || randomC < selectC - 1 || selectC + 1 < randomC))
            {
                _cells[randomR, randomC].CellState = CellState.Mine;

                foreach (var neighbor in GetNeighborCells(randomR, randomC))
                {
                    if (neighbor.CellState != CellState.Mine)
                    {
                        neighbor.CellState++;
                    }
                }
            }
            else
            {
                i--;
            }
        }
        _isPlaying = true;
    }

    public void BreakAndCheck(Cell selectCell)
    {
        int selectR;
        int sselectC;
        GetCellsIndex(selectCell, out selectR, out sselectC);

        if (selectCell.CoverState == CoverState.Cover)
        {
            selectCell.CoverState = CoverState.Open;

            if (selectCell.CellState == CellState.Mine)
            {
                _isPlaying = false;
                _gameStatusText.text = "Gameover";
                _gameStatusText.color = Color.red;
                _gameStatusText.gameObject.SetActive(true);
                return;
            }
            
            if (selectCell.CellState == CellState.None)
            {
                foreach (var neighbor in GetNeighborCells(selectR, sselectC))
                {
                    BreakAndCheck(neighbor);
                }
            }
        }

        if (selectCell.CellState != CellState.Mine && selectCell.CellState != CellState.None)
        {
            int neighborFlag = 0;
            foreach (var neighbor in GetNeighborCells(selectR, sselectC))
            {
                if (neighbor.CoverState == CoverState.Flag)
                {
                    neighborFlag++;
                }
            }
            if ((int)selectCell.CellState == neighborFlag)
            {
                foreach (var neighbor in GetNeighborCells(selectR, sselectC))
                {
                    if (neighbor.CoverState == CoverState.Cover)
                    {
                        BreakAndCheck(neighbor);
                    }
                }
            }
        }

        int openCount = 0;
        for (int r = 0; r < _rows; r++)
        {
            for (int c = 0; c < _columns; c++)
            {
                if (_cells[r, c].CoverState == CoverState.Open)
                {
                    openCount++;
                }
            }
        }
        if (openCount == _rows * _columns - _mineCount)
        {
            _isPlaying = false;
            _gameStatusText.text = "Clear";
            _gameStatusText.color = Color.green;
            _gameStatusText.gameObject.SetActive(true);

            // ランキングシステムを発動させる
            var ranking = Instantiate(m_rankingPrefab);
            ranking.GetComponent<RankingManager>().SetScoreOfCurrentPlay(_time);
        }
    }

    public void AddFlagCount(int value)
    {
        _flagCount += value;
        _flagCountText.text = $"Flag : {_flagCount}";
    }

    IEnumerable<Cell> GetNeighborCells(int row, int column)
    {
        //List<Cell> neighborCells = new List<Cell>();
        for (int i = -1; i <= 1; i++)
        {
            for (int k = -1; k <= 1; k++)
            {
                if (0 <= row + i && row + i < _rows && 0 <= column + k && column + k < _columns 
                    && _cells[row + i, column + k] != _cells[row, column])
                {
                    yield return _cells[row + i, column + k];
                    //neighborCells.Add(cells[row + k, column + m]);
                }
            }
        }
        //return neighborCells.ToArray();
    }

    void GetCellsIndex(Cell cell, out int row, out int column)
    {
        for (int i = 0; i < _rows; i++)
        {
            for (int k = 0; k < _columns; k++)
            {
                if (_cells[i, k] == cell)
                {
                    row = i;
                    column = k;
                    return;
                }
            }
        }
        row = -1;
        column = -1;
    }

    void RecommendedRatio()
    {
        _mineCount = _rows * _columns / 6;
    }


    //void Setup(int row = -1, int columns = -1)
    //{
    //    //Cell selectCell 
    //    //for (int r = 0; r < _rows; r++)
    //    //{
    //    //    for (int c = 0; c < _columns; c++)
    //    //    {
    //    //        cells[r, c] = Instantiate(_cellPrefab, _cellLayoutGroup.transform);
    //    //    }
    //    //}

    //    for (int i = 0; i < _mineCount; i++)
    //    {
    //        int r = Random.Range(0, _rows);
    //        int c = Random.Range(0, _columns);
    //        if (cells[r, c].CellState != CellState.Mine && cells[r, c] != //cells[row, columns])
    //        {
    //            cells[r, c].CellState = CellState.Mine;

    //            foreach (var near in GetNeighborCells(r, c))
    //            {
    //                if (near.CellState != CellState.Mine)
    //                {
    //                    near.CellState++;
    //                }
    //            }
    //            //for (int k = -1; k <= 1; k++)
    //            //{
    //            //    for (int m = -1; m <= 1; m++)
    //            //    {
    //            //        if (0 <= r + k && r + k < _rows && 0 <= c + m && c + m < _columns)
    //            //        {
    //            //            cells[r + k, c + m].CellState += cells[r + k, c + m].CellState != CellState.Mine ? 1 : 0;
    //            //        }
    //            //    }
    //            //}
    //        }
    //        else
    //        {
    //            i--;
    //        }
    //    }
    //}

    //void Resetup(int row = -1, int columns = -1)
    //{
    //    foreach (var cell in cells)
    //    {
    //        Destroy(cell.gameObject);
    //    }
    //    mineSet = true;
    //    Setup(row, columns);
    //}

    //public void ReBreak(Cell selectCell)
    //{
    //    int r;
    //    int c;
    //    GetCellsIndex(selectCell, out r, out c);
    //    mineSet = false;
    //    Resetup(r, c);
    //    cells[r, c].Break();
    //}

    //public void BreakNeighbor(Cell selectCell)
    //{
    //    int row;
    //    int column;
    //    GetCellsIndex(selectCell, out row, out column);

    //    foreach (var near in GetNeighborCells(row, column))
    //    {
    //        near.Break();
    //    }

    //    //for (int i = 0; i < _rows; i++)
    //    //{
    //    //    for (int k = 0; k < _columns; k++)
    //    //    {
    //    //        if (cells[i, k] == selectCell)
    //    //        {
    //    //            foreach (var near in GetNeighborCells(row, column))
    //    //            {
    //    //                near.Break();
    //    //            }
    //    //            //for (int m = -1; m <= 1; m++)
    //    //            //{
    //    //            //    for (int o = -1; o <= 1; o++)
    //    //            //    {
    //    //            //        if (0 <= i + m && i + m < _rows && 0 <= k + o && k + o < _columns && !(m == 0 && o == 0))
    //    //            //        {
    //    //            //            cells[i + m, k + o].Break();
    //    //            //        }
    //    //            //    }
    //    //            //}

    //    //            return;
    //    //        }
    //    //    }
    //    //}
    //}

    //public bool ClearCheck()
    //{
    //    int openCount = 0;
    //    for (int r = 0; r < _rows; r++)
    //    {
    //        for (int c = 0; c < _columns; c++)
    //        {
    //            openCount += cells[r, c].CoverState == CoverState.Open ? 1 : 0;
    //        }
    //    }
    //    return openCount == _rows * _columns - _mineCount ? true : false;
    //}


}
