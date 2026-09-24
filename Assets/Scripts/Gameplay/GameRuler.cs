using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public enum BoardCheckType
{
    Horizontal = 0,
    Vertical = 1,
    DiagonalDown = 2,
    DiagonalUp = 3,
}

public enum BoardChipType
{
    None = 0,
    BlueChip = 1,
    RedChip = 2,
}

public class GameRuler : NetworkBehaviour
{
    public static GameRuler Instance { get; private set; }

    public BoardChipType[,] _gameMatrix;
    private bool[,] _visited;
    
    private Dictionary<int, int> _columnToRow;

    private int _rowCount = 6;
    private int _columnCount = 7;

    private int _currentTeam = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            return;
        }

        _gameMatrix = new BoardChipType[_rowCount, _columnCount];
        _columnToRow = new Dictionary<int, int>();
        _visited = new bool[_rowCount,_columnCount];

        for (int column = 0; column < _columnCount; column++)
        {
            _columnToRow[column] = 0;
        }
    }

    public bool PutChipOnColumn(int team, int column)
    {
        if (!IsServer)
        {
            return false;
        }

        if (column < 0 || column >= _columnCount)
        {
            return false;
        }

        if (team < 0 || team != _currentTeam)
        {
            return false;
        }

        int filled = _columnToRow[column];

        if (filled >= _rowCount)
        {
            return false;
        }

        int row = _rowCount - 1 - filled;

        _gameMatrix[row, column] = team == 0 ? BoardChipType.BlueChip : BoardChipType.RedChip;
        _columnToRow[column] = filled + 1;

        _currentTeam = 1 - _currentTeam;

        return true;
    }
    internal void CheckGameStatus()
    {
        for (int r = 0; r < _rowCount; r++)
        {
            for (int c = 0; c < _columnCount; c++)
            {
                BoardChipType cellChipType = _gameMatrix[r, c];

                if (cellChipType == BoardChipType.None)
                {
                    continue;
                }

                WalkInBoard(r,c,cellChipType);
                ClearBuffer();
            }
        }
    }

    private void WalkInBoard(int r, int c,BoardChipType chipType)
    {
        var checkTypes = (BoardCheckType[])Enum.GetValues(typeof(BoardCheckType));

        foreach (var checkType in checkTypes)
        {
            DFSInBoard(r, c, 0, chipType,checkType);
            ClearBuffer();
        }

    }

    private void ClearBuffer()
    {
        Array.Clear(_visited, 0, _visited.Length);
    }

    private void DFSInBoard(int r, int c, int conjunct, BoardChipType chipType,BoardCheckType checkType)
    {
        if (r < 0 || r >= _rowCount || c < 0 || c >= _columnCount || _visited[r, c] || _gameMatrix[r, c] != chipType)
        {
            return;
        }

        _visited[r, c] = true;

        if (_gameMatrix[r, c] == chipType)
        {
            conjunct++;
        }

        if (conjunct == 4)
        {
            Debug.Log("Match Finished!" + " Winner: " + chipType.ToString());
            return;
        }

        if (checkType is BoardCheckType.Horizontal)
        {
            DFSInBoard(r, c + 1, conjunct, chipType, BoardCheckType.Horizontal);
            DFSInBoard(r, c - 1, conjunct, chipType, BoardCheckType.Horizontal);
        }
        else if (checkType is BoardCheckType.Vertical)
        {
            DFSInBoard(r - 1, c, conjunct, chipType, BoardCheckType.Vertical);
            DFSInBoard(r + 1, c, conjunct, chipType, BoardCheckType.Vertical);
        }
        else if (checkType is BoardCheckType.DiagonalDown)
        {
            DFSInBoard(r + 1, c + 1, conjunct, chipType, BoardCheckType.DiagonalDown);
            DFSInBoard(r - 1, c - 1, conjunct, chipType, BoardCheckType.DiagonalDown);
        }
        else
        {
            DFSInBoard(r + 1, c - 1, conjunct, chipType, BoardCheckType.DiagonalUp);
            DFSInBoard(r - 1, c + 1, conjunct, chipType, BoardCheckType.DiagonalUp);
        }
    }
}
