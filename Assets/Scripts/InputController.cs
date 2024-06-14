using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    private GameManager gameManager;
    private Vector2 startingTouch;
    private bool isSwiping = false;
    public static event Action<Vector2> OnPlayerSwipe;
    private void Start()
    {
        gameManager = GetComponent<GameManager>();
    }

    private void Update()
    {
        if (gameManager.state != GameState.WaitingInput)
        {
            return;
        }

        if (Input.touchCount == 1)
        {
            if (isSwiping)
            {
                Vector2 diff = Input.GetTouch(0).position - startingTouch;
                
                diff = new Vector2(diff.x / Screen.width, diff.y / Screen.width);

                if (diff.magnitude > 0.01f) 
                {
                    if (Mathf.Abs(diff.y) > Mathf.Abs(diff.x)) 
                    {
                        if (diff.y < 0)
                        {
                            OnPlayerSwipe?.Invoke(Vector2.down);
                        }
                        else
                        {
                            OnPlayerSwipe?.Invoke(Vector2.up);
                        }
                    }
                    else //left Or Right
                    {
                        if (diff.x < 0)
                        {
                            OnPlayerSwipe?.Invoke(Vector2.left);
                        }
                        else
                        {
                            OnPlayerSwipe?.Invoke(Vector2.right);
                        }
                    }
                    isSwiping = false;
                }
            }

            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                startingTouch = Input.GetTouch(0).position;
                isSwiping = true;
            }

            else if (Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                isSwiping = false;
            }

        }
    }

}
