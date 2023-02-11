using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    public class Cell
    {
        public bool visited = false;
        public bool[] status = new bool[4];
    }

    [System.Serializable]
    public class Rule
    {
        public GameObject room;
        public Vector2Int minPosition;
        public Vector2Int maxPosition;

        public bool obligatory;

        public int ProbabilityOfSpawning(int x, int y)
        {
            // 0 - cannot spawn 1 - can spawn 2 - HAS to spawn
            
            if(x >= minPosition.x && x <= maxPosition.x && y >= minPosition.y && y <= maxPosition.y)
            {
                return obligatory ? 2 : 1;
            }
            
            return 0;
        }

    }

    public Vector2Int size;
    public int startPos = 0;
    public Rule[] rooms;
    public GameObject[] finalRooms;
    public GameObject previousReplacedRoom;
    public Vector2 offset;

    List<Cell> board;

    // Start is called before the first frame update
    void Start()
    {
        MazeGenerator();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void GenerateDungeon()
    {
        for(int i = 0; i < size.x; i++)
        {
            for(int j = 0; j < size.y; j++)
            {
                int RandomRoom = -1;
                List<int> availableRooms = new List<int>();

                for(int k = 0; k < rooms.Length; k++)
                {
                    int p = rooms[k].ProbabilityOfSpawning(i, j);

                    if(p == 2)
                    {
                        RandomRoom = k;
                        break;
                    } else if(p == 1)
                    {
                        availableRooms.Add(k);
                    }
                }

                if(RandomRoom == -1)
                {
                    if(availableRooms.Count > 0)
                    {
                        RandomRoom = availableRooms[Random.Range(0, availableRooms.Count)];
                    }
                    else
                    {
                        RandomRoom = 0;
                    }
                }


                var newRoom = Instantiate(rooms[RandomRoom].room, new Vector3(i * offset.x, 0, -j * offset.y), Quaternion.identity, transform).GetComponent<RoomBehaviour>();
                newRoom.UpdateRoom(board[i + j * size.x].status);

                newRoom.name += " " + i + "-" + j;
            }
        }
        List<GameObject> a = new List<GameObject>();
        foreach(Transform child in transform)
        {
            a.Add(child.gameObject);
        }
        for(int o = 0; o < finalRooms.Length; o++)
        {
        var replacedRoom = a[Random.Range(1, a.Count - 1)];
        List<GameObject> v = new List<GameObject>();
        
        foreach(GameObject gameObject in finalRooms)
        {
            v.Add(gameObject);
        }
        for(int f = 0; f < v.Count - 1; f++)
        {
            if(replacedRoom == v[f])
            {
                replacedRoom = a[Random.Range(1, a.Count - 1)];
                f = 0;
            }
        }

        
        if(replacedRoom != previousReplacedRoom)
        {
            finalRooms[o].GetComponent<RoomBehaviour>().UpdateRoom(replacedRoom.GetComponent<RoomBehaviour>().Status);
            Instantiate(finalRooms[o], replacedRoom.transform.position, Quaternion.identity, transform);
            Destroy(replacedRoom);
        }
        previousReplacedRoom = replacedRoom;
        }
    }

    void MazeGenerator()
    {
        board = new List<Cell>();

        for(int i = 0; i < size.x; i++)
        {
            for(int j = 0; j < size.y; j++)
            {
                board.Add(new Cell());
            }
        }

        int currentCell = startPos;

        Stack<int> path = new Stack<int>();

        int k = 0;

        while(k < 1000) // If dungeon is bigger than make this number bigger
        {
            k++;

            board[currentCell].visited = true;

            // Check the cell's neighbors
            List<int> neighbors = CheckNeighbors(currentCell);

            if(neighbors.Count == 0)
            {
                if(path.Count == 0)
                {
                    break;
                }
                else
                {
                    currentCell = path.Pop();
                }
            }
            else
            {
                path.Push(currentCell);

                int newCell = neighbors[Random.Range(0, neighbors.Count)];

                if(newCell > currentCell)
                {
                    // Down or right
                    if(newCell - 1 == currentCell)
                    {
                        board[currentCell].status[2] = true;
                        currentCell = newCell;
                        board[currentCell].status[3] = true;
                    }
                    else
                    {
                        board[currentCell].status[1] = true;
                        currentCell = newCell;
                        board[currentCell].status[0] = true;
                    }
                }
                else
                {
                    // Up or left
                    if(newCell + 1 == currentCell)
                    {
                        board[currentCell].status[3] = true;
                        currentCell = newCell;
                        board[currentCell].status[2] = true;
                    }
                    else
                    {
                        board[currentCell].status[0] = true;
                        currentCell = newCell;
                        board[currentCell].status[1] = true;
                    }
                }

            }

        }
        GenerateDungeon();
    }

    List<int> CheckNeighbors(int cell)
    {
        List<int> neighbors = new List<int>();

        // Check up neighbor
        if(cell - size.x >= 0 && !board[Mathf.FloorToInt(cell - size.x)].visited)
        {
            neighbors.Add(Mathf.FloorToInt(cell - size.x));
        }

        // Check down neighbor
        if(cell + size.x < board.Count && !board[Mathf.FloorToInt(cell + size.x)].visited)
        {
            neighbors.Add(Mathf.FloorToInt(cell + size.x));
        }

        // Check right neighbor
        if((cell + 1) % size.x != 0 && !board[Mathf.FloorToInt(cell + 1)].visited)
        {
            neighbors.Add(Mathf.FloorToInt(cell + 1));
        }

        // Check left neighbor
        if(cell % size.x != 0 && !board[Mathf.FloorToInt(cell - 1)].visited)
        {
            neighbors.Add(Mathf.FloorToInt(cell - 1));
        }
        
        return neighbors;
    }

}
