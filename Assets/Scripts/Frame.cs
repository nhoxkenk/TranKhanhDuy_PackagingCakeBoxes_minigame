using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Frame : MonoBehaviour
{
    public Tile tileOccupiedThisFrame;
    public Vector2 Pos { get => transform.position; set => transform.position = value; }
}
