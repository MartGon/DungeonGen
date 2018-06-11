using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class DungeonGenerator : MonoBehaviour {

    // Seed
    public int seed;

    // Dimensions
    public int dungeonWidth;
    public int dungeonDepth;

    // Número máximo de bloqueos
    public int maxNumberOfBlocks;

    // Misc
    public GameObject floorCell;
    public GameObject corridorFloorCell;
    public GameObject wallModel;
    public GameObject corridorWallModel;
    public GameObject roofLightModel;
    public GameObject columnModel;
    public GameObject roofModel;
    public GameObject wallTop;
    public GameObject gateModel;
    public GameObject keyPrefab;
    public Camera miniMapCamera;

    public GameObject teleportExit;
    public GameObject RoomPrefab;
    public GameObject player;
    public GameObject enemy;
    public GameObject enemy2;
    public GameObject lootCrate;
    public GameObject weapon;

    // Interface
    public InterfaceController interfaceController;

    // Keep it simple
    GameObject corridors;
    public GameObject enemies;
    public GameObject keys;
    public GameObject gates;

    // Constants
    const float cellHeight = -0.01f;
    const int cellWidth = 6;
    const int cellDepth = 6;
    Vector3Int offsetVector = new Vector3Int(3, 0, 3);

    // Room Size
    public int minRoomWidth = 0;
    public int minRoomDepth = 0;
    public int maxRoomWidth = 0;
    public int maxRoomDepth = 0;

    // NavMesh
    GameObject dungeon;

    // Misc
    public bool debug;
    public int maxTreasureRooms;
    public int level;
    public MainMenu.DifficultyMode difficulty;

    // Line Type
    public enum LineType
    {
        HORIZONTAL,
        VERTICAL
    }

    // DungeonNodes
    List<DungeonNode> dungeonNodes = new List<DungeonNode>();

    // Dictionary with dungeonCells
    Dictionary<Vector3Int, FloorCell> dungeonCells = new Dictionary<Vector3Int, FloorCell>();

    // Corridors
    List<Corridor> corridorList = new List<Corridor>();

    // Dungeon Room
    List<Room> dungeonRooms = new List<Room>();

    // Main-Primigenial node
    DungeonNode mainNode; 

    void Start ()
    {
        if (!debug)
        {
            level = PlayerPrefs.GetInt("level");
            difficulty = (MainMenu.DifficultyMode)PlayerPrefs.GetInt("difficulty");
            seed = PlayerPrefs.GetInt("seed");
            if(seed != 0)
                Random.InitState(seed);

            PlayerPrefs.SetInt("seed", 0);

            interfaceController.updateLevelInfo(level, Random.seed);
            generateDungeonDimensionsByLevel(level);
            getMaxTreasureRoomsByLevel(level);
            loadPlayerFromPreviousLevel();
        }

        if(debug)
            Random.InitState(seed);

        float far = placeCamera();
        adaptPlayerCameraRenderDistance(far);
        generateDungeon();
    }
	
	// Update is called once per frame
	void Update ()
    {
	}

    float placeCamera()
    {
        float x = dungeonWidth * cellWidth / 2;
        float z = dungeonDepth * cellDepth / 2;
        Vector3 cameraPosition = new Vector3(x, 24, z);
        miniMapCamera.transform.position = cameraPosition;
        return miniMapCamera.orthographicSize= Mathf.Max(x, z);
    }

    void adaptPlayerCameraRenderDistance(float far)
    {
        Player mPlayer = player.GetComponent<Player>();
        Camera playerCamera = mPlayer.CameraGo.GetComponentInChildren<Camera>();
        playerCamera.farClipPlane = far;
    }

    void loadPlayerFromPreviousLevel()
    {
        // DontDestroy on load
        if (PlayerPrefs.GetInt("load") != 1)
        {
            GameObject.Destroy(player);
            player = GameObject.FindGameObjectWithTag("Player");
            Player mPlayer = player.GetComponent<Player>();
            mPlayer.interfaceController = GameObject.FindGameObjectWithTag("InterfaceController").GetComponent<InterfaceController>();
            mPlayer.interfaceController.updateAmmoDisplay(mPlayer.currentWeapon.weaponMagazineCount, mPlayer.currentWeapon.totalAmmo);
            mPlayer.interfaceController.updateHP(mPlayer.hpCount, mPlayer.hp);
            mPlayer.acquiredKeys = new List<char>();
        }
        else
        {
            PlayerPrefs.SetInt("load", 0);
            DontDestroyOnLoad(player);
        }
    }

    void generateDungeonDimensionsByLevel(int level)
    {
        // base stats
        int factor = Mathf.Max((int)difficulty + 1, 2);

        dungeonWidth = maxRoomWidth * factor + 1;
        dungeonDepth = maxRoomDepth * factor + 1;
        Debug.Log("La seed " + Random.seed);

        for (int i = 1; i < level; i += factor)
        {
            float randomIndex = Random.Range(0f, 1f);

            if (randomIndex > 0.5)
                dungeonWidth += (maxRoomWidth);
            else
                dungeonDepth += (maxRoomDepth);
        }

        maxNumberOfBlocks = level / 2;
    }

    void getMaxTreasureRoomsByLevel(int level)
    {
        maxTreasureRooms = Mathf.CeilToInt(level / 5);

        // Update
        maxTreasureRooms = 2;
    }

    void generateDungeon()
    {
        // Dungeon GO
        dungeon = new GameObject();
        dungeon.name = "Dungeon";

        // Corridors GO
        corridors = new GameObject();
        corridors.name = "Corridors";
        corridors.transform.SetParent(dungeon.transform);

        // Create starting node
        mainNode = new DungeonNode();
        mainNode.nodeLevel = 0;
        mainNode.startX = 0;
        mainNode.startZ = 0;
        mainNode.endX = dungeonWidth;
        mainNode.endZ = dungeonDepth;
        mainNode.NodeId = 'A';
        mainNode.nodeGameObject = new GameObject();
        mainNode.nodeGameObject.name = "Nodo A";
        mainNode.nodeGameObject.transform.SetParent(dungeon.transform);
        dungeonNodes.Add(mainNode);

        PerformanceController.DungeonPerformanceReport report = new PerformanceController.DungeonPerformanceReport();

        // Generamos el grafo
        System.DateTime date = System.DateTime.Now;
        generateDungeonGraph(mainNode);

        // Creamos las salas a partir del grafo
        createRoomsFromGraph();
        report.rooms += (float)(System.DateTime.Now - date).TotalMilliseconds;

        // Inicializamos el diccionario
        initDungeonCells();

        // Creamos lo pasillos entre las salas
        date = System.DateTime.Now;
        createCorridorsInBetweenNodes();
        report.corridors += (float)(System.DateTime.Now - date).TotalMilliseconds;

        // Spawn Rooms
        date = System.DateTime.Now;
        spawnRooms();
        report.rooms += (float)(System.DateTime.Now - date).TotalMilliseconds;

        // Spawn corridors
        date = System.DateTime.Now;
        spawnCorridors();
        report.corridors += (float)(System.DateTime.Now - date).TotalMilliseconds;

        // Buscamos intersecciones
        date = System.DateTime.Now;
        generateDungeonBlocks();
        report.block += (float)(System.DateTime.Now - date).TotalMilliseconds;

        date = System.DateTime.Now;
        placeTreasureRooms();

        placeEnemies();
        report.misc += (float)(System.DateTime.Now - date).TotalMilliseconds;

        PerformanceController.addReport(report);
    }

    GameObject createSciFiRoom(Room room)
    {
        GameObject roomGo = new GameObject();
        roomGo.name = "SciFiRoom";
        roomGo.transform.SetParent(dungeon.transform);

        foreach (FloorCell cell in room.floorCells.Values)
        {
            Vector3 cellPos = cell.position * cellWidth + offsetVector;
            GameObject.Instantiate(floorCell, cellPos, Quaternion.identity, roomGo.transform);
            GameObject.Instantiate(roofModel, cellPos, Quaternion.identity, roomGo.transform);
            
            if (cell.position.x == room.lastVert.x - 1)
            {
                if (dungeonCells.ContainsKey(cell.position + new Vector3Int(1, 0, 0)))
                    if (dungeonCells[cell.position + new Vector3Int(1, 0, 0)].type == FloorCell.FloorCellType.VOID_CELL)
                    {
                        Vector3 wallPos = (cell.position + new Vector3Int(1, 0, 1)) * cellWidth + offsetVector;
                        GameObject.Instantiate(wallModel, wallPos, Quaternion.Euler(0, 180, 0), roomGo.transform);
                        GameObject.Instantiate(wallTop, wallPos, Quaternion.Euler(0, 180, 0), roomGo.transform);
                    }
            }
            if (cell.position.x == room.firstVert.x + 1)
            {
                if (dungeonCells.ContainsKey(cell.position + new Vector3Int(-1, 0, 0)))
                    if (dungeonCells[cell.position + new Vector3Int(-1, 0, 0)].type == FloorCell.FloorCellType.VOID_CELL)
                    {
                        Vector3 wallPos = (cell.position + new Vector3Int(0, 0, 0)) * cellWidth + offsetVector;
                        GameObject.Instantiate(wallModel, wallPos, Quaternion.Euler(0, 0, 0), roomGo.transform);
                        GameObject.Instantiate(wallTop, wallPos, Quaternion.Euler(0, 0, 0), roomGo.transform);
                    }
            }
            if (cell.position.z == room.lastVert.z - 1)
            {
                if (dungeonCells.ContainsKey(cell.position + new Vector3Int(0, 0, 1)))
                    if (dungeonCells[cell.position + new Vector3Int(0, 0, 1)].type == FloorCell.FloorCellType.VOID_CELL)
                    {
                        Vector3 wallPos = (cell.position + new Vector3Int(0, 0, 1)) * cellWidth + offsetVector;
                        GameObject.Instantiate(wallModel, wallPos, Quaternion.Euler(0, 90, 0), roomGo.transform);
                        GameObject.Instantiate(wallTop, wallPos, Quaternion.Euler(0, 90, 0), roomGo.transform);
                    }
            }
            if (cell.position.z == room.firstVert.z + 1)
            {
                if (dungeonCells.ContainsKey(cell.position + new Vector3Int(0, 0, -1)))
                {
                    if (dungeonCells[cell.position + new Vector3Int(0, 0, -1)].type == FloorCell.FloorCellType.VOID_CELL)
                    {
                        Vector3 wallPos = (cell.position + new Vector3Int(1, 0, 0)) * cellWidth + offsetVector;
                        GameObject.Instantiate(wallModel, wallPos, Quaternion.Euler(0, -90, 0), roomGo.transform);
                        GameObject.Instantiate(wallTop, wallPos, Quaternion.Euler(0, -90, 0), roomGo.transform);
                    }
                }
            }   
        }

        // Place Two First Columns
        Vector3 originOffset = new Vector3(1, 0, 1);
        Vector3 lastVertOffset = new Vector3(0, 0, 0);
        GameObject.Instantiate(columnModel, (room.firstVert + originOffset) * cellWidth + offsetVector + new Vector3Int(0, 0, -2), Quaternion.identity, roomGo.transform);
        GameObject.Instantiate(columnModel, (room.lastVert + lastVertOffset) * cellWidth + offsetVector + new Vector3Int(2, 0, 0), Quaternion.identity, roomGo.transform);

        // Place the other two columns
        Vector3 secondOffset = new Vector3(1, 0, 0);
        Vector3 thirdOffset = new Vector3(0, 0, 1);
        Vector3 secondVert = new Vector3(room.firstVert.x, 0, room.lastVert.z);
        Vector3 thirdVert = new Vector3(room.lastVert.x, 0, room.firstVert.z); ;
        GameObject.Instantiate(columnModel, (secondVert + secondOffset) * cellWidth + offsetVector, Quaternion.identity, roomGo.transform);
        GameObject.Instantiate(columnModel, (thirdVert + thirdOffset) * cellWidth + offsetVector + new Vector3Int(2, 0, -2), Quaternion.identity, roomGo.transform);
        return roomGo;
    }

    void generateDungeonGraph(DungeonNode mainNode)
    {
        Stack<DungeonNode> stack = new Stack<DungeonNode>();

        DungeonNode currentNode = mainNode;
        char lastNodeId = mainNode.NodeId;

        while (true)
        {
            while (currentNode.getWidth() > maxRoomWidth * 2 || currentNode.getDepth() > maxRoomDepth * 2)
            {
                //Debug.Log("El ancho es " + currentNode.getWidth() + " y el prof es " + currentNode.getDepth());

                // Choose line type TODO - Cambiar a 2
                LineType lineType = (LineType)Random.Range(0, 2);

                int minZ = currentNode.startZ + maxRoomDepth;
                int maxZ = currentNode.endZ - maxRoomDepth;
                int minX = currentNode.startX + maxRoomWidth;
                int maxX = currentNode.endX - maxRoomDepth;

                if (minZ > maxZ && minX < maxX)
                {
                    //Debug.Log("Se forzó vertical");
                    lineType = LineType.VERTICAL;
                }
                else if (minX > maxX && minZ < maxZ)
                {
                    //Debug.Log("Se forzó horizontal");
                    lineType = LineType.HORIZONTAL;
                }
                else
                {
                    //Debug.Log("Se eligió al azar");
                    lineType = (LineType)Random.Range(0, 2);
                }

                DungeonNode upperNode = new DungeonNode();
                DungeonNode lowerNode = new DungeonNode();
                List <DungeonNode> connections = new List<DungeonNode>();
                if (lineType == LineType.HORIZONTAL)
                {
                    int z = Random.Range(minZ, maxZ);
                    //Debug.Log("La linea H se traza por " + z + " con nodo iicial " + (minZ) + " y final " + (maxZ));

                    // X coordinates
                    upperNode.startX = currentNode.startX;
                    lowerNode.startX = currentNode.startX;
                    upperNode.endX = currentNode.endX;
                    lowerNode.endX = currentNode.endX;

                    // Z coordinates
                    upperNode.startZ = z;
                    upperNode.endZ = currentNode.endZ;
                    lowerNode.startZ = currentNode.startZ;
                    lowerNode.endZ = z;
                }
                else
                {
                    int x = Random.Range(minX, maxX);
                    //Debug.Log("La linea V se traza por " + x + " con nodo iicial " + (minX) + " y final " + (maxX));

                    // Z coordinates
                    upperNode.startZ = currentNode.startZ;
                    lowerNode.startZ = currentNode.startZ;
                    upperNode.endZ = currentNode.endZ;
                    lowerNode.endZ= currentNode.endZ;

                    // X coordinates
                    upperNode.startX = x;
                    upperNode.endX = currentNode.endX;
                    lowerNode.startX = currentNode.startX;
                    lowerNode.endX = x;
                }

                // set id
                lastNodeId++;
                upperNode.NodeId = lastNodeId;
                lastNodeId++;
                lowerNode.NodeId = lastNodeId;

                // Set parent
                upperNode.parent = currentNode;
                lowerNode.parent = currentNode;

                // Set bro
                upperNode.brother = lowerNode;
                lowerNode.brother = upperNode;

                // NodeLevel
                upperNode.nodeLevel = currentNode.nodeLevel + 1;
                lowerNode.nodeLevel = currentNode.nodeLevel + 1;

                // Creamos las conexiones
                connections.Add(upperNode);
                connections.Add(lowerNode);
                currentNode.children = connections;

                // Se crea el gameObject para mantener limpieza
                upperNode.nodeGameObject = new GameObject();
                lowerNode.nodeGameObject = new GameObject();
                upperNode.nodeGameObject.name = "Nodo " + upperNode.NodeId;
                lowerNode.nodeGameObject.name = "Nodo " + lowerNode.NodeId;
                upperNode.nodeGameObject.transform.SetParent(currentNode.nodeGameObject.transform);
                lowerNode.nodeGameObject.transform.SetParent(currentNode.nodeGameObject.transform);

                // Se añaden a la lista global
                dungeonNodes.Add(upperNode);
                dungeonNodes.Add(lowerNode);

                // Elegimos el nodo pequeño
                if (upperNode.getArea() < lowerNode.getArea())
                {
                    currentNode = upperNode;
                    stack.Push(lowerNode);
                }
                else
                {
                    currentNode = lowerNode;
                    stack.Push(upperNode);
                }
            }

            if (stack.Count == 0)
                return;

            currentNode = stack.Pop();
        }
    }

    void createRoomsFromGraph()
    {
        foreach(DungeonNode dungeonNode in dungeonNodes)
        {
            if(dungeonNode.children == null)
            {
                
                int finalWidth = Random.Range(minRoomWidth, maxRoomWidth);
                int finalDepth = Random.Range(minRoomDepth, maxRoomDepth);

                int firstVertX = Random.Range(dungeonNode.startX, dungeonNode.endX - finalWidth);
                int firstVertZ = Random.Range(dungeonNode.startZ, dungeonNode.endZ - finalDepth);


                // Create room object
                dungeonNode.room = new Room();
                dungeonNode.room.lastVert.x = firstVertX + finalWidth;
                dungeonNode.room.lastVert.z = firstVertZ + finalDepth;
                dungeonNode.room.firstVert.x = firstVertX; 
                dungeonNode.room.firstVert.z = firstVertZ;

                dungeonNode.addRoomToNode(dungeonNode.room);
                dungeonNode.room.parentNode = dungeonNode;
                dungeonRooms.Add(dungeonNode.room);
            }
        }
    }

    void spawnRooms()
    {
        foreach (DungeonNode dungeonNode in dungeonNodes)
        {
            if (dungeonNode.children == null)
            {
                dungeonNode.room.roomGameObject = createSciFiRoom(dungeonNode.room);
                dungeonNode.room.roomGameObject.name = "Room " + dungeonNode.NodeId;
                dungeonNode.room.roomGameObject.transform.SetParent(dungeonNode.nodeGameObject.transform);
            }
        }
    }

    void spawnCorridors()
    {
        foreach (FloorCell cell in dungeonCells.Values)
        {
            if (cell.type != FloorCell.FloorCellType.CORRIDOR_CELL)
                continue;

            // Corridor de la celda
            Corridor corridor = findCorridorOfCell(cell);

            GameObject prefabToUse = corridorFloorCell;
            GameObject.Instantiate(roofModel, cell.position * cellWidth + offsetVector, Quaternion.identity, corridor.corridorGameObject.transform);
            List<FloorCell> neis = getNeighbourList(cell, dungeonCells);
            List<FloorCell> corridorNeis = getNeighbourListOfType(cell, dungeonCells,FloorCell.FloorCellType.CORRIDOR_CELL);

            // Checkeamos si hace esquina
            if (corridorNeis.Count == 2)
            {
                FloorCell corridorCell1 = corridorNeis[0];
                FloorCell corridorCell2 = corridorNeis[1];
                Vector3Int diffWith1 = corridorCell1.position - cell.position;
                Vector3Int diffWith2 = corridorCell2.position - cell.position;

                //Debug.Log(diffWith1 + " vs " + diffWith2);

                if (Mathf.Abs(diffWith1.x) != Mathf.Abs(diffWith2.x))
                    prefabToUse = floorCell;
            }
            else if (corridorNeis.Count > 2)
            {
                prefabToUse = floorCell;
                cell.type = FloorCell.FloorCellType.ROOM_CELL;
            }

            bool firstTime = true;
            foreach (FloorCell nei in neis)
            {
                if (nei.type == FloorCell.FloorCellType.VOID_CELL)
                {
                    int rotationY = 0;
                    Vector3 diff = (nei.position - cell.position);
                    Vector3 offsetPosition = new Vector3(0, 0, 0);
                    Vector3 cellOffset = new Vector3Int(0, 0, 0);
                    if (diff.z == 1)
                    {
                        rotationY = 90;
                        cellOffset.z = 6;
                    }
                    else if (diff.z == -1)
                    {
                        rotationY = 90;
                        cellOffset.z = 6;
                        offsetPosition.z = 4;
                    }
                    else if (diff.x == 1)
                        offsetPosition.x = 2;
                    else
                        offsetPosition.x = 6;

                    if (firstTime)
                    {
                        GameObject.Instantiate(prefabToUse, cell.position * cellWidth + offsetVector + cellOffset, Quaternion.Euler(0, rotationY, 0), corridor.corridorGameObject.transform);
                        firstTime = false;
                    }
                    GameObject.Instantiate(corridorWallModel, cellPosToGlobal(nei.position) + offsetPosition, Quaternion.Euler(0, rotationY, 0), corridor.corridorGameObject.transform);
                    GameObject.Instantiate(wallTop, cellPosToGlobal(nei.position) + offsetPosition, Quaternion.Euler(0, rotationY, 0), corridor.corridorGameObject.transform);
                }
            }
        }
    }

    Corridor createCorridorsInBetween(Room room1, Room room2, DungeonNode endNode = null, DungeonNode startNode = null, int roomNumber = 1)
    {
        List<FloorCell> corridorCells = PathFinding.Astar(room1.getCentricFloorCell(), room2.getCentricFloorCell(), dungeonCells, endNode);
        List<FloorCell> filteredCorridorCells = new List<FloorCell>();

        foreach(FloorCell cell in corridorCells)
        {
            if (cell.type != FloorCell.FloorCellType.ROOM_CELL)
            {
                cell.type = FloorCell.FloorCellType.CORRIDOR_CELL;
                filteredCorridorCells.Add(cell);
            }
        }

        // Create the corridor object
        Corridor corridor = new Corridor();
        corridor.originRoom = room1;
        corridor.destRoom = room2;
        corridor.setCorridorCells(filteredCorridorCells);

        room1.exitCorridors.Add(corridor);
        room2.exitCorridors.Add(corridor);

        return corridor;
        //Debug.Log("Pasillos de la sala 1 " + room1.exitCorridors.Count + " pasillos de la sala 2 " + room2.exitCorridors.Count);
    }

    void initDungeonCells()
    {
        for (int x = 0; x < dungeonWidth; x++)
            for (int z = 0; z < dungeonDepth; z++)
            {
                Vector3Int pos = new Vector3Int(x, 0, z);
                FloorCell fCell = new FloorCell(pos);

                fCell.type = FloorCell.FloorCellType.VOID_CELL;
                foreach (DungeonNode node in dungeonNodes)
                {
                    if (node.room == null)
                        continue;

                    if (fCell.isWithinBoundaries(node.room.firstVert, node.room.lastVert))
                    {
                        fCell.type = FloorCell.FloorCellType.ROOM_CELL;
                        node.room.floorCells.Add(pos, fCell);
                        break;
                    }
                }

                dungeonCells.Add(pos, fCell);
            }
    }

    List<DungeonNode> getNodePair(ref List <DungeonNode> dungeonGraph)
    {
        List<DungeonNode> nodePair = new List<DungeonNode>();

        int maxNodeLevel = dungeonGraph.Max(x => x.nodeLevel);

        foreach (DungeonNode dungeonNode in dungeonGraph)
        {
            foreach (DungeonNode other in dungeonGraph)
            {
                if (dungeonNode.nodeLevel == other.nodeLevel && dungeonNode != other && dungeonNode.nodeLevel == maxNodeLevel)
                {
                    if (other.parent == dungeonNode.parent)
                    {
                        nodePair.Add(dungeonNode);
                        nodePair.Add(other);
                        //Debug.Log(maxNodeLevel);
                        return nodePair;
                    }
                }
            } 
        }

        //Debug.Log("No se encontró una pareja de nodos");
        return nodePair;
    }

    void createCorridorsInBetweenNodes()
    {
        List<DungeonNode> dungeonGraph = new List<DungeonNode>(dungeonNodes);

        int roomNumber = 1;
        while (dungeonGraph.Count > 0)
        {
            List<DungeonNode> nodePair = getNodePair(ref dungeonGraph);

            if(nodePair == null || nodePair.Count != 2)
            {
                //Debug.Log("Error en nodos");
                break;
            }

            Dictionary<float, List<Room>> roomPairByDistance = new Dictionary<float, List<Room>>();
            // Cogemos la pareja más cercana
            foreach(Room roomA in nodePair[0].roomsInNode)
                foreach (Room roomB in nodePair[1].roomsInNode)
                {
                    List<Room> roomPair = new List<Room>();
                    roomPair.Add(roomA);
                    roomPair.Add(roomB);
                    float distance = (roomA.getCentricFloorCell().position - roomB.getCentricFloorCell().position).magnitude;
                    if (!roomPairByDistance.ContainsKey(distance))
                        roomPairByDistance.Add(distance, roomPair);
                   /* else
                        Debug.Log("Ya lo contenía");*/
                }

            float index = roomPairByDistance.Keys.Min();
            Room room1 = roomPairByDistance[index][0];
            Room room2 = roomPairByDistance[index][1];

            Corridor corridor = createCorridorsInBetween(room1, room2, nodePair[1], nodePair[0]);

            // Para visualizar desde editor
            corridor.corridorGameObject = new GameObject();
            corridor.corridorGameObject.name = "Corridor " + nodePair[1].NodeId + " - " + nodePair[0].NodeId;
            corridor.corridorGameObject.transform.SetParent(corridors.transform);

            // Set corridors in nodes
            nodePair[0].corridorToBro = corridor;
            nodePair[1].corridorToBro = corridor;

            // Añadimos a la lista
            corridorList.Add(corridor);

            // Remove the nodes from the current graph
            dungeonGraph.Remove(nodePair[0]);
            dungeonGraph.Remove(nodePair[1]);

            roomNumber += 2;
        }
    }

    void placeEnemies()
    {
        if (difficulty == MainMenu.DifficultyMode.NAVIGATION)
            return;

            //int maxNodeLevel = dungeonNodes.Max(x => x.nodeLevel);
        int minEnemiesPerRoom = 1;
        int maxEnemiesPerRoom = 2 + (int)difficulty;

        foreach(Room room in dungeonRooms)
        {
            if(room.type != Room.RoomType.PLAYER_ROOM)
            {
                int enemiesNumber = Random.Range(minEnemiesPerRoom, maxEnemiesPerRoom);

                for(int i = 0; i < enemiesNumber; i++)
                {
                    // Posición
                    Vector3 pos = cellPosToGlobal(room.getCentricFloorCell().position);
                    int offsetX = Random.Range(-(room.getRoomWidth() / 2 - 1), room.getRoomWidth() / 2 - 1);
                    int offsetZ = Random.Range(-(room.getRoomDepth() / 2 - 1), room.getRoomDepth() / 2 - 1);
                    Vector3 offsetPos = new Vector3(offsetX, 0, offsetZ);
                    pos += offsetPos;

                    // Prefab
                    GameObject enemyToSpawn = enemy;
                    float randomValue = Random.Range(0, 1f);

                    if (randomValue < 0.25f)
                        enemyToSpawn = enemy2;
                    else
                        enemyToSpawn = enemy;

                    GameObject enemyGO = GameObject.Instantiate(enemyToSpawn, pos, Quaternion.identity, enemies.transform);
                    NavMeshAgent nav = enemyGO.GetComponent<NavMeshAgent>();
                    Unit unit = enemyGO.GetComponent<Unit>();
                    unit.homeRoom = room;

                    // Drop chances
                    unit.ammoDropChance *= 4 - (int)difficulty;

                    if (nav)
                    {
                        enemyGO.SetActive(true);
                        nav.enabled = true;
                    }
                }
            }
        }
    }

    public static List<FloorCell> getNeighbourList(FloorCell tile, Dictionary<Vector3Int, FloorCell> FloorCellDictionary, int distance = 1)
    {
        // Cogemos los vecinos
        List<FloorCell> neigbours = new List<FloorCell>();

        for (int x = -distance; x < distance + 1; x++)
            for (int y = -distance; y < distance + 1; y++)
                // Cogemos vecinos en cruz
                if (FloorCellDictionary.ContainsKey(new Vector3Int(x, 0, y) + tile.position) && Mathf.Abs(x) + Mathf.Abs(y) == 1)
                {
                    FloorCell nei = FloorCellDictionary[new Vector3Int(x, 0, y) + tile.position];
                    neigbours.Add(nei);
                }

        return neigbours;
    }

    public static bool hasNeighbourCellsOfType(FloorCell tile, Dictionary<Vector3Int, FloorCell> FloorCellDictionary, FloorCell.FloorCellType type, int distance = 1)
    {
        bool hasNeiCells = false;
        List<FloorCell> neigbours = new List<FloorCell>();

        neigbours = getNeighbourList(tile, FloorCellDictionary);

        foreach(FloorCell nei in neigbours)
        {
            if(nei.type == type)
            {
                hasNeiCells = true;
                break;
            }
        }

        return hasNeiCells;
    }

    public static List<FloorCell> getNeighbourListOfType(FloorCell tile, Dictionary<Vector3Int, FloorCell> FloorCellDictionary, FloorCell.FloorCellType floorCellType, int distance = 1)
    {
        // Cogemos los vecinos
        List<FloorCell> neigbours = new List<FloorCell>();

        for (int x = -distance; x < distance + 1; x++)
            for (int y = -distance; y < distance + 1; y++)
                // Cogemos vecinos en cruz
                if (FloorCellDictionary.ContainsKey(new Vector3Int(x, 0, y) + tile.position) && Mathf.Abs(x) + Mathf.Abs(y) == 1)
                {
                    FloorCell nei = FloorCellDictionary[new Vector3Int(x, 0, y) + tile.position];
                    if(nei.type == floorCellType)
                        neigbours.Add(nei);
                }

        return neigbours;
    }

    public Vector3 cellPosToGlobal(Vector3 cellPos)
    {
       return cellPos * cellWidth + offsetVector;
    }

    void blockTwoNodesPath(DungeonNode a, DungeonNode b)
    {
        if(a.brother != b || b.brother != a)
        {
            Debug.Log("Los nodos no son hermanos!!!");
            return;
        }

        Corridor corridorToBlock = a.corridorToBro;

        GameObject firstGate = GameObject.Instantiate(gateModel, cellPosToGlobal(corridorToBlock.firstCell.position) + getGateOffset(corridorToBlock.firstCell), getGateRotation(corridorToBlock.firstCell), gates.transform);
        GameObject lastGate = GameObject.Instantiate(gateModel, cellPosToGlobal(corridorToBlock.lastCell.position) + getGateOffset(corridorToBlock.lastCell), getGateRotation(corridorToBlock.lastCell), gates.transform);
        firstGate.gameObject.name = "Puerta " + a.NodeId + " - " + b.NodeId;
        lastGate.gameObject.name = "Puerta " + a.NodeId + " - " + b.NodeId;


        Gate firstGateO = firstGate.GetComponent<Gate>();
        Gate lastGateO = lastGate.GetComponent<Gate>();

        firstGateO.GateId = a.NodeId;
        lastGateO.GateId = a.NodeId;

        if(firstGateO == null || lastGateO == null)
        {
            Debug.Log("A las puertas les falta la script gate");
            return;
        }

        corridorToBlock.firstGate = firstGateO;
        corridorToBlock.lastGate = lastGateO;
    }

    Quaternion getGateRotation(FloorCell corridorCell)
    {
        Quaternion rotation = Quaternion.identity;

        List<FloorCell> neis = getNeighbourListOfType(corridorCell, dungeonCells, FloorCell.FloorCellType.ROOM_CELL);

        if(neis.Count != 1)
        {
            Debug.Log("Las celdas vecinas son distintas de 1");
            return rotation;
        }

        Vector3 diff = neis[0].position - corridorCell.position;

        if (Mathf.Abs(diff.x) == 1)
            rotation = Quaternion.Euler(0, -90, 0);

        return rotation;
    }

    Vector3Int getGateOffset(FloorCell corridorCell)
    {
        Vector3Int offset = Vector3Int.zero;

        List<FloorCell> neis = getNeighbourListOfType(corridorCell, dungeonCells, FloorCell.FloorCellType.ROOM_CELL);

        if (neis.Count != 1)
        {
            Debug.Log("Las celdas vecinas son distintas de 1");
            return offset;
        }

        Vector3 diff = neis[0].position - corridorCell.position;

        if (diff.x == 1)
            offset = new Vector3Int(6, 0, 0);
        else if(diff.z == 1)
            offset = new Vector3Int(0, 0, 6);

        return offset;
    }

    void placeKeyInRoom(Room room, DungeonNode nodeToBlock)
    {
        GameObject keyGo = GameObject.Instantiate(keyPrefab, cellPosToGlobal(room.getCentricFloorCell().position), Quaternion.identity, keys.transform);
        Key key = keyGo.GetComponent<Key>();
        room.type = Room.RoomType.KEY_ROOM;

        if(key == null)
        {
            Debug.Log("Falta la script en el prefab llave");
            return;
        }

        key.openGateId = nodeToBlock.NodeId;
    }

    // Métodos relacionados con el nuevo algoritmo de generación de bloqueos

    void generateDungeonBlocks()
    {
        // Paso 1 - Cogemo el nodo de mayor nivel que será nuestro inicio
        DungeonNode startingNode = getMaxLevelNode();
        Room startingRoom = startingNode.roomsInNode.First<Room>();
        DungeonNode nodeWithStartNode = null;

        if(mainNode.children == null)
        {
            Debug.Log("No se hizo ninguna división");
            return;
        }

        // Diferenciamos de los hijos del nodo primigenio, cual tiene el inicio
        if(mainNode.children.First<DungeonNode>().roomsInNode.Contains(startingRoom))
        {
            nodeWithStartNode = mainNode.children.First<DungeonNode>();
        }
        else
        {
            nodeWithStartNode = mainNode.children.Last<DungeonNode>();
        }

        // Paso 2 - Descendemos desde el padre descubriendo bloqueos
        Queue<List<DungeonNode>> pairQueue = new Queue<List<DungeonNode>>();
        List<DungeonNode> valuePair = mainNode.children;
        pairQueue.Enqueue(valuePair);

        List<DungeonNode> currentPair = null;
        int blockCount = 0;
        while(pairQueue.Count > 0 && blockCount < maxNumberOfBlocks)
        {
            startingRoom = startingNode.roomsInNode.First<Room>();
            currentPair = pairQueue.Dequeue();

            // Al guardar hijos nulos
            if(currentPair == null)
            {
                continue;
            }

            // Criterio para poner un bloqueo
            if (currentPair.First<DungeonNode>().children == null || currentPair.Last<DungeonNode>().children == null)
            {
                //Debug.Log("El nodo con el que trabajar es " + currentPair.First<DungeonNode>().NodeId + " y el o su bro no tienen hijos");
                continue;
            }

            //Debug.Log("El par actual es " + currentPair.First<DungeonNode>().NodeId + " - " + currentPair.Last<DungeonNode>().NodeId);

            // Paso 3 - Para cada bloqueo, elegimos el nodo para trabajar aquel que SÍ tiene el inicio
            DungeonNode nodeToWork = null;
            if(currentPair.First<DungeonNode>().roomsInNode.Contains(startingRoom))
            {
                startingRoom = startingNode.roomsInNode.First<Room>();
                nodeToWork = currentPair.First<DungeonNode>();
            }
            else if(currentPair.Last<DungeonNode>().roomsInNode.Contains(startingRoom))
            {
                startingRoom = startingNode.roomsInNode.First<Room>();
                nodeToWork = currentPair.Last<DungeonNode>();
            }

            // Paso 4 - En caso de que ninguno tenga inicio, hará de inicio el nodo que une al padre con el inicio
            // Para ello tenemos que hacer una busqueda recursiva hasta encontrar la sala que nos lleva al inicio y pertenezca a alguno de los nodos 
            // de la pareja actual
            else
            {
                //Debug.Log("Ninguno de los nodos contienen el inicio");

                DungeonNode parent = currentPair.First<DungeonNode>().parent;
                while(parent != null)
                {
                    DungeonNode parentBro = parent.brother;
                    if(parentBro.roomsInNode.Contains(startingRoom))
                    {
                        startingRoom = getRoomLeadingToStart(parent, startingRoom);

                        if (currentPair.First<DungeonNode>().roomsInNode.Contains(startingRoom))
                            nodeToWork = currentPair.First<DungeonNode>();
                        else if (currentPair.Last<DungeonNode>().roomsInNode.Contains(startingRoom))
                            nodeToWork = currentPair.Last<DungeonNode>();
                        else
                        {
                            parent = currentPair.First<DungeonNode>().parent;
                            continue;
                        }
                        Debug.Log("El nodo con el que trabajar es " + nodeToWork.NodeId + " para el bloqueo " + currentPair.First<DungeonNode>().NodeId + " - " + currentPair.Last<DungeonNode>().NodeId);
                        break;
                    }
                    parent = parentBro.parent;
                }
            }     

            // Paso 5 - Se elige de los hijos del nodo, aquel que NO tiene el inicio
            DungeonNode kidLessChild;

            if (nodeToWork.children != null)
            {
                if (!nodeToWork.children.First<DungeonNode>().roomsInNode.Contains(startingRoom))
                {
                    kidLessChild = nodeToWork.children.First<DungeonNode>();
                }
                else
                    kidLessChild = nodeToWork.children.Last<DungeonNode>();
            }
            else
            {
                kidLessChild = nodeToWork;
            }

            // Paso 6 - Se coloca una llave en alguno de sus nodos
            //Debug.Log("Se va a bloquear el enlace " + nodeToWork.brother.NodeId + " con " + nodeToWork.NodeId);

                // Bloqueamos el pasillo
            blockTwoNodesPath(nodeToWork.brother, nodeToWork);
            blockCount++;

            List<Room> possibleRooms = new List<Room>(kidLessChild.roomsInNode);
            possibleRooms.RemoveAll((x => x.type == Room.RoomType.KEY_ROOM));

            if(possibleRooms.Count == 0)
            {
                //Debug.Log("No hay salas válidas para colocar la llave, se utilizará cualquiera válida");
                possibleRooms = kidLessChild.roomsInNode;
            }

            int randomIndex = Random.Range(0, possibleRooms.Count);
            Room chosenRoom = possibleRooms[randomIndex];

            //Debug.Log("Ponemos una llave en " + chosenRoom.parentNode.NodeId);
            placeKeyInRoom(chosenRoom, nodeToWork.brother);

            // Paso 7 - Se añaden bloqueos a la lista
            pairQueue.Enqueue(nodeToWork.children);
            pairQueue.Enqueue(nodeToWork.brother.children);
        }

        // Place END
        //placeFinishRoom(finishNode);
        alternatePlaceFinishRoom(startingNode, nodeWithStartNode);

        // Place player
        placePlayer(startingNode);
    }

    DungeonNode getMaxLevelNode()
    {
        DungeonNode startingNode = null;

        List<DungeonNode> nodes = dungeonNodes;
        nodes.Sort((x, y) => y.nodeLevel.CompareTo(x.nodeLevel));
        startingNode = nodes.First<DungeonNode>();

        return startingNode;
    }

    void placeTreasureRooms()
    {
        foreach(Room room in dungeonRooms)
        {

            if (maxTreasureRooms == 0)
                return;

            if (room.exitCorridors.Count == 1 && room.type == Room.RoomType.STANDARD)
            {
                room.type = Room.RoomType.TREASURE_ROOM;
                maxTreasureRooms--;

                Vector3 pos = cellPosToGlobal(room.getCentricFloorCell().position);
                GameObject createGO = GameObject.Instantiate(lootCrate, pos, Quaternion.identity);
                Weapon weaponObject = weapon.GetComponent<Weapon>();
                weaponObject.weaponLevel = level;
                GameObject.Instantiate(weapon, pos + new Vector3(0, 1, 0), Quaternion.identity);
            }
        }
    }

    // Devuelve la habitación dentro del nodo dado que va hacia el hermano
    Room getRoomLeadingToStart(DungeonNode node, Room startingRoom)
    {
        Room leadingRoom = null;
        Room firstCandidate = node.corridorToBro.originRoom;
        Room secondCandidate = node.corridorToBro.destRoom;

        if (node.roomsInNode.Contains(firstCandidate))
            leadingRoom = firstCandidate;
        else
            leadingRoom = secondCandidate;

        return leadingRoom;
    }

    void placePlayer(DungeonNode startingNode)
    {
        Room playerRoom = startingNode.roomsInNode.First<Room>();
        playerRoom.type = Room.RoomType.PLAYER_ROOM;
        FloorCell cell = playerRoom.getCentricFloorCell();
        player.transform.position = cell.position * cellWidth + offsetVector + new Vector3Int(0, 4, 0);
        //Debug.Log("Hemos colocado al jugador en" + (cell.position * cellWidth + offsetVector + new Vector3Int(0, 4, 0)) + " en el nodo " + playerRoom.parentNode.NodeId);
    }

    void alternatePlaceFinishRoom(DungeonNode startNode, DungeonNode nodeWithStart = null)
    {
        DungeonNode finishNode = null;
        Room playerRoom = startNode.roomsInNode.First<Room>();

        foreach (DungeonNode node in dungeonNodes)
        {
            if (node.children != null)
                continue;

            if (node.roomsInNode.First<Room>() == playerRoom)
                continue;

            if (nodeWithStart.getTotalChildren().Contains(node))
                continue;


            Room startingRoom = playerRoom;
            //Debug.Log("El inicio es " + playerRoom.parentNode.NodeId);
            int iterationCount = 0;
            DungeonNode currentNode = node;
            while(currentNode != null)
            {
                DungeonNode bro = currentNode.brother;
                // ¿El nodo actual tiene el inciio?
                if(currentNode.roomsInNode.Contains(startingRoom))
                {
                    node.distanceToStartRoom = iterationCount;
                    //Debug.Log("El iterationCount para " + node.NodeId + " es " + node.distanceToStartRoom);

                    if (finishNode == null)
                        finishNode = node;
                    else if (finishNode.distanceToStartRoom < node.distanceToStartRoom)
                        finishNode = node;
                    else if (finishNode.distanceToStartRoom == node.distanceToStartRoom && finishNode.nodeLevel > node.nodeLevel)
                        finishNode = node;

                    break;
                }

                // ¿ Lo tiene el hermano ?
                if(bro.roomsInNode.Contains(startingRoom))
                {
                    startingRoom = getRoomLeadingToStart(currentNode, startingRoom);
                    iterationCount++;

                    // Reseteamos el padre
                    currentNode = node;
                }
                // Ascendemos a nuestro padre
                else
                {
                    currentNode = bro.parent;
                }
            }
        }

        Room finishRoom = finishNode.roomsInNode.First<Room>();
        finishRoom.type = Room.RoomType.FINISH_ROOM;
        //Debug.Log("La cantidad de rooms para poner el final es " + finishNode.roomsInNode.Count);
        FloorCell fCell = finishRoom.getCentricFloorCell();
        GameObject.Instantiate(teleportExit, fCell.position * cellWidth + offsetVector + new Vector3Int(0, 0, 0), Quaternion.identity);
        //Debug.Log("Hemos colocado el final en" + (fCell.position * cellWidth + offsetVector + new Vector3Int(0, 0, 0)) + " en el nodo " + finishRoom.parentNode.NodeId);
    }

    Corridor findCorridorOfCell(FloorCell cell)
    {
        if (cell.type != FloorCell.FloorCellType.CORRIDOR_CELL)
            return null;

        foreach (Corridor corridor in corridorList)
            if (corridor.floorCells.Values.Contains(cell))
                return corridor;

        return null;
    }
}

