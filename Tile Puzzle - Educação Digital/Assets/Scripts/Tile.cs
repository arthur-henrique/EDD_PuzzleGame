using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] PuzzleManager puzzleManager;
    public Vector2 gridPosition;
    public Vector2 targetPosition;
    private static Vector2 emptySlotPos = new Vector2(1, -1);

    private void Start()
    {
        gridPosition = transform.position;
        puzzleManager.tiles.Add(this);
    }


    private void OnMouseDown()
    {
        print("MouseDown");
        if (IsAdjacentToEmptySlot())
        {
            print("MouseIN");
            StartCoroutine(MoveTileToEmptySlot());
        }
    }
    private bool IsAdjacentToEmptySlot()
    {
        return (Mathf.Abs(gridPosition.x - emptySlotPos.x) == 1 && gridPosition.y == emptySlotPos.y) ||
               (Mathf.Abs(gridPosition.y - emptySlotPos.y) == 1 && gridPosition.x == emptySlotPos.x);
    }

    private IEnumerator MoveTileToEmptySlot()
    {
        Vector2 emptySlotWorldPos = GridToWorld(emptySlotPos);
        while (Vector2.Distance(transform.position, emptySlotWorldPos) > 0.01f)
        {
            transform.position = Vector2.Lerp(transform.position, emptySlotWorldPos, Time.deltaTime * 10);
            yield return null;
        }

        transform.position = emptySlotWorldPos;

        // Swap grid positions after move completes
        (gridPosition, emptySlotPos) = (emptySlotPos, gridPosition);
        PuzzleManager.instance.CheckPuzzleCompletion();
    }

    private Vector2 GridToWorld(Vector2 gridPos)
    {
        return new Vector2(gridPos.x, gridPos.y);
    }
}
