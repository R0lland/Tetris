using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager Instance;

    public int gridWidth;
    public int gridHeight;
    public Material ghostMaterial;

    [SerializeField]
    private HUDManager _hud;
    [SerializeField]
    private CreateGrid _gridManager;
    [SerializeField]
    private Transform _tetriminosParent;
    [SerializeField]
    private List<Tetrimino> _tetriminoPrefabs;

    private int _currentScore;
    private GameObject[,] _matrix;
    private int[] _lineBlocks;
    private AudioSource _scoreSound;

    public GameObject[,] Matrix {
        get {
            return _matrix;
        }
    }

    //https://tetris.fandom.com/wiki/Tetris_Guideline: Based on Tetris Wiki rules
    //Set an Instance to be called by other scripts
    private void Awake() {
        Instance = this;
    }

    private void Start() {
        _scoreSound = GetComponent<AudioSource>();
    }

    //Setting variables to start the game
    public void PlayGame() {
        _currentScore = 0;
        _hud.StartGame();
        _gridManager.Create(gridWidth, gridHeight);
        _matrix = new GameObject[gridHeight, gridWidth];
        _lineBlocks = new int[gridHeight];
        StartCoroutine(WaitToStart());
    }

    //Delete all tetriminos after finish the game
    private void DeleteTetriminos() {
        for (int i = 0; i < _tetriminosParent.childCount; i++) {
            Destroy(_tetriminosParent.GetChild(i).gameObject);
        }
    }

    //Return to Menu
    public void ReturnMenu() {
        DeleteTetriminos();
        _hud.ReturnMenu();
    }

    public void QuitGame() {
        Application.Quit();
    }

    //Replay the game
    public void Replay() {
        DeleteTetriminos();
        _currentScore = 0;
        _hud.SetScore(_currentScore);
        _hud.DisableGameOver();
        _matrix = new GameObject[gridHeight, gridWidth];
        _lineBlocks = new int[gridHeight];
        StartCoroutine(WaitToStart());
    }

    //Score Points
    public void ScorePoints() {
        _currentScore += 100;
        _hud.SetScore(_currentScore);
    }

    //Open Game Over screen
    public void GameOver() {
        _hud.GameOver(_currentScore);
    }

    //Add the blocks from the tetriminio to the respective place in the matrix
    public void AddBlockToMatrix(int line, int column, GameObject block) {
        _matrix[line, column] = block;
        _lineBlocks[line]++;
    }

    //Intantiate Tetriminio
    public void InstantiateTetriminio() {
        Tetrimino tetrimino = Instantiate(_tetriminoPrefabs[Random.Range(0, _tetriminoPrefabs.Count)], _tetriminosParent);
        tetrimino.Init();
    }

    //Check if there are any lines completed. I'm using a List to check the amount of elements in each row, so that I don't have to verify all elements of each row all the time
    public void CheckLines() {
        List<int> linesToClear = new List<int>();

        for (int i = 0; i < _lineBlocks.Length; i++) {
            if (_lineBlocks[i] >= gridWidth) {
                linesToClear.Add(i);
            }
        }

        if (linesToClear.Count > 0) {
            ClearLines(linesToClear);
        }
    }

    private void ClearLine(int lineToClear)
    {
        for (int i = 0; i < gridWidth; i++) {
            Destroy(_matrix[lineToClear, i]);
            _matrix[lineToClear, i] = null;
        }
    }

    //Clear lines that are completed, and pull lines from above to the blank spaces
    private void ClearLines(List<int> linesToClear) {
        int iteractions = 0;
        for (int i = 0; i < linesToClear.Count; i++)
        {
            ClearLine(linesToClear[i] - iteractions);

            for (int j = (linesToClear[i] + (1 - iteractions)); j < gridHeight; j++) {
                for (int k = 0; k < gridWidth; k++) {
                    if (_matrix[j, k] != null) {
                        _matrix[j, k].transform.position += new Vector3(0f, -1f, 0f);
                        _matrix[j - 1, k] = _matrix[j, k];
                        _matrix[j, k] = null;
                    }
                }
                _lineBlocks[j - 1] = _lineBlocks[j];
                _lineBlocks[j] = 0;
            }
            ScorePoints();
            iteractions++;
        }
        _scoreSound.Stop();
        _scoreSound.Play();
    }

    //Timer to start the game
    private IEnumerator WaitToStart() {
        yield return new WaitForSeconds(1f);
        InstantiateTetriminio();
    }

    public void PauseGame(bool pause) {
        Time.timeScale = pause ? 1 : 0;
    }
}