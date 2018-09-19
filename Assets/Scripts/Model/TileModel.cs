using Assets.Scripts.Data;
using Assets.Scripts.Enum;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Model
{
    public class TileModel : MonoBehaviour
    {
        #region Delegates
        public delegate void OnTileMove(TileData tileData, MovingDirectionEnum movingDirection);
        public delegate void OnEndTileMove(TileModel tileModel);
        public delegate void OnDestroyTile(TileData tileData);

        public OnTileMove onTileMove;
        public OnEndTileMove onEndTileMove;
        public OnDestroyTile onDestroyTile;
        #endregion

        public TileData TileData { get { return _tileData; } }

        [SerializeField] private TileData _tileData;

        private Camera _mainCamera;

        private Vector2 _startTouchPos;
        private Vector2 _endTouchPos;

        private const float _MOVING_TIME = 0.2f;

        private GameOptions _gameOptions;

        #region MonoBehaviour Methods

        private void Start()
        {
            _gameOptions = GameOptions.Instance;
            _mainCamera = Camera.main;
        }

        private void OnMouseDown()
        {
            _startTouchPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        }

        private void OnMouseUp()
        {
            _endTouchPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);

            MovingDirectionEnum movingDirection = GetMovingDirection();

            if (IsCanMove(movingDirection))
            {
                onTileMove(_tileData, movingDirection);
            }
        }
        #endregion

        #region Public Methods
        public void InitializeTile(int row, int column)
        {
            _tileData.Row = row;
            _tileData.Column = column;

            transform.localPosition = new Vector2(column, 0);
        }

        public void ChangeTilePosition(int row, int column, Transform newParent)
        {
            _tileData.Row = row;
            _tileData.Column = column;

            transform.SetParent(newParent);
        }

        public void DestroyTile()
        {
            onDestroyTile(_tileData);
            Destroy(gameObject);
        }

        public IEnumerator MoveTile(bool doItNeedToNotify = false)
        {
            Vector3 newPos = new Vector3(_tileData.Column, 0, 0);

            Vector3 startPos = transform.localPosition;

            float startTime = Time.realtimeSinceStartup;
            float fraction = 0;

            while (fraction < 1)
            {
                fraction = Mathf.Clamp01((Time.realtimeSinceStartup - startTime) / _MOVING_TIME);

                transform.localPosition = Vector3.Lerp(startPos, newPos, fraction);

                yield return null;
            }

            if(doItNeedToNotify)
            {
                onEndTileMove.Invoke(this);
            }
        }


        #endregion

        #region Private Methods

        private MovingDirectionEnum GetMovingDirection()
        {
            float swipeAngle = Mathf.Atan2(_endTouchPos.y - _startTouchPos.y, _endTouchPos.x - _startTouchPos.x) * 180 / Mathf.PI;

            if(swipeAngle == 0)
            {
                return MovingDirectionEnum.NoMove;
            }

            if(swipeAngle < 135 && swipeAngle >= 45)
            {
                return MovingDirectionEnum.MoveUp;
            }

            if(swipeAngle < 45 && swipeAngle >= -45)
            {
                return MovingDirectionEnum.MoveRight;
            }

            if(swipeAngle < -45 && swipeAngle >= -135)
            {
                return MovingDirectionEnum.MoveDown;
            }

            if(swipeAngle < -135 || swipeAngle >= 135)
            {
                return MovingDirectionEnum.MoveLeft;
            }

            return MovingDirectionEnum.NoMove;
        }

        private bool IsCanMove(MovingDirectionEnum movingDirection)
        {
            switch(movingDirection)
            {
                case MovingDirectionEnum.MoveUp:
                    return _tileData.Row < _gameOptions.Rows - 1;
                case MovingDirectionEnum.MoveDown:
                    return _tileData.Row > 0;
                case MovingDirectionEnum.MoveLeft:
                    return _tileData.Column > 0;
                case MovingDirectionEnum.MoveRight:
                    return _tileData.Column < _gameOptions.Columns - 1;
                default:
                    return false;
            }
        }

        #endregion
    }
}
