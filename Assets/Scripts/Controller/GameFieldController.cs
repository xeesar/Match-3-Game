using Assets.Scripts.Model;
using Assets.Scripts.Enum;
using Assets.Scripts.Data;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Controller
{
    public class GameFieldController : MonoBehaviour
    {
        [SerializeField] private GameFieldModel _gameField;
        [SerializeField] private TileCreator _tileCreator;

        private List<Transform> _rowsParents;
        private List<TileModel> _swapedTiles;

        private GameOptions _gameOptions;

        private void Start()
        {
            _gameField.onEndTurn = EndTurn;

            _gameOptions = GameOptions.Instance;

            _rowsParents = new List<Transform>();
            _swapedTiles = new List<TileModel>();
        }

        public void PrepareGameField()
        {
            int rows = _gameOptions.Rows;
            int columns = _gameOptions.Columns;

            for (int row = 0; row < rows; row++)
            {
                Transform currentRow = new GameObject("Row_" + (row + 1)).transform;

                for (int column = 0; column < columns; column++)
                {
                    SpawnTile(row, column, currentRow);
                }

                currentRow.SetParent(_gameField.transform);
                currentRow.transform.localPosition = new Vector2(0, row);
                _rowsParents.Add(currentRow);
            }
        }

        private void EndTurn(List<GameObject> matches)
        {
            DestroyMatches(matches);
            matches.Clear();

            StartCoroutine(ClearAndFillGameField());
        }

        private IEnumerator ClearAndFillGameField()
        {
            List<GameObject> matches = new List<GameObject>();
            do
            {
                DestroyMatches(matches);

                yield return StartCoroutine(SpawnUpperBlocks());

                matches = _gameField.FindMatches();

            } while (matches.Count > 0);
        }

        private IEnumerator SpawnUpperBlocks()
        {
            TileModel[,] tiles = _gameField.GameField;

            int upperRow = _gameOptions.Rows - 1;
            int columns = _gameOptions.Columns;

            while (_gameField.IsHasEmptyTiles())
            {
                for (int column = 0; column < columns; column++)
                {
                    if (tiles[upperRow, column] == null)
                    {
                        SpawnTile(upperRow, column, _rowsParents[upperRow]);
                    }
                }

                yield return StartCoroutine(_gameField.CollapseGameField(_rowsParents));
            }
        }


        private void SpawnTile(int row, int column, Transform newParent)
        {
            TileModel tile = _tileCreator.CreateTile(row, column, newParent);

            tile.onTileMove = ChangeTilePosition;
            tile.onDestroyTile = RemoveTile;
            tile.onEndTileMove = EndOfMoving;

            _gameField.AddTile(tile, row, column);
        }

        private void RemoveTile(TileData tileData)
        {
            _gameField.RemoveTlie(tileData.Row, tileData.Column);
        }

        private void ChangeTilePosition(TileData tileData, MovingDirectionEnum movingDirection)
        {
            bool isItYAxis = movingDirection == MovingDirectionEnum.MoveUp || movingDirection == MovingDirectionEnum.MoveDown;

            int tileForSwapRow;
            int tileForSwapColumn;

            if (isItYAxis)
            {
                tileForSwapRow = movingDirection == MovingDirectionEnum.MoveUp ? tileData.Row + 1 : tileData.Row - 1;
                tileForSwapColumn = tileData.Column;
            }
            else
            {
                tileForSwapColumn = movingDirection == MovingDirectionEnum.MoveLeft ? tileData.Column - 1 : tileData.Column + 1;
                tileForSwapRow = tileData.Row;
            }

            _gameField.SwapTiles(tileData.Row, tileData.Column, tileForSwapRow, tileForSwapColumn, true);
        }

        private void EndOfMoving(TileModel tileModel)
        {
            _swapedTiles.Add(tileModel);

            if(_swapedTiles.Count > 1)
            {
                _gameField.EndSwapingTiles(_swapedTiles);
                _swapedTiles.Clear();
            }
        }

        private void DestroyMatches(List<GameObject> matches)
        {
            for (int i = 0; i < matches.Count; i++)
            {
                matches[i].GetComponent<TileModel>().DestroyTile();
            }
        }
    }
}
