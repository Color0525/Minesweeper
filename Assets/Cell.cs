using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum CellState
{
    None = 0,
    One = 1,
    Two = 2,
    Three = 3,
    Four = 4,
    Five = 5,
    Six = 6,
    Seven = 7,
    Eight = 8,

    Mine = -1,
}
public enum CoverState
{
    Cover = 0,  
    Open = 1,
    Flag = 2,
}

public class Cell : MonoBehaviour
{
    //[SerializeField] Image _cellImage = null;
    [SerializeField] Text _cellText = null;
    [SerializeField] CellState _cellState = CellState.None;
    public CellState CellState
    {
        get { return _cellState; }
        set 
        { 
            _cellState = value;
            OnCellStateChanged();
        }
    }

    [SerializeField] Image _coverImage = null;
    [SerializeField] Text _coverText = null;
    [SerializeField] CoverState _coverState = CoverState.Cover;
    public CoverState CoverState
    {
        get { return _coverState; }
        set
        {
            _coverState = value;
            OnCoverStateChanged();
        }
    }

    //public event Action<Cell> Opened;
    //↓の内容を↑に省略可能
    //Action<Cell> _opened;//デリゲート
    //public event Action<Cell> Opened //公開イベント
    //{
    //    add { _opened += value; }
    //    remove { _opened -= value; } 
    //}

    private void OnValidate()
    {
        OnCellStateChanged();
        OnCoverStateChanged();
    }

    private void OnCellStateChanged()
    {
        if (_cellText == null){ return; }

        if (_cellState == CellState.None)
        {
            _cellText.text = "";
        }
        else if (_cellState == CellState.Mine)
        {

            _cellText.text = "X";
            _cellText.color = Color.red;
        }
        else
        {
            _cellText.text = ((int)_cellState).ToString();
            _cellText.color = Color.blue;
        }
    }

    private void OnCoverStateChanged()
    {
        if (_coverText == null) { return; }

        if (_coverState == CoverState.Cover)
        {
            _coverImage.color = Color.gray;
            _coverText.text = "";
        }
        else if (_coverState == CoverState.Flag)
        {
            _coverImage.color = Color.yellow;
            _coverText.text = "F";
            _coverText.color = Color.red;
        }
        else
        {
            _coverImage.color = Color.clear;
            _coverText.text = "";
        }
    }

    public void BreakOrFlag()
    {
        //Opened?.Invoke(this);

        Minesweeper ms = FindObjectOfType<Minesweeper>();

        if (Input.GetButtonUp("Fire1"))
        {
            if (!ms.IsPlaying)
            {
                ms.SetMine(this);
            }

            ms.BreakAndCheck(this);
        }
        else if (Input.GetButtonUp("Fire2"))
        {
            if (_coverState == CoverState.Cover)
            {
                CoverState = CoverState.Flag;
                ms.AddFlagCount(-1);
            }
            else if (_coverState == CoverState.Flag)
            {
                CoverState = CoverState.Cover;
                ms.AddFlagCount(1);
            }
        }
    }
}
