using Assets.Scripts.Controller;
using Assets.Scripts.Enum;
using Assets.Scripts.Data;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace Assets.Scripts.Model
{
    public class GameFieldModel : MonoBehaviour
    {
        public delegate void OnEndTurn(List<GameObject> matches);
        public OnEndTurn onEndTurn;

        #region Variables

        public TileModel[,] GameField { get { return _tiles; } }

        private GameOptions _gameOptions;

        private TileModel[,] _tiles;

        private List<GameObject> _matches;

        private const float _SPAWN_DELAY = 0.1f;

        #endregion

        #region MonoBehaviour Methods

        private void Start()
        {
            _gameOptions = GameOptions.Instance;

            _tiles = new TileModel[_gameOptions.Rows, _gameOptions.Columns];

            _matches = new List<GameObject>();
        }

        #endregion

        #region Public Methods

        public void AddTile(TileModel tile, int row, int column)
        {
            _tiles[row, column] = tile;
        }

        public void RemoveTlie(int row, int column)
        {
            _tiles[row, column] = null;
        }

        public void ClearGameField()
        {
            int rows = transform.childCount;

            for (int i = 0; i < rows; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }

        public void SwapTiles(int firstTileRow, int firstTileColumn, int tileForSwapRow, int tileForSwapColumn, bool doItNotify)
        {
            TileModel currentTile = _tiles[firstTileRow, firstTileColumn];
            TileModel tileForSwap = _tiles[tileForSwapRow, tileForSwapColumn];
            Transform tempParent = tileForSwap.transform.parent;

            _tiles[firstTileRow, firstTileColumn] = tileForSwap;
            tileForSwap.ChangeTilePosition(firstTileRow, firstTileColumn, currentTile.transform.parent);

            _tiles[tileForSwapRow, tileForSwapColumn] = currentTile;
            currentTile.ChangeTilePosition(tileForSwapRow, tileForSwapColumn, tempParent);

            StartCoroutine(_tiles[firstTileRow, firstTileColumn].MoveTile(doItNotify));
            StartCoroutine(_tiles[tileForSwapRow, tileForSwapColumn].MoveTile(doItNotify));
        }

        public void EndSwapingTiles(List<TileModel> swapedTiles)
        {
            TileData tileData;

            for (int i = 0; i < swapedTiles.Count; i++)
            {
                tileData = swapedTiles[i].TileData;
                _matches.AddRange(FindMatchesAt(tileData.Row, tileData.Column));
            }

            if (_matches.Count == 0)
            {
                UndoSwaping(swapedTiles);
            }
            else
            {
                onEndTurn.Invoke(_matches);
            }
        }
        public IEnumerator CollapseGameField(List<Transform> rowsParents)
        {
            int rows = _gameOptions.Rows;
            int columns = _gameOptions.Columns;
            int? firstEmptyTileX;

            for (int y = 0; y < columns; y++)
            {
                firstEmptyTileX = null;

                for (int x = 0; x < rows; x++)
                {
                    if (_tiles[x, y] == null && !firstEmptyTileX.HasValue)
                    {
                        firstEmptyTileX = x;
                    }
                    else if (firstEmptyTileX.HasValue && _tiles[x, y] != null)
                    {
                        _tiles[x, y].ChangeTilePosition(firstEmptyTileX.Value, y, rowsParents[firstEmptyTileX.Value]);
                        StartCoroutine(_tiles[x, y].MoveTile());
                        _tiles[firstEmptyTileX.Value, y] = _tiles[x, y];

                        _tiles[x, y] = null;
                        firstEmptyTileX++;
                    }
                }
            }

            yield return new WaitForSeconds(_SPAWN_DELAY);
        }

        public bool IsHasEmptyTiles()
        {
            int rows = _gameOptions.Rows;
            int columns = _gameOptions.Columns;

            for (int y = 0; y < columns; y++)
            {
                for (int x = 0; x < rows; x++)
                {
                    if (_tiles[x, y] == null)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public List<GameObject> FindMatches()
        {
            List<GameObject> tempMathces = new List<GameObject>();

            for (int row = 0; row < _gameOptions.Rows; row++)
            {
                for (int column = 0; column < _gameOptions.Columns; column++)
                {
                    if (_tiles[row, column] != null && !tempMathces.Contains(_tiles[row, column].gameObject))
                    {
                        tempMathces.AddRange(FindMatchesAt(row, column));
                    }
                }
            }

            return tempMathces;
        }

        public List<GameObject> FindMatchesAt(int startRow, int startColumn)
        {
            List<GameObject> tempMathces = new List<GameObject>();

            tempMathces.AddRange(FindHorizontalMatches(startRow, startColumn));
            tempMathces.AddRange(FindVerticalMatches(startRow, startColumn));

            return tempMathces;

        }

        #endregion

        #region Private Methods

        private void UndoSwaping(List<TileModel> swapedTiles)
        {
            TileData firstTileData = swapedTiles[0].TileData;
            TileData secondTileData = swapedTiles[1].TileData;

            SwapTiles(firstTileData.Row, firstTileData.Column, secondTileData.Row, secondTileData.Column, false);
        }

        private List<GameObject> FindHorizontalMatches(int currentRow, int currentColumn)
        {
            List<GameObject> horizontalMatches = new List<GameObject>();

            GameObject curentTileObject = _tiles[currentRow, currentColumn].gameObject;

            TileTypeEnum currentTileType = _tiles[currentRow, currentColumn].TileData.tileType;

            int matchingCondition = _gameOptions.MatchingCondition;

            bool isThisSameTile = false;

            for (int i = 0; i < _gameOptions.Columns; i++)
            {
                if (_tiles[currentRow, i] == null)
                {
                    continue;
                }

                isThisSameTile = _tiles[currentRow, i].TileData.tileType == currentTileType;

                if (!isThisSameTile && (horizontalMatches.Count < matchingCondition || !horizontalMatches.Contains(curentTileObject)))
                {
                    horizontalMatches.Clear();
                    continue;
                }

                if (!isThisSameTile && horizontalMatches.Count >= matchingCondition)
                {
                    break;
                }

                if (isThisSameTile)
                {
                    horizontalMatches.Add(_tiles[currentRow, i].gameObject);
                }
            }

            if (horizontalMatches.Count < matchingCondition)
            {
                horizontalMatches.Clear();
            }

            return horizontalMatches;
        }

        private List<GameObject> FindVerticalMatches(int currentRow, int currentColumn)
        {
            List<GameObject> verticalMatches = new List<GameObject>();

            GameObject curentTileObject = _tiles[currentRow, currentColumn].gameObject;

            TileTypeEnum currentTileType = _tiles[currentRow, currentColumn].TileData.tileType;

            int matchingCondition = _gameOptions.MatchingCondition;
            bool isThisSameTile = false;

            for (int i = 0; i < _gameOptions.Rows; i++)
            {
                if (_tiles[i, currentColumn] == null)
                {
                    continue;
                }

                isThisSameTile = _tiles[i, currentColumn].TileData.tileType == currentTileType;

                if (!isThisSameTile && (verticalMatches.Count < matchingCondition || !verticalMatches.Contains(curentTileObject)))
                {
                    verticalMatches.Clear();
                    continue;
                }

                if (!isThisSameTile && verticalMatches.Count >= matchingCondition)
                {
                    break;
                }

                if (isThisSameTile)
                {
                    verticalMatches.Add(_tiles[i, currentColumn].gameObject);
                }
            }

            if (verticalMatches.Count < matchingCondition)
            {
                verticalMatches.Clear();
            }

            return verticalMatches;
        }

        #endregion
    }
}
