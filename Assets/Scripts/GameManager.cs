using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int Level = 1;
    [SerializeField] private int rows = 3;
    [SerializeField] private int columns = 3;
    [SerializeField] private float gamePlayTimeLimit = 5f;
    private float timeTick;
    [SerializeField] private float travelTime = 0.2f;
    [SerializeField] private Frame framePrefab;
    [SerializeField] private SpriteRenderer backgroundPrefab;
    [SerializeField] private SpriteRenderer timerPrefab;
    [SerializeField] private TextMeshPro textTimer;
    [SerializeField] private float offset = 0.5f;
    [SerializeField] private List<TileDetails> details;

    [SerializeField] private Tile tilePrefab;

    [SerializeField] private List<LevelInformation> informations;

    private List<Frame> frames;
    private List<Tile> tiles;
    private List<Tile> blockedTiles;
    [SerializeField] public GameState state;
    private Coroutine coroutine;

    public event Action<bool> OnInputDown = delegate { };

    private TileDetails GetTileDetailByType(int index)
    {
        for (int i = 0; i < details.Count; i++)
        {
            if ((int)details[i].type == index)
            {
                return details[i];
            }
        }
        return null;
    }

    private Frame GetFramePosition(Vector2 pos)
    {
        return frames.FirstOrDefault(f => f.Pos == pos);
    }
    IEnumerator DecreaseVariableOverTime()
    {
        while (state != GameState.Win)
        {
            yield return new WaitForSeconds(1f);
            timeTick--;
            textTimer.text = timeTick.ToString();
            if (timeTick == 0)
            {
                ChangeState(GameState.Lose);
                break;
            }
        }
    }

    private void Start()
    {
        InputController.OnPlayerSwipe += ShiftTile;
        timeTick = gamePlayTimeLimit;
        textTimer.text = timeTick.ToString();
        coroutine = StartCoroutine(DecreaseVariableOverTime());
        ChangeState(GameState.GenerateLevel);

    }

    private void ChangeState(GameState state)
    {
        this.state = state;

        switch (this.state)
        {
            case GameState.GenerateLevel:
                {
                    frames = new List<Frame>();
                    tiles = new List<Tile>();
                    blockedTiles = new List<Tile>();
                    GenerateGrid();
                    break;
                }
            case GameState.SpawnTile:
                {
                    StartCoroutine(SpawnTile(Level));
                    break;
                }

            case GameState.WaitingInput:
                break;
            case GameState.Moving:
                break;
            case GameState.Win:
                {
                    if (Level < informations.Count)
                    {
                        //Debug.Log(1);
                        Level++;
                        StartCoroutine(ClearTile());

                        if (coroutine != null || timeTick != gamePlayTimeLimit)
                        {
                            StopCoroutine(DecreaseVariableOverTime());
                            timeTick = gamePlayTimeLimit;
                            textTimer.text = timeTick.ToString();
                        }
                        coroutine = StartCoroutine(DecreaseVariableOverTime());

                        ChangeState(GameState.SpawnTile);
                    }
                    else
                    {
                        //Debug.Log(2);
                    }
                    
                    break;
                }
                
            case GameState.Lose:
                break;
            default:
                break;
        }
    }

    private void GenerateGrid()
    {
        Vector2 centerScreen = new Vector2((float)rows / 2 - 0.5f, (float)columns / 2 - 0.5f);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                float xPosition = centerScreen.x + (i - rows / 2) * offset;
                float yPosition = centerScreen.y + (j - columns / 2) * offset;
                Frame frame = Instantiate(framePrefab, new Vector2(xPosition, yPosition), Quaternion.identity);
                frames.Add(frame);
            }
        }

        SpriteRenderer background = Instantiate(backgroundPrefab, centerScreen, Quaternion.identity);
        SpriteRenderer timer = Instantiate(timerPrefab, new Vector2(centerScreen.x, centerScreen.y + columns), Quaternion.identity);
        textTimer = Instantiate(textTimer, new Vector2(centerScreen.x, centerScreen.y + columns), Quaternion.identity);
        textTimer.text = gamePlayTimeLimit.ToString();
        Camera.main.transform.position = new Vector3(centerScreen.x, centerScreen.y, -10f);

        ChangeState(GameState.SpawnTile);
    }

    private IEnumerator SpawnTile(int level)
    {
        yield return 0;

        LevelInformation information = informations.Find(p => p.Level == level);

        Vector2 cakePos = frames[information.cakeOccupiedTileIndex].Pos;
        Tile cakeTile = Instantiate(tilePrefab, cakePos, Quaternion.identity);
        cakeTile.Create(GetTileDetailByType(0));
        cakeTile.SetTile(frames[information.cakeOccupiedTileIndex]);
        tiles.Add(cakeTile);

        Vector2 packPos = frames[information.packOccupiedTileIndex].Pos;
        Tile packTile = Instantiate(tilePrefab, packPos, Quaternion.identity);
        packTile.Create(GetTileDetailByType(1));
        packTile.SetTile(frames[information.packOccupiedTileIndex]);
        tiles.Add(packTile);

        foreach (Tile tile in tiles)
        {
            OnInputDown += tile.HandleInputDown;
        }

        if (information.blockedTileOccupiedTile.Count > 0)
        {
            for (int i = 0; i < information.blockedTileOccupiedTile.Count; i++)
            {
                Vector2 blockedPos = frames[information.blockedTileOccupiedTile[i]].Pos;
                Tile blocked = Instantiate(tilePrefab, blockedPos, Quaternion.identity);
                blocked.SetTile(frames[information.blockedTileOccupiedTile[i]]);
                blocked.Create(GetTileDetailByType(2));
                blockedTiles.Add(blocked);
            }
        }

        ChangeState(GameState.WaitingInput);
    }

    private IEnumerator ClearTile()
    {
        yield return 0;

        foreach (var tile in tiles)
        {
            Destroy(tile.gameObject);
        }
        tiles.Clear();

        
        foreach (var blockedTile in blockedTiles)
        {
            Destroy(blockedTile.gameObject);
        }
        blockedTiles.Clear();
    }

    private void ShiftTile(Vector2 direction)
    {
        ChangeState(GameState.Moving);
        if (direction == Vector2.down || direction == Vector2.up)
        {
            OnInputDown?.Invoke(true);
        }
        else
        {
            OnInputDown?.Invoke(false);

        }

        List<Tile> orderedTiles = tiles.OrderBy(t => t.Pos.x).ThenBy(t => t.Pos.y).ToList();

        if (direction != Vector2.right || direction != Vector2.up)
        {
            orderedTiles.Reverse();
        }

        foreach (Tile tile in orderedTiles)
        {
            Frame nextFrame = tile.frame;
            do
            {
                tile.SetTile(nextFrame);

                Frame possibleFrame = GetFramePosition(nextFrame.Pos + direction);

                if (possibleFrame != null)
                {
                    if (possibleFrame.tileOccupiedThisFrame != null && possibleFrame.tileOccupiedThisFrame.CanMerge(tile.tileDetails))
                    {
                        bool isDownAndPack = direction == Vector2.down && possibleFrame.tileOccupiedThisFrame.tileDetails.type == TileType.Pack;
                        bool isUpAndCake = direction == Vector2.up && possibleFrame.tileOccupiedThisFrame.tileDetails.type == TileType.Cake;

                        if (isDownAndPack || isUpAndCake)
                        {
                            tile.Merge(possibleFrame.tileOccupiedThisFrame);
                        }
                    }

                    if (possibleFrame.tileOccupiedThisFrame == null)
                    {
                        nextFrame = possibleFrame;
                    }
                }
            } while (nextFrame != tile.frame);


        }

        Sequence sequence = DOTween.Sequence();
        foreach (Tile tile in orderedTiles)
        {
            Vector3 position = tile.tileMergeWith == null ? tile.frame.Pos : tile.tileMergeWith.Pos;
            sequence.Insert(0, tile.transform.DOMove(position, travelTime));
        }

        sequence.OnComplete(() =>
        {
            foreach (Tile tile in orderedTiles.Where(ot => ot.tileMergeWith != null))
            {
                RemoveTile(tile);
            }
        });

        ChangeState(GameState.WaitingInput);
    }

    private void RemoveTile(Tile tile)
    {
        if(tile.tileDetails.type == TileType.Cake) 
        {
            tiles.Remove(tile);
            Destroy(tile.gameObject);
        }
        else
        {
            tile.frame = tile.tileMergeWith.frame;
            tiles.Remove(tile.tileMergeWith);
            Destroy(tile.tileMergeWith.gameObject);
        }

        ChangeState(GameState.Win); 
    }
}

public enum GameState
{
    GenerateLevel,
    SpawnTile,
    WaitingInput,
    Moving,
    Win,
    Lose
}