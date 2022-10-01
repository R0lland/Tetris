using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public struct WallKick {
    public Vector2[] positionsToTest;
}

public class WallKickController : MonoBehaviour
{
    public static WallKickController Instance;

    public WallKick[] wallKickGeneralClockwise;
    public WallKick[] wallKickGeneralCounterClockwise;
    public WallKick[] wallKickIClockwise;
    public WallKick[] wallKickICounterClockwise;
    public WallKick[] wallKickOnInstantiate;

    private void Awake() {
        Instance = this;
    }
}
