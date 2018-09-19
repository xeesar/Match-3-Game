using UnityEngine;

namespace Assets.Scripts.Controller
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private GameFieldController _gameFieldController;

        #region MonoBehaviour Methods

        private void Awake()
        {
            _gameFieldController = FindObjectOfType<GameFieldController>();
        }

        private void Start()
        {
            StartGame();
        }
        #endregion

        #region Private Methods

        private void StartGame()
        {
            _gameFieldController.PrepareGameField();
        }

        private void EndGame()
        {
            Debug.Log("End Game.");
        }

        #endregion
    }
}