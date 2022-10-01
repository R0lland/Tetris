using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tetrimino : MonoBehaviour {
    public float diff;

    private bool _canMove;
    private float _timerAutoMove;
    private float _timerSideMove;
    private float _timerLock;
    private Transform _ghostPiece;

    //List of Vector2 to use as a base to the tetris rules
    private int _currentRotId;
    private bool _locked;
    private bool _waitingToLock;
    private float _lockDelayTime;
    private GameManager _gameManager;
    
    protected WallKickController _wallKickController;

    // Update is called once per frame
    void Update() {
        if (_canMove) {
            //Hard drop
            if (Input.GetKeyDown(KeyCode.Space)) {
                transform.position = _ghostPiece.position;
                if (!CheckGameOver()) {
                    LockTetrimino();
                }
            }

            if (Input.GetKeyDown(KeyCode.UpArrow)) {
                CheckRotation(true);
            }

            if (Input.GetKeyDown(KeyCode.X)) {
                CheckRotation(true);
            }

            if (Input.GetKeyDown(KeyCode.Z)) {
                CheckRotation(false);
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow)) {
                _waitingToLock = false;
                Move(-1, 0);
                _timerSideMove = -0.5f;
            }

            if (Input.GetKeyDown(KeyCode.RightArrow)) {
                _waitingToLock = false;
                Move(1, 0);
                _timerSideMove = -0.5f;
            }

            //Delay after holding the right or left button to the sides to move faster
            if (Input.GetKey(KeyCode.RightArrow)) {
                MoveFastSide(1, 0);
            } else if (Input.GetKey(KeyCode.LeftArrow)) {
                MoveFastSide(-1, 0);
            }
            _timerSideMove += Time.deltaTime;

            //If it's not on a lock delay, will move automatically
            if (!_waitingToLock) {
                if (Input.GetKey(KeyCode.DownArrow)) {
                    MoveAutomatically(0.1f);
                    _lockDelayTime = 0.0f;
                } else {
                    MoveAutomatically(0.6f);
                    _lockDelayTime = 0.8f;
                }
            }

            if (Input.GetKey(KeyCode.DownArrow)) {
                _lockDelayTime = 0.0f;
            }
            //Create a lock delay so that the player can choose better after a movement or rotation when close to a boundary
            if (_waitingToLock) {
                _timerLock += Time.deltaTime;
                if (_timerLock > _lockDelayTime) {
                    LockTetrimino();
                }
            }
        }
    }

    private void MoveFastSide(int moveX, int moveY) {
        if (_timerSideMove > 0.1f) {
            Move(moveX, moveY);
            _timerSideMove = 0f;
        }
    }

    //Check where the tetriminio can move
    private bool CheckMovement(int moveX, int moveY) {
        for (int i = 0; i < transform.childCount; i++) {
            int destX = Mathf.RoundToInt(transform.GetChild(i).transform.position.x + moveX);
            int destY = Mathf.RoundToInt(transform.GetChild(i).transform.position.y + moveY);

            if (CheckGameOver()) {
                return false;
            }

            if (moveY < 0 && (destY < 0 || _gameManager.Matrix[destY, destX] != null)) {
                _waitingToLock = true;
                _timerLock = 0f;
                _timerAutoMove = 0f;
                return false;
            }

            if (destX < 0 || destX >= _gameManager.gridWidth || _gameManager.Matrix[destY, destX] != null) {
                return false;
            }
        }
        return true;
    }

    private bool CheckGameOver() {
        for (int i = 0; i < transform.childCount; i++) {
            int destX = Mathf.RoundToInt(transform.GetChild(i).transform.position.x);
            int destY = Mathf.RoundToInt(transform.GetChild(i).transform.position.y - 1);
            if (Mathf.RoundToInt(transform.GetChild(i).transform.position.y) > _gameManager.gridHeight - 1 && _gameManager.Matrix[destY, destX] != null) {
                _canMove = false;
                _gameManager.GameOver();
                return true;
            }
        }
        return false;
    }

    protected virtual bool ControlWallKickFromTetrimino(bool clockwise, Vector3 originalPosition, int currentRot) {
        return ControlWallKick(clockwise ? _wallKickController.wallKickGeneralClockwise : _wallKickController.wallKickGeneralCounterClockwise, originalPosition, currentRot);
    }

    //Check where the tetriminio can rotate
    private bool CheckRotation(bool clockwise) {
        _waitingToLock = false;
        Vector3 originalRot = transform.eulerAngles;
        Vector3 originalPos = transform.position;
        int nextId = GetNextRotationId(clockwise);
        bool canRotate;
        int rot = clockwise ? -90 : 90;
        transform.Rotate(new Vector3(0f, 0f, rot));

        //Check the Wallkick function
        canRotate = ControlWallKickFromTetrimino(clockwise, originalPos, _currentRotId);

        if (!canRotate) {
            transform.position = originalPos;
            transform.eulerAngles = originalRot;
        } else {
            _currentRotId = nextId;
            SetGhostPiece();
        }
        return true;
    }

    //Checking for WallKicks based on the rotation Rules
    //https://tetris.fandom.com/wiki/SRS
    protected bool ControlWallKick(WallKick[] wallKick, Vector3 originalPos, int rotId) {
        bool canRotate = false;
        for (int i = 0; i < wallKick[rotId].positionsToTest.Length; i++) {
            bool fit = true;
            Vector2 PosToTest = wallKick[rotId].positionsToTest[i];
            transform.position = new Vector3(originalPos.x + PosToTest.x, originalPos.y + PosToTest.y, originalPos.z);
            for (int j = 0; j < transform.childCount; j++) {
                int posX = Mathf.RoundToInt(transform.GetChild(j).transform.position.x);
                int posY = Mathf.RoundToInt(transform.GetChild(j).transform.position.y);
                if (posX < 0 || posX >= _gameManager.gridWidth || posY < 0 || posY >= _gameManager.gridHeight ||
                    _gameManager.Matrix[posY, posX] != null) {
                    fit = false;
                    break;
                }
            }
            if (fit) {
                canRotate = true;
                break;
            }
            if (i > 0) {
                _timerAutoMove = 0f;
            }
        }
        return canRotate;
    }

    //Check Next Possible Id
    private int GetNextRotationId(bool clockwise) {
        int id = _currentRotId;
        id += clockwise ? 1 : -1;
        if (id > 3) {
            id = 0;
        } else if (id < 0) {
            id = 3;
        }
        return id;
    }

    public void Init()
    {
        _timerAutoMove = 0f;
        _currentRotId = 0;
        _wallKickController = WallKickController.Instance;
        _gameManager = GameManager.Instance;
        InstantiateOnTop();
    }

    private void InstantiateOnTop() {
        bool positionIsCorrect = false;
        transform.position = new Vector3(_gameManager.gridWidth / 2 + diff - 1, _gameManager.gridHeight + diff, 0f);
        //Always instantiate the tetrimino on the top of the matrix
        while (!positionIsCorrect) {
            transform.position += new Vector3(0f, -1f, 0f);
            positionIsCorrect = true;
            for (int i = 0; i < transform.childCount; i++) {
                if (Mathf.RoundToInt(transform.GetChild(i).transform.position.y) >= _gameManager.gridHeight) {
                    positionIsCorrect = false;
                }
            }
        }

        //Verify if will overlap other blocks
        bool needToKick = false;
        for (int j = 0; j < transform.childCount; j++) {
            int posX = Mathf.RoundToInt(transform.GetChild(j).transform.position.x);
            int posY = Mathf.RoundToInt(transform.GetChild(j).transform.position.y);
            if (_gameManager.Matrix[posY, posX] != null) {
                needToKick = true;
                break;
            }
        }

        Vector3 originalPos = transform.position;
        //if overlap, try to kick the tetrimino to another locations
        if (needToKick) {
            bool fit = ControlWallKick(_wallKickController.wallKickOnInstantiate, originalPos, 0);
            //If doesn't fit in any location it's game over
            if (!fit) {
                transform.position = originalPos;
                _gameManager.GameOver();
                return;
            }
        }

        //Instantiate the same object as a ghost, but set the material as gray
        
        Tetrimino ghostPiece = Instantiate(this, transform.parent);
        ghostPiece.enabled = false;
        _ghostPiece = ghostPiece.transform;
        for (int i = 0; i < _ghostPiece.childCount; i++) {
            _ghostPiece.GetChild(i).GetComponent<MeshRenderer>().material = _gameManager.ghostMaterial;
        }
        SetGhostPiece();
        _canMove = true;
    }

    //Move to the sides
    private void Move(int moveX, int moveY) {
        if (CheckMovement(moveX, moveY)) {
            transform.position += new Vector3(moveX, moveY, 0f);
            SetGhostPiece();
        }
    }

    //Move down automatically
    private void MoveAutomatically(float speed) {
        if (_timerAutoMove >= speed) {
            if (CheckMovement(0, -1)) {
                transform.position += new Vector3(0f, -1f, 0f);
                _timerAutoMove = 0f;
            }
        }
        _timerAutoMove += Time.deltaTime;
    }

    //Set the position of the ghost that will show where the block will fit. Start checking from the current tetrimino Position
    private void SetGhostPiece() {
        bool foundBoundary = false;
        _ghostPiece.position = transform.position;
        _ghostPiece.eulerAngles = transform.eulerAngles;
        for (int i = 0; i < _gameManager.gridHeight; i++) {
            for (int j = 0; j < _ghostPiece.childCount; j++) {
                int destX = Mathf.RoundToInt(_ghostPiece.GetChild(j).transform.position.x);
                int destY = Mathf.RoundToInt(_ghostPiece.GetChild(j).transform.position.y - 1);

                if ((destY < 0 || _gameManager.Matrix[destY, destX] != null)) {
                    foundBoundary = true;
                    break;
                }
            }
            if (foundBoundary) {
                break;
            }
            _ghostPiece.position = new Vector3(_ghostPiece.position.x, _ghostPiece.position.y - 1, _ghostPiece.position.z);
        }
        _ghostPiece.position = new Vector3(_ghostPiece.position.x, _ghostPiece.position.y, _ghostPiece.position.z + 1);
    }

    //After the tetrimino reaches the bottom of the matrix, or it has collided with another Tetrimino it will be added as a permanent component to the matrix
    private void LockTetrimino() {
        if (_locked) {
            return;
        }
        _locked = true;
        _canMove = false;
        List<int> linesAddedToMatrix = new List<int>();
        for (int i = 0; i < transform.childCount; i++) {
            int line = Mathf.RoundToInt(transform.GetChild(i).transform.position.y);
            int column = Mathf.RoundToInt(transform.GetChild(i).transform.position.x);
            _gameManager.AddBlockToMatrix(line, column, transform.GetChild(i).gameObject);
            if (!linesAddedToMatrix.Contains(line))
            {
                linesAddedToMatrix.Add(line);
            }
        }
        
        if (_gameManager.legacyClearLines)
            _gameManager.CheckLines();
        else
            _gameManager.CheckLinesNew(linesAddedToMatrix);
        _gameManager.InstantiateTetriminio();
        Destroy(_ghostPiece.gameObject);
    }
}
