using Assets.Scripts.Extentions;
using UnityEngine;

namespace Assets.Scripts.Data
{
    public class GameOptions : Singleton<GameOptions>
    {
        public int Rows { get { return _rows; } }
        public int Columns { get { return _columns; } }
        public int MatchingCondition { get { return _tilesCountForMatching; } }

        [Header("Game Field Options")]
        [SerializeField] [Range(1, _MAX_COLUMNS)] private int _columns = 1;
        [SerializeField] [Range(1, _MAX_ROWS)] private int _rows = 1;
        [SerializeField] private int _tilesCountForMatching = 2;

        #region Constants
        private const int _MAX_ROWS = 10;
        private const int _MAX_COLUMNS = 7;
        #endregion
    }
}