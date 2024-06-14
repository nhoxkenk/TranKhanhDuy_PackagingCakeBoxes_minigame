using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Frame frame;
    public Tile tileMergeWith;
    public bool canMerge;
    [SerializeField] private SpriteRenderer render;
    [SerializeField] public TileDetails tileDetails;
    public Vector2 Pos { get => transform.position; set => transform.position = value; }

    public void Create(TileDetails details)
    {
        tileDetails = details;
        render.sprite = tileDetails.Sprite;
    }

    public void SetTile(Frame frame)
    {
        if(this.frame != null)
        {
            this.frame.tileOccupiedThisFrame = null;
        }
        this.frame = frame;
        this.frame.tileOccupiedThisFrame = this;
    }

    public void Merge(Tile tileToMerge)
    {
        tileMergeWith = tileToMerge;
        frame.tileOccupiedThisFrame = null;
        this.canMerge = true;
    }

    public bool CanMerge(TileDetails details)
    {
        if(canMerge)
        {
            return true;
        }

        return false;
    }

    public void HandleInputDown(bool value) => canMerge = value;
}
