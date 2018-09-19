using Assets.Scripts.Model;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Controller
{
    public class TileCreator : MonoBehaviour
    {
        [SerializeField] private List<GameObject> _tilesPrefabs;

        #region Public Methods

        public TileModel CreateTile(int row, int column, Transform newParent)
        {
            TileModel createdTile = GetRandomTileObject().GetComponent<TileModel>();

            createdTile.transform.SetParent(newParent);
            createdTile.InitializeTile(row, column);

            return createdTile;
        }

        #endregion

        #region Private Methods

        private GameObject GetRandomTileObject()
        {
            int randomTileIndex = Random.Range(0, _tilesPrefabs.Count);

            return Instantiate(_tilesPrefabs[randomTileIndex]) as GameObject;
        }

        #endregion
    }
}