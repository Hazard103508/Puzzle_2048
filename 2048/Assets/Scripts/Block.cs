using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public int Value;
    public Vector2Int location;
    public BlockState state;

    #region Methods
    /// <summary>
    /// Inicia el movimiento del bloque
    /// </summary>
    /// <param name="targetLocation">Corrdenada destino (posicion de la grilla)</param>
    public void Move(Vector2Int targetLocation)
    {
        StartCoroutine(MoveCO(targetLocation));

        this.location = targetLocation;
        this.state = BlockState.Moving;
    }
    private IEnumerator MoveCO(Vector2Int targetLocation)
    {
        Vector3 startingPos = transform.localPosition;
        Vector3 finalPos = new Vector2(targetLocation.x * 2, targetLocation.y * 2);
        float elapsedTime = 0;

        float distance = Mathf.Abs(targetLocation.x - this.location.x) + Mathf.Abs(targetLocation.y - this.location.y);
        float time = distance * 0.2f; // duracion en segundo que demora trasladarce de una celda a otra

        while (elapsedTime < time)
        {
            transform.localPosition = Vector3.Lerp(startingPos, finalPos, (elapsedTime / time));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        this.state = BlockState.Idle;
        yield return null;
    }
    #endregion
}
public enum BlockState
{
    Idle,
    Moving,
}