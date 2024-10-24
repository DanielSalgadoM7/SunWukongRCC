using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class MapBuilder : SingletonScript<MapBuilder>
{
    public Dictionary<string, Sala> mapBuilderRoomDictionary = new Dictionary<string, Sala>();
    private Dictionary<string, RoomTemplateSO> roomTemplateDictionary = new Dictionary<string, RoomTemplateSO>();
    private List<RoomTemplateSO> roomTemplateList = null;
    private RoomNodeTypeListSO roomNodeTypeList;
    private bool mapBuildSuccessful;

    protected override void Awake()
    {
        base.Awake();

        // Load the room node type list
        LoadRoomNodeTypeList();

        // GameResources.Instance.dimmedMaterial.SetFloat("Alpha_Slider", 1f);

    }

    private void LoadRoomNodeTypeList()
    {
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    public bool GenerateDungeon(NivelMapaSO currentDungeonLevel)
    {
        roomTemplateList = currentDungeonLevel.roomTemplateList;

        // Load the scriptable object room templates into the dictionary
        LoadRoomTemplatesIntoDictionary();
        mapBuildSuccessful = false;
        int dungeonBuildAttempts = 0;

        while (!mapBuildSuccessful && dungeonBuildAttempts < Settings.maxMapaBuild)
        {
            dungeonBuildAttempts++;
            // Select a random room node graph from the list
            RoomNodeGraphSO roomNodeGraph = SelectRandomRoomNodeGraph(currentDungeonLevel.roomNodeGraphList);
            int dungeonRebuildAttemptsForNodeGraph = 0;
            mapBuildSuccessful = false;

            // Loop until dungeon successfully built or more than max attempts for node graph
            while (!mapBuildSuccessful && dungeonRebuildAttemptsForNodeGraph <= Settings.maxRebuildGrafos)
            {
                // Clear dungeon room gameobjects and dungeon room dictionary
                ClearDungeon();

                dungeonRebuildAttemptsForNodeGraph++;

                // Attempt To Build A Random Dungeon For The Selected room node graph
                mapBuildSuccessful = AttemptToBuildRandomDungeon(roomNodeGraph);
            }
            if (mapBuildSuccessful)
            {
                // Instantiate Room Gameobjects
                InstantiateRoomGameobjects();
            }
        }
        return mapBuildSuccessful;
    }

    private void LoadRoomTemplatesIntoDictionary()
    {
        // Clear room template dictionary
        roomTemplateDictionary.Clear();

        // Load room template list into dictionary
        foreach (RoomTemplateSO roomTemplate in roomTemplateList)
        {
            if (!roomTemplateDictionary.ContainsKey(roomTemplate.guid))
            {
                roomTemplateDictionary.Add(roomTemplate.guid, roomTemplate);
            }
            else
            {
                Debug.Log("Duplicate Room Template Key In " + roomTemplateList);
            }
        }
    }

    private bool AttemptToBuildRandomDungeon(RoomNodeGraphSO roomNodeGraph)
    {
        // Create Open Room Node Queue
        Queue<RoomNodeSO> openRoomNodeQueue = new Queue<RoomNodeSO>();
        RoomNodeSO entranceNode = roomNodeGraph.GetRoomNode(roomNodeTypeList.lista.Find(x => x.isEntrada));
        if (entranceNode != null)
        {
            openRoomNodeQueue.Enqueue(entranceNode);
        }
        else
        {
            Debug.Log("No Entrance Node");
            return false;
        }
        bool noRoomOverlaps = true;
        noRoomOverlaps = ProcessRoomsInOpenRoomNodeQueue(roomNodeGraph, openRoomNodeQueue, noRoomOverlaps);
        if (openRoomNodeQueue.Count == 0 && noRoomOverlaps)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool ProcessRoomsInOpenRoomNodeQueue(RoomNodeGraphSO roomNodeGraph, Queue<RoomNodeSO> openRoomNodeQueue, bool noRoomOverlaps)
    {
        while (openRoomNodeQueue.Count > 0 && noRoomOverlaps == true)
        {
            RoomNodeSO roomNode = openRoomNodeQueue.Dequeue();
            foreach (RoomNodeSO childRoomNode in roomNodeGraph.GetNodeSalaFilho(roomNode))
            {
                openRoomNodeQueue.Enqueue(childRoomNode);
            }
            if (roomNode.roomNodeType.isEntrada)
            {
                RoomTemplateSO roomTemplate = GetRandomRoomTemplate(roomNode.roomNodeType);

                Sala room = CreateRoomFromRoomTemplate(roomTemplate, roomNode);

                room.isPosicionado = true;

                mapBuilderRoomDictionary.Add(room.id, room);
            }
            else
            {

                Sala parentRoom = mapBuilderRoomDictionary[roomNode.idListaNodePai[0]];

                noRoomOverlaps = CanPlaceRoomWithNoOverlaps(roomNode, parentRoom);
            }
        }
        return noRoomOverlaps;
    }

    private bool CanPlaceRoomWithNoOverlaps(RoomNodeSO roomNode, Sala parentRoom)
    {
        bool roomOverlaps = true;
        while (roomOverlaps)
        {
            List<Doorway> unconnectedAvailableParentDoorways = GetUnconnectedAvailableDoorways(parentRoom.doorWayList).ToList();
            if (unconnectedAvailableParentDoorways.Count == 0)
            {
                return false;
            }
            Doorway doorwayParent = unconnectedAvailableParentDoorways[UnityEngine.Random.Range(0, unconnectedAvailableParentDoorways.Count)];
            RoomTemplateSO roomtemplate = GetRandomTemplateForRoomConsistentWithParent(roomNode, doorwayParent);
            Sala room = CreateRoomFromRoomTemplate(roomtemplate, roomNode);
            if (PlaceTheRoom(parentRoom, doorwayParent, room))
            {
                roomOverlaps = false;
                room.isPosicionado = true;
                mapBuilderRoomDictionary.Add(room.id, room);

            }
            else
            {
                roomOverlaps = true;
            }
        }
        return true;
    }

    private RoomTemplateSO GetRandomTemplateForRoomConsistentWithParent(RoomNodeSO roomNode, Doorway doorwayParent)
    {
        RoomTemplateSO roomtemplate = null;
        if (roomNode.roomNodeType.isCorredor)
        {
            switch (doorwayParent.orientation)
            {
                case Orientation.norte:
                case Orientation.sul:
                    roomtemplate = GetRandomRoomTemplate(roomNodeTypeList.lista.Find(x => x.isCorredorNS));
                    break;


                case Orientation.leste:
                case Orientation.oeste:
                    roomtemplate = GetRandomRoomTemplate(roomNodeTypeList.lista.Find(x => x.isCorredorLO));
                    break;


                case Orientation.none:
                    break;

                default:
                    break;
            }
        }
        else
        {
            roomtemplate = GetRandomRoomTemplate(roomNode.roomNodeType);
        }
        return roomtemplate;
    }

    private bool PlaceTheRoom(Sala parentRoom, Doorway doorwayParent, Sala room)
    {
        Doorway doorway = GetOppositeDoorway(doorwayParent, room.doorWayList);
        if (doorway == null)
        {
            doorwayParent.isUnavailable = true;
            return false;
        }
        Vector2Int parentDoorwayPosition = parentRoom.lowerBounds + doorwayParent.position - parentRoom.templateLowerBounds;

        Vector2Int adjustment = Vector2Int.zero;
        switch (doorway.orientation)
        {
            case Orientation.norte:
                adjustment = new Vector2Int(0, -1);
                break;

            case Orientation.leste:
                adjustment = new Vector2Int(-1, 0);
                break;

            case Orientation.sul:
                adjustment = new Vector2Int(0, 1);
                break;

            case Orientation.oeste:
                adjustment = new Vector2Int(1, 0);
                break;

            case Orientation.none:
                break;

            default:
                break;
        }
        room.lowerBounds = parentDoorwayPosition + adjustment + room.templateLowerBounds - doorway.position;
        room.upperBounds = room.lowerBounds + room.templateUpperBounds - room.templateLowerBounds;
        Sala overlappingRoom = CheckForRoomOverlap(room);
        if (overlappingRoom == null)
        {
            doorwayParent.isConnected = true;
            doorwayParent.isUnavailable = true;
            doorway.isConnected = true;
            doorway.isUnavailable = true;
            return true;
        }
        else
        {
            doorwayParent.isUnavailable = true;
            return false;
        }
    }

    private Doorway GetOppositeDoorway(Doorway parentDoorway, List<Doorway> doorwayList)
    {
        foreach (Doorway doorwayToCheck in doorwayList)
        {
            if (parentDoorway.orientation == Orientation.leste && doorwayToCheck.orientation == Orientation.oeste)
            {
                return doorwayToCheck;
            }
            else if (parentDoorway.orientation == Orientation.oeste && doorwayToCheck.orientation == Orientation.leste)
            {
                return doorwayToCheck;
            }
            else if (parentDoorway.orientation == Orientation.norte && doorwayToCheck.orientation == Orientation.sul)
            {
                return doorwayToCheck;
            }
            else if (parentDoorway.orientation == Orientation.sul && doorwayToCheck.orientation == Orientation.norte)
            {
                return doorwayToCheck;
            }
        }
        return null;
    }

    private Sala CheckForRoomOverlap(Sala roomToTest)
    {
        foreach (KeyValuePair<string, Sala> keyvaluepair in mapBuilderRoomDictionary)
        {
            Sala room = keyvaluepair.Value;

            if (room.id == roomToTest.id || !room.isPosicionado)
                continue;

            if (IsOverLappingRoom(roomToTest, room))
            {
                return room;
            }
        }
        return null;
    }

    private bool IsOverLappingRoom(Sala room1, Sala room2)
    {
        bool isOverlappingX = IsOverLappingInterval(room1.lowerBounds.x, room1.upperBounds.x, room2.lowerBounds.x, room2.upperBounds.x);
        bool isOverlappingY = IsOverLappingInterval(room1.lowerBounds.y, room1.upperBounds.y, room2.lowerBounds.y, room2.upperBounds.y);
        if (isOverlappingX && isOverlappingY)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool IsOverLappingInterval(int imin1, int imax1, int imin2, int imax2)
    {
        if (Mathf.Max(imin1, imin2) <= Mathf.Min(imax1, imax2))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private RoomTemplateSO GetRandomRoomTemplate(RoomNodeTypeSO roomNodeType)
    {
        List<RoomTemplateSO> matchingRoomTemplateList = new List<RoomTemplateSO>();
        foreach (RoomTemplateSO roomTemplate in roomTemplateList)
        {
            if (roomTemplate.roomNodeType == roomNodeType)
            {
                matchingRoomTemplateList.Add(roomTemplate);
            }
        }
        if (matchingRoomTemplateList.Count == 0)
            return null;
        return matchingRoomTemplateList[UnityEngine.Random.Range(0, matchingRoomTemplateList.Count)];
    }

    private IEnumerable<Doorway> GetUnconnectedAvailableDoorways(List<Doorway> roomDoorwayList)
    {
        // Loop through doorway list
        foreach (Doorway doorway in roomDoorwayList)
        {
            if (!doorway.isConnected && !doorway.isUnavailable)
                yield return doorway;
        }
    }

    private Sala CreateRoomFromRoomTemplate(RoomTemplateSO roomTemplate, RoomNodeSO roomNode)
    {
        Sala room = new Sala();
        room.templateID = roomTemplate.guid;
        room.id = roomNode.id;
        room.prefab = roomTemplate.prefab;
        room.roomNodeType = roomTemplate.roomNodeType;
        room.lowerBounds = roomTemplate.lowerBounds;
        room.upperBounds = roomTemplate.upperBounds;
        room.spawnPositionArray = roomTemplate.spawnPositionArray;
        room.templateLowerBounds = roomTemplate.lowerBounds;
        room.templateUpperBounds = roomTemplate.upperBounds;
        room.listaIdSalaFilho = CopyStringList(roomNode.idListaNodeFilho);
        room.doorWayList = CopyDoorwayList(roomTemplate.doorwayList);

        if (roomNode.idListaNodePai.Count == 0)
        {
            room.idSalaPai = "";
            room.isJaVisitado = true;

        }
        else
        {
            room.idSalaPai = roomNode.idListaNodePai[0];
        }

        return room;
    }

    private RoomNodeGraphSO SelectRandomRoomNodeGraph(List<RoomNodeGraphSO> roomNodeGraphList)
    {
        if (roomNodeGraphList.Count > 0)
        {
            return roomNodeGraphList[UnityEngine.Random.Range(0, roomNodeGraphList.Count)];
        }
        else
        {
            Debug.Log("No room node graphs in list");
            return null;
        }
    }

    private List<Doorway> CopyDoorwayList(List<Doorway> oldDoorwayList)
    {
        List<Doorway> newDoorwayList = new List<Doorway>();
        foreach (Doorway doorway in oldDoorwayList)
        {
            Doorway newDoorway = new Doorway();
            newDoorway.position = doorway.position;
            newDoorway.orientation = doorway.orientation;
            newDoorway.doorPrefab = doorway.doorPrefab;
            newDoorway.isConnected = doorway.isConnected;
            newDoorway.isUnavailable = doorway.isUnavailable;
            newDoorway.doorwayStartCopyPosition = doorway.doorwayStartCopyPosition;
            newDoorway.doorwayCopiaLarguraTile = doorway.doorwayCopiaLarguraTile;
            newDoorway.doorwayCopiaAlturaTile = doorway.doorwayCopiaAlturaTile;
            newDoorwayList.Add(newDoorway);
        }
        return newDoorwayList;
    }

    private List<string> CopyStringList(List<string> oldStringList)
    {
        List<string> newStringList = new List<string>();
        foreach (string stringValue in oldStringList)
        {
            newStringList.Add(stringValue);
        }
        return newStringList;
    }

    private void InstantiateRoomGameobjects()
    {
        // percorre todas as salas do mapa
        foreach (KeyValuePair<string, Sala> keyvaluepair in mapBuilderRoomDictionary)
        {
            Sala room = keyvaluepair.Value;

            Vector3 roomPosition = new Vector3(room.lowerBounds.x - room.templateLowerBounds.x, room.lowerBounds.y - room.templateLowerBounds.y, 0f);

            // sala instanciada
            GameObject roomGameobject = Instantiate(room.prefab, roomPosition, Quaternion.identity, transform);

            SalaInstanciada instantiatedRoom = roomGameobject.GetComponentInChildren<SalaInstanciada>();

            instantiatedRoom.sala = room;

            // inicia a sala instanciada
            instantiatedRoom.Initialise(roomGameobject);

            // Save gameobject reference.
            room.salaInstanciada = instantiatedRoom;
        }
    }

    public RoomTemplateSO GetRoomTemplate(string roomTemplateID)
    {
        if (roomTemplateDictionary.TryGetValue(roomTemplateID, out RoomTemplateSO roomTemplate))
        {
            return roomTemplate;
        }
        else
        {
            return null;
        }
    }

    public Sala GetRoomByRoomID(string roomID)
    {
        if (mapBuilderRoomDictionary.TryGetValue(roomID, out Sala room))
        {
            return room;
        }
        else
        {
            return null;
        }
    }

    private void ClearDungeon()
    {
        if (mapBuilderRoomDictionary.Count > 0)
        {
            foreach (KeyValuePair<string, Sala> keyvaluepair in mapBuilderRoomDictionary)
            {
                Sala room = keyvaluepair.Value;

                if (room.salaInstanciada != null)
                {
                    Destroy(room.salaInstanciada.gameObject);
                }
            }
            mapBuilderRoomDictionary.Clear();
        }
    }
}