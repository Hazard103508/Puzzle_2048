using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Table : MonoBehaviour
{
    private LevelState State;
    private Dictionary<int, GameObject> BlocskDic;
    private List<Block> lstBlocks; // lista de bloques

    public GameObject BlocksRoot;
    public GameOver gameOver;

    [Header("Prefabs")]
    public List<GameObject> Blocks;

    [Header("Events")]
    public UnityEvent<int> PointEarned;
    public UnityEvent ResetPoints;

    #region Start
    void Start()
    {
        lstBlocks = new List<Block>();

        BlocskDic = new Dictionary<int, GameObject>();
        BlocskDic.Add(2, Blocks[0]);
        BlocskDic.Add(4, Blocks[1]);
        BlocskDic.Add(8, Blocks[2]);
        BlocskDic.Add(16, Blocks[3]);
        BlocskDic.Add(32, Blocks[4]);
        BlocskDic.Add(64, Blocks[5]);
        BlocskDic.Add(128, Blocks[6]);
        BlocskDic.Add(256, Blocks[7]);
        BlocskDic.Add(512, Blocks[8]);
        BlocskDic.Add(1024, Blocks[9]);
        BlocskDic.Add(2048, Blocks[10]);
        BlocskDic.Add(4096, Blocks[11]);

        this.State = LevelState.New;

        //TEST
        //Add_Block(new Vector2Int(0, 0), 2);
        //Add_Block(new Vector2Int(1, 0), 4);
        //Add_Block(new Vector2Int(2, 0), 2);
        //Add_Block(new Vector2Int(3, 0), 4);
        //Add_Block(new Vector2Int(0, 1), 4);
        //Add_Block(new Vector2Int(1, 1), 2);
        //Add_Block(new Vector2Int(2, 1), 4);
        //Add_Block(new Vector2Int(3, 1), 2);
        //Add_Block(new Vector2Int(0, 2), 2);
        //Add_Block(new Vector2Int(1, 2), 4);
        //Add_Block(new Vector2Int(2, 2), 2);
        //Add_Block(new Vector2Int(3, 2), 4);
        //Add_Block(new Vector2Int(0, 3), 4);
        //Add_Block(new Vector2Int(1, 3), 2);
        //Add_Block(new Vector2Int(2, 3), 4);
        //Add_Block(new Vector2Int(3, 3), 2);
    }
    #endregion

    #region Update
    void Update()
    {
        switch (this.State)
        {
            case LevelState.New:
                Add_NewBlock();
                if (Check_GameOver())
                {
                    this.State = LevelState.GameOver;
                    gameOver.Show();
                }
                else
                    this.State = LevelState.Idle;
                break;
            case LevelState.Idle:
                if (Input.GetKeyDown(KeyCode.LeftArrow))
                    Move(Direction.Left);
                else if (Input.GetKeyDown(KeyCode.RightArrow))
                    Move(Direction.Right);
                else if (Input.GetKeyDown(KeyCode.UpArrow))
                    Move(Direction.Up);
                else if (Input.GetKeyDown(KeyCode.DownArrow))
                    Move(Direction.Down);
                break;
            case LevelState.Moving:
                Merge_Block();
                if (Check_Idle())
                    this.State = LevelState.New;
                break;
            case LevelState.GameOver:
                break;
            default:
                break;
        }
    }
    #endregion

    #region Methods
    public void Restart()
    {
        gameOver.Close();

        lstBlocks.ForEach(block => Destroy(block.gameObject));
        lstBlocks = new List<Block>();
        ResetPoints.Invoke();
        this.State = LevelState.New;
    }

    /// <summary>
    /// Agrega un nuevo bloque al tablero
    /// </summary>
    /// <returns></returns>
    private bool Add_NewBlock()
    {
        var nextLocation = Get_FreeLocation();
        if (nextLocation.HasValue)
        {
            int value = Random.value < 0.8f ? 2 : 4;
            Add_Block(nextLocation.Value, value);
            PointEarned.Invoke(value);
            return true;
        }

        return false;
    }
    /// <summary>
    /// Busca una casilla vacia
    /// </summary>
    /// <returns></returns>
    private Vector2Int? Get_FreeLocation()
    {
        // obtengo las coordenadas del tablero que no estan disponibles
        var locationsBlock = lstBlocks.Select(x => x.location).ToList();

        for (int x = 4 - 1; x >= 0; x--)
            for (int y = 4 - 1; y >= 0; y--)
            {
                var location = new Vector2Int(x, y);
                if (!locationsBlock.Contains(location))
                    return location; // retorna la coordenada que no este ocupando ningun bloque
            }

        return null; // si no hay casillas vacias se termina el juego
    }
    /// <summary>
    /// Agrega un nuevo boque en la celda indicada
    /// </summary>
    /// <param name="location">Valor del bloque</param>
    /// <param name="value">Valor del bloque</param>
    private void Add_Block(Vector2Int location, int value)
    {
        var obj = Instantiate(BlocskDic[value], BlocksRoot.transform);
        obj.transform.localPosition = new Vector3(location.x * 2, location.y * 2);

        var block = obj.GetComponent<Block>();
        block.location = location;
        block.Value = value;
        this.lstBlocks.Add(block);
    }
    /// <summary>
    /// Desplaza los bloques del tablero
    /// </summary>
    /// <param name="direction">Direccion de desplazamiento</param>
    private void Move(Direction direction)
    {
        var displacement = Vector2Int.zero;
        List<Block> _blocks = null;

        switch (direction)
        {
            case Direction.Left:
                displacement = new Vector2Int(0, -1);
                _blocks = lstBlocks.OrderBy(obj => obj.location.x).OrderBy(obj => obj.location.y).ToList();
                break;
            case Direction.Right:
                displacement = new Vector2Int(0, 1);
                _blocks = lstBlocks.OrderByDescending(obj => obj.location.x).OrderBy(obj => obj.location.y).ToList();
                break;
            case Direction.Up:
                displacement = new Vector2Int(1, 0);
                _blocks = lstBlocks.OrderBy(obj => obj.location.x).OrderByDescending(obj => obj.location.y).ToList();
                break;
            case Direction.Down:
                displacement = new Vector2Int(-1, 0);
                _blocks = lstBlocks.OrderBy(obj => obj.location.x).OrderBy(obj => obj.location.y).ToList();
                break;
            default:
                break;
        }

        _blocks.ForEach(b =>
        {
            var _targetLocation = Get_TargetLocation(displacement, b); // busca la coordenada del tablero libre mas cercana
            if (_targetLocation.HasValue)
                b.Move(_targetLocation.Value); // inicia el desplazamiento del bloque al nuevo destino
        });

        if (_blocks.Any(x => x.state == BlockState.Moving))
            this.State = LevelState.Moving; // indica que hay bloques en movimiento
    }
    /// <summary>
    /// Obtiene la celda destino del bloque
    /// </summary>
    /// <param name="displacement">Valor del desplazamiento entre celdas</param>
    /// <param name="currentLocation">Coordenadas actuales del bloque a desplazar</param>
    /// <param name="value">Valor del bloque</param>
    /// <returns></returns>
    private Vector2Int? Get_TargetLocation(Vector2Int displacement, Block block)
    {
        Vector2Int? emptyLocation = null;
        int x = block.location.x;
        int y = block.location.y;

        while (true)
        {
            x += displacement.y;
            y += displacement.x;
            if (x < 0 || x > 3 || y < 0 || y > 3)
                break;

            var location = new Vector2Int(x, y);
            var _blocks = lstBlocks.Where(x => x.location == location).ToList(); // bloque encontrado en el recorrido de desplazamiento
                                                                                 // si no hay bloques asume que la casilla esta libre
            if (_blocks.Count == 0)
                emptyLocation = location; // coordenada libre, siguie buscando en la siguiente
            else if (_blocks.Count == 1)
            {
                var targetBlock = _blocks.First();
                if (targetBlock != null)
                {
                    if (targetBlock.state == BlockState.Idle && targetBlock.Value == block.Value)
                        return targetBlock.location; // retorna la coordenada de un bloque de igual valor
                    else
                        break; // el bloque tiene otro valor
                }
            }
            // si hay 2 bloques asume que esta pendiente la union de los mismos, por lo tanto no esta disponible
            else if (_blocks.Count > 1)
                break;
        }

        return emptyLocation;
    }
    /// <summary>
    /// Determina si todos los bloques estan quietos
    /// </summary>
    /// <returns></returns>
    private bool Check_Idle()
    {
        return !lstBlocks.Any(x => x.state != BlockState.Idle);
    }
    /// <summary>
    /// suma los bloque que se encuentran en la misma ubicacion
    /// </summary>
    private void Merge_Block()
    {
        var lstMerge = lstBlocks.Where(x => x.state == BlockState.Idle).GroupBy(x => x.location).Where(x => x.Count() > 1).ToList();
        lstMerge.ForEach(group =>
        {
            var block1 = group.First();
            var block2 = group.Last();

            Add_Block(block1.location, block2.Value * 2); // crea un nuevo bloque duplicando su valor

            Destroy(block1.gameObject); // elimina un bloque
            Destroy(block2.gameObject); // elimina un bloque

            lstBlocks.Remove(block1);
            lstBlocks.Remove(block2);
        });
    }
    /// <summary>
    /// Valida si si termino el juego
    /// </summary>
    /// <returns></returns>
    private bool Check_GameOver()
    {
        if (lstBlocks.Count == 16)
        {
            foreach (Block b in lstBlocks)
            {
                if (Check_BlockValue(b.location + Vector2Int.up, b.Value))
                    return false; // encontro un movimiento disponible en el bloque de arriba
                if (Check_BlockValue(b.location + Vector2Int.down, b.Value))
                    return false; // encontro un movimiento disponible en el bloque de abajo
                if (Check_BlockValue(b.location + Vector2Int.left, b.Value))
                    return false; // encontro un movimiento disponible en el bloque de la izquierda
                if (Check_BlockValue(b.location + Vector2Int.right, b.Value))
                    return false; // encontro un movimiento disponible en el bloque de la derecha
            }
            return true; // no encontro movimiento disponible --> Game Over
        }

        return false; // no es game over
    }
    /// <summary>
    /// Determina si existe un bloque para la ubicacion y valor indicado
    /// </summary>
    /// <param name="location">Ubicacion del bloque a buscar</param>
    /// <param name="value">Valor del bloque a validar</param>
    /// <returns></returns>
    private bool Check_BlockValue(Vector2Int location, int value)
    {
        return lstBlocks.Any(obj => obj.location == location && obj.Value == value);
    }
    #endregion
}
public enum LevelState
{
    New,
    Idle,
    Moving,
    GameOver
}
public enum Direction
{
    Left,
    Right,
    Up,
    Down
}