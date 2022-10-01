using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetriminoI : Tetrimino
{
    protected override bool ControlWallKickFromTetrimino(bool clockwise, Vector3 originalPosition, int currentRot)
    {
        return ControlWallKick(clockwise ? _wallKickController.wallKickIClockwise : _wallKickController.wallKickICounterClockwise, originalPosition, currentRot);
    }
}
