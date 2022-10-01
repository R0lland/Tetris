using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tetrimino : MonoBehaviour {
    public float diff;

    private bool _canMove;
    private float _timerAutoMove;
    private float _timerSideMove;
    private float _timerLock;
    private Tetrimino _ghostPiece;

    //List of Vector2 to use as a base to the tetris rules
    private int _currentRotId;
    private bool _locked;
    private bool _waitingToLock;
    private float _lockDelayTime;
    
    protected WallKickController _wallKickController;

    // Update is called once per frame
    void Update() {
        if (_canMove) {
            //Hard drop
            if (Input.GetKeyDown(KeyCode.Space)) {
                transform.position = _ghostPiece.transform.position;
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

            if (moveY < 0 && (destY < 0 || GameManager.Instance.Matrix[destY, destX] != null)) {
                _waitingToLock = true;
                _timerLock = 0f;
                _timerAutoMove = 0f;
                return false;
            }

            if (destX < 0 || destX >= GameManager.Instance.gridWidth || GameManager.Instance.Matrix[destY, destX] != null) {
                return false;
            }
        }
        return true;
    }

    private bool CheckGameOver() {
        for (int i = 0; i < transform.childCount; i++) {
            int destX = Mathf.RoundToInt(transform.GetChild(i).transform.position.x);
            int destY = Mathf.RoundToInt(transform.GetChild(i).transform.position.y - 1);
            if (Mathf.RoundToInt(transform.GetChild(i).transform.position.y) > GameManager.Instance.gridHeight - 1 && GameManager.Instance.Matrix[destY, destX] != null) {
                _canMove = false;
                GameManager.Instance.GameOver();
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
                if (posX < 0 || posX >= GameManager.Instance.gridWidth || posY < 0 || posY >= GameManager.Instance.gridHeight ||
                    GameManager.Instance.Matrix[posY, posX] != null) {
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


    public void InstantiateOnTop() {
        _timerAutoMove = 0f;
        _currentRotId = 0;
        _wallKickController = WallKickController.Instance;
        bool positionIsCorrect = false;
        transform.position = new Vector3(GameManager.Instance.gridWidth / 2 + diff - 1, GameManager.Instance.gridHeight + diff, 0f);
        //Always instantiate the tetrimino on the top of the matrix
        while (!positionIsCorrect) {
            transform.position += new Vector3(0f, -1f, 0f);
            positionIsCorrect = true;
            for (int i = 0; i < transform.childCount; i++) {
                if (Mathf.RoundToInt(transform.GetChild(i).transform.position.y) >= GameManager.Instance.gridHeight) {
                    positionIsCorrect = false;
                }
            }
        }

        //Verify if will overlap other blocks
        bool needToKick = false;
        for (int j = 0; j < transform.childCount; j++) {
            int posX = Mathf.RoundToInt(transform.GetChild(j).transform.position.x);
            int posY = Mathf.RoundToInt(transform.GetChild(j).transform.position.y);
            if (GameManager.Instance.Matrix[posY, posX] != null) {
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
                GameManager.Instance.GameOver();
                return;
            }
        }

        //Instantiate the same object as a ghost, but set the material as gray
        _ghostPiece = Instantiate(this, transform.parent);
        _ghostPiece.enabled = false;
        for (int i = 0; i < _ghostPiece.transform.childCount; i++) {
            _ghostPiece.transform.GetChild(i).GetComponent<MeshRenderer>().material = GameManager.Instance.ghostMaterial;
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
        _ghostPiece.transform.position = transform.position;
        _ghostPiece.transform.eulerAngles = transform.eulerAngles;
        for (int i = 0; i < GameManager.Instance.gridHeight; i++) {
            for (int j = 0; j < _ghostPiece.transform.childCount; j++) {
                int destX = Mathf.RoundToInt(_ghostPiece.transform.GetChild(j).transform.position.x);
                int destY = Mathf.RoundToInt(_ghostPiece.transform.GetChild(j).transform.position.y - 1);

                if ((destY < 0 || GameManager.Instance.Matrix[destY, destX] != null)) {
                    foundBoundary = true;
                    break;
                }
            }
            if (foundBoundary) {
                break;
            }
            _ghostPiece.transform.position = new Vector3(_ghostPiece.transform.position.x, _ghostPiece.transform.position.y - 1, _ghostPiece.transform.position.z);
        }
        _ghostPiece.transform.position = new Vector3(_ghostPiece.transform.position.x, _ghostPiece.transform.position.y, _ghostPiece.transform.position.z + 1);
    }

    //After the tetrimino reaches the bottom of the matrix, or it has collided with another Tetrimino it will be added as a permanent component to the matrix
    private void LockTetrimino() {
        if (_locked) {
            return;
        }
        _locked = true;
        _canMove = false;
        List<int> linesToCheck = new List<int>();
        for (int i = 0; i < transform.childCount; i++) {
            int line = Mathf.RoundToInt(transform.GetChild(i).transform.position.y);
            int column = Mathf.RoundToInt(transform.GetChild(i).transform.position.x);
            GameManager.Instance.AddBlockToMatrix(line, column, transform.GetChild(i).gameObject);

            linesToCheck.Add(line);
        }

        GameManager.Instance.CheckLines();
        GameManager.Instance.InstantiateTetriminio();
        Destroy(_ghostPiece.gameObject);
    }
}
