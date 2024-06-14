using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class LevelInformation
{
    public int Level;
    public int cakeOccupiedTileIndex;
    public int packOccupiedTileIndex;
    public List<int> blockedTileOccupiedTile = new List<int>();
}
