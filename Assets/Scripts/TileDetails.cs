using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TileDetails
{
    public TileType type;
    public Sprite Sprite;
}

public enum TileType { Cake, Pack, BlockedTile}
