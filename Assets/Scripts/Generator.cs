using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

/// <summary>
/// The Generator is the heart of the program.
/// It creates rooms in empty spaces, moves them together and decides where to put doors.
/// Manipulate parameters in the Inspector to fit the generator to your needs.
/// </summary>
public class Generator : MonoBehaviour {

    /// <summary>
    /// Doors are used to logically connect two adjacent rooms.
    /// They are basically tuples containing nothing but two rooms.
    /// </summary>
    public class Door : Object{
        private Room[] connection = new Room[2];

        /// <summary>
        /// Constructor for a Door.
        /// </summary>
        /// <param name="room1">First room of this connection</param>
        /// <param name="room2">Second room of this connection</param>
        public Door(Room room1, Room room2)
        {
            connection[0] = room1;
            connection[1] = room2;
        }
        /// <summary>
        /// Re´move this Door Object permanently.
        /// </summary>
        public void Destroy()
        {
            Object.Destroy(this);
        }

        public void DrawLine()
        {
            Gizmos.DrawLine(connection[0].transform.position, connection[1].transform.position);
        }
    }

    //Attributes
    public List<Room> rooms = new List<Room>();
    public List<Door> doors = new List<Door>();
    public int amountOfRooms = 0;
    public Vector2 minMaxRoomDimensions = Vector2.zero;
    public Vector2 minMaxWidth = Vector2.zero;
    public Vector2 minMaxHeight = Vector2.zero;
    private bool generating = true;
    private bool pushing = false;
    private RandomUtility randomUtility;
    public GameObject textObject;
    private Text loadingText;

    //Methods
    void Start ()
    {
        if (Room.roomTypes.Count == 0)
        {
            Room.roomTypes.Add("undefined", 0);
            Room.roomTypes.Add("public", 1);
            Room.roomTypes.Add("private", 2);
            Room.roomTypes.Add("entrance", 3);
        }
        randomUtility = gameObject.AddComponent<RandomUtility>();
        textObject = GameObject.Find("LoadingText");
        loadingText = textObject.GetComponent<Text>();
        loadingText.text = "...";
        StartCoroutine(Generate());
        //StartCoroutine(CreateRooms(amountOfRooms));
        //StartCoroutine(TestNTimes(10));
    }

    void Update()
    {

    }

    /// <summary>
    /// The main method.
    /// It calls the coroutines and makes sure, that everything is done before the next step.
    /// </summary>
    /// <returns></returns>
    private IEnumerator Generate()
    {
        loadingText.text = "Creating rooms..";
        StartCoroutine(CreateRooms(amountOfRooms));

        while (pushing || generating)
        {
            yield return null;
        }

        loadingText.text = "Creating public rooms..";
        ConnectPublicRooms();
        yield return null;

        while (pushing || rooms.Any<Room>(r => r.moving))
        {
            yield return null;
        }

        loadingText.text = "Attaching private rooms..";
        AttachPrivateRooms();
        yield return null;

        while (pushing || rooms.Any<Room>(r => r.moving))
        {
            yield return null;
        }

        loadingText.text = "Deciding on entrance..";
        SetEntrance();
        loadingText.text = "Generating doors..";
        AddDoors();

        loadingText.text = "Almost done..";

        foreach (Room r in rooms)
        {
            r.rb.isKinematic = false;
        }
        loadingText.text = "Done!";
        Make();
    }

    /// <summary>
    /// Method to add meshes etc to the Objects
    /// Modify to customize the visualisation of rooms and doors.
    /// </summary>
    private void Make()
    {
        foreach(Room r in rooms)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.GetComponent<Collider>().enabled = false;
            box.transform.localScale = r.transform.localScale;
            box.transform.position = r.transform.position;
            box.transform.parent = r.transform;
            box.GetComponent<Renderer>().material = new Material(Shader.Find("Diffuse"));
            box.GetComponent<Renderer>().material.color = Color.gray;

        }
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (Door d in doors)
        {
            d.DrawLine();
        }
    }

    /// <summary>
    /// Creates one room with given position vector and size vector.
    /// </summary>
    /// <param name="size">Defines the size of a room</param>
    /// <param name="pos">Defines the position of a room</param>
    /// <returns>Room</returns>
    public Room CreateRoom(Vector3 size, Vector3 pos)
    {
        GameObject defaultRoom = new GameObject();
        
        Room room = defaultRoom.AddComponent<Room>();

        room.size = size;
        room.position = pos;
        rooms.Add(room);
        return room;
    }

    public void Restart()
    {
        Application.LoadLevel(0);
    }

    /// <summary>
    /// Creates set amount of rooms of random sizes and positions.
    /// When rooms intersect (Room.collides) calls Room.Push(awayFromCollidingObject) until colliders dont intersect anymore.
    /// </summary>
    /// <param name="amount">Count of desired rooms</param>
    /// <returns>null</returns>
    IEnumerator CreateRooms(int amount)
    {
        generating = true;
        int roomsCreated = 0;
        while(roomsCreated < amount)
        {
            Room newRoom = CreateRoom(randomUtility.RandomVector((int)minMaxRoomDimensions.x, (int)minMaxRoomDimensions.y), 
                randomUtility.RandomVector((int)minMaxWidth.x, (int)minMaxWidth.y, (int)minMaxHeight.x, (int)minMaxHeight.y));
            newRoom.gameObject.name = "Room" + roomsCreated;
            yield return null;
            //Debug.Log("Room " + roomsCreated + " : " + newRoom.transform.rotation.ToString());
            while (newRoom.collides)
            {
                newRoom.Push(GetDirectionAwayFrom(newRoom.transform.position, newRoom.otherRoom.transform.position));
                yield return null;
            }
            roomsCreated++;
        }

        List<Room> sortedRooms = getRoomsSorted();
        int i = 0;

        foreach (Room r in sortedRooms) // Larger half of all rooms should be public rooms
        {
            if (i > Mathf.CeilToInt(sortedRooms.Count() / 2) && r.type == Room.roomTypes["undefined"]) {
                r.type = Room.roomTypes["public"];
            }
            else
            {
                if(r.type == Room.roomTypes["undefined"])
                    r.type = Room.roomTypes["private"];
            }
            i++;
        }
        //test:

        StartCoroutine(MoveAllRoomsToClear());
        yield return null;
    }

    /// <summary>
    /// Moves rooms apart after placement until no rooms collide anymore.
    /// </summary>
    /// <returns></returns>
    private IEnumerator MoveAllRoomsToClear()
    {
        bool ready = false;
        //SetRoomsTrigger(false);
        while (!ready)
        {
            foreach (Room r in rooms)
            {
                while (r.collides)
                {
                    r.moving = true;
                    r.Push(GetDirectionAwayFrom(r.transform.position, r.otherRoom.transform.position));
                    yield return null;
                }
                r.moving = false;
            }

            yield return null;

            if (CollidingRooms().Length == 0)
            {
                ready = true;
            }

            SetRoomsTrigger(true);

            yield return null;

            if (CollidingRooms().Length == 0)
            {
                ready = true;
                //Debug.Log("No Collisionswhatsoever" + CollidingRooms().Length);
            }
            /*else
            {
                ready = false;
            }*/
            else //Backup
            {
                ready = false;
                while (CollidingRooms().Length > 0)
                {
                    foreach(Room r in CollidingRooms())
                    {
                        if (r.collides)
                        {
                            if(r.otherRoom != null) r.Push(GetDirectionAwayFrom(r.transform.position, r.otherRoom.transform.position));
                            yield return null;
                        }
                    }
                }
            }
        }
        generating = false;
        SetRoomsTrigger(false);
        yield return null;
    }


    /// <summary>
    /// Pushes list of rooms to target room
    /// </summary>
    /// <param name="pushRooms">List of rooms to move</param>
    /// <param name="toTarget">target to move rooms to</param>
    /// <returns></returns>
    private IEnumerator PushRoomsToTarget(List<Room> pushRooms, Room toTarget, bool connectedIsOk = false)
    {
        while (generating)
        {
            yield return null; //skip frame until other Coroutine is done;
        }
        pushing = true;

        foreach(Room r in pushRooms)
        {
            if(r == toTarget)
            {
                r.moving = false;
                continue;
            }
            r.moving = true;
            float time = Time.deltaTime; //timer to stop infinite push
            while (true)
            {
                r.Push(-GetDirectionAwayFrom(r.transform.position, toTarget.transform.position));
                yield return null;
                if (r.collides && r.neighbours.Contains(toTarget))
                {
                    r.rb.isKinematic = true;
                    r.moving = false;
                    break;
                }
                if (connectedIsOk && r.ConnectedTo(toTarget))
                {
                    r.rb.isKinematic = true;
                    r.moving = false;
                    break;
                }
                time += Time.deltaTime;
                if (time >= 5f)
                {
                    if (!r.neighbours.Contains(toTarget) && r.collides){
                        r.transform.position = randomUtility.RandomVector(Mathf.FloorToInt(minMaxWidth.x)*2, Mathf.FloorToInt(minMaxWidth.y)*2, Mathf.FloorToInt(minMaxHeight.x)*2, Mathf.FloorToInt(minMaxHeight.y)*2);
                        time = Time.deltaTime;
                        if(r.type == Room.roomTypes["private"])
                        {
                            toTarget = FindClosestRoom(r, GetRoomsByType("public"));
                        }
                        //StartCoroutine(PushRoomsToTarget(pushRooms, toTarget)); // change pos and retry;
                        //StartCoroutine(PushRoomsToTarget(new List<Room>() { r }, toTarget));
                    }
                    continue;
                }
            }
            yield return null;
            pushing = false;
        }
        //Debug.Log("Done");

        //TODO TEST TEST
        //foreach (Room r in rooms)
        //{
        //    foreach (Room n in r.neighbours)
        //    {
        //        AddDoor(r, n);
        //    }
        //}
        //yield return null;
        //pushing = false;
    }

    /// <summary>
    /// Pushes all Rooms to target Vector3
    /// </summary>
    /// <param name="pushRooms">List of rooms to move</param>
    /// <param name="toTarget">target point to move rooms to</param>
    /// <returns></returns>
    private IEnumerator PushRoomsToTarget(List<Room> pushRooms, Vector3 toTarget, bool connectedIsOk = false)
    {
        while (generating || pushing)
        {
            yield return null;
        }
        pushing = true;
        while (generating)
        {
            yield return null; //skip frame until other Coroutine is done;
        }

        foreach (Room r in pushRooms)
        {
            if (r.myCollider.bounds.Contains(toTarget))
            {
                continue;
            }
            float time = Time.deltaTime; //timer to stop infinite push
            bool contains = false;

            while (true)
            {
                if (r.neighbours.Count() > 0)
                {
                    foreach (Room n in r.neighbours)
                    {
                        if (n.type == Room.roomTypes["public"] && n.myCollider.bounds.Contains(toTarget))
                        {
                            contains = true;
                        }else if(n.type == Room.roomTypes["private"])
                        {
                            n.Push(GetDirectionAwayFrom(r.transform.position, n.transform.position) * 15);
                        }
                    }
                }
                r.Push(-GetDirectionAwayFrom(r.transform.position, toTarget));
                yield return null;
                if (r.myCollider.bounds.Contains(toTarget) || contains)
                {
                    r.rb.isKinematic = true;
                    break;
                }
                time += Time.deltaTime;
                if (time >= 5f)
                {
                    break;
                }
            }
            yield return null;
        }
        pushing = false;
    }


    /// <summary>
    /// Get all public rooms and move them together
    /// </summary>
    private void ConnectPublicRooms()
    {
        
        List<Room> publicRooms = GetRoomsByType("public");
        if (publicRooms.Count > 1)
        {
            foreach (Room pRoom in GetRoomsByType("private"))
            {
                pRoom.gameObject.SetActive(false);
            }
            Vector3 middle = GetMiddlePointBetween(publicRooms);
            Room middleRoom = FindClosestRoom(middle, publicRooms);
            middleRoom.rb.isKinematic = true;
            foreach (Room pR in publicRooms)
            {
                pR.moving = true;
            }
            StartCoroutine(PushRoomsToTarget(publicRooms, middleRoom, true));
        }
    }

    private void AttachPrivateRooms()
    {
        pushing = true;
        foreach(Transform t in GameObject.Find("Rooms").transform)
        {
            t.gameObject.SetActive(true);
        }
        List<Room> privateRooms = GetRoomsByType("private");
        foreach(Room p in privateRooms)
        {
            if (!p.rb.isKinematic)
            {
                Room closestPublic = FindClosestRoom(p, GetRoomsByType("public"));
                StartCoroutine(PushRoomsToTarget(new List<Room>() { p }, closestPublic));
            }
        }
    }

    private void SetEntrance()
    {
        RoomWithLeastNeighbours(GetRoomsByType("public")).SetRoomType("entrance");
    }

    private Room RoomWithLeastNeighbours(List<Room> fromList)
    {
        float minNeighbours = Mathf.Infinity;
        Room hasLeast = null;
        foreach (Room room in fromList)
        {
            if (room.neighbours.Count() < minNeighbours)
            {
                hasLeast = room;
                minNeighbours = room.neighbours.Count;
            }
        }
        return hasLeast;
    }

    private void AddDoors()
    {
        foreach(Room getsDoor in rooms)
        {
            if(getsDoor.type == Room.roomTypes["public"])
            {
                foreach(Room neighbour in getsDoor.neighbours)
                {
                    if(neighbour.type == Room.roomTypes["public"] || neighbour.type == Room.roomTypes["entrance"])
                    {
                        AddDoor(getsDoor, neighbour);
                    }
                }
            }
            else
            {
                if(getsDoor.type == Room.roomTypes["private"])
                {
                    List<Room> candidates = GetRoomsByType("public", getsDoor.neighbours);
                    candidates.AddRange(GetRoomsByType("entrance", getsDoor.neighbours));
                    Room neighbour = RoomWithLeastNeighbours(candidates);
                    if (neighbour != null)
                    {
                        AddDoor(getsDoor, neighbour);
                    }
                    else
                    {
                        neighbour = FindClosestRoom(getsDoor, GetRoomsByType("public"));
                        getsDoor.AddNeighbour(neighbour);
                        AddDoor(getsDoor, neighbour);
                    }
                }
            }
        }
    }

    private Room FindClosestRoom(Room toRoom, List<Room> fromList)
    {
        Room closest = null;
        float minDist = Mathf.Infinity;
        foreach (Room someRoom in fromList)
        {
            float dist = Vector3.Distance(someRoom.transform.position, toRoom.transform.position);
            if (dist < minDist && someRoom != toRoom)
            {
                closest = someRoom;
                minDist = dist;
            }
        }
        return closest;
    }

    private Room FindClosestRoom(Vector3 toPos, List<Room> fromList)
    {
        Room closest = null;
        float minDist = Mathf.Infinity;
        foreach(Room someRoom in fromList)
        {
            float dist = Vector3.Distance(someRoom.transform.position, toPos);
            if(dist < minDist)
            {
                closest = someRoom;
                minDist = dist;
            }
        }
        return closest;
    }

    /// <summary>
    /// Returns a Vector3 in the middle between objects in the list
    /// </summary>
    /// <param name="objects">List of objects</param>
    /// <returns></returns>
    private Vector3 GetMiddlePointBetween(List<GameObject> objects)
    {
        Vector3 center = Vector3.zero;
        foreach(GameObject o in objects)
        {
            center += o.transform.position;
        }
        return center / objects.Count();
    }

    /// <summary>
    /// Returns a Vector3 in the middle between rooms in the list
    /// </summary>
    /// <param name="roomList">List of rooms</param>
    /// <returns></returns>
    private Vector3 GetMiddlePointBetween(List<Room> roomList)
    {
        List<GameObject> objectList = new List<GameObject>();
        foreach(Room r in roomList)
        {
            objectList.Add(r.gameObject);
        }
        return GetMiddlePointBetween(objectList);
    }

    void OnCollisionEnter(Collision coll)
    {
        //Debug.Log("Collision Enter");
    }

    /// <summary>
    /// Adds a logical connection (Door) between two rooms as long as they are defined as each others neighbours.
    /// Throws System.Exception.
    /// </summary>
    /// <param name="FromRoom">First room of desired connection</param>
    /// <param name="ToRoom">second room of desired connection</param>
    public void AddDoor(Room FromRoom, Room ToRoom)
    {
        if (FromRoom.neighbours.Contains(ToRoom))
        {
            Door door = new Door(FromRoom, ToRoom);
            doors.Add(door);
            FromRoom.doors.Add(doors[doors.Count - 1]);
            ToRoom.doors.Add(doors[doors.Count - 1]);
        }
        else
        {
            throw new System.Exception("ERROR: Trying to add door between non-adjacent rooms!\n Tried to connect " + FromRoom.name + " to " + ToRoom.name + "!");
        }
    }

    /// <summary>
    /// Returns a normalised Vector3 that is opposite to the vector between the two given positions.
    /// </summary>
    /// <param name="pos">Position of the object that should move away</param>
    /// <param name="otherPos">Position of the object it should move away from</param>
    /// <returns>Vector3</returns>
    private Vector3 GetDirectionAwayFrom(Vector3 pos, Vector3 otherPos)
    {
        return ((otherPos - pos) * -1).normalized;
    }

    private void SetRoomsTrigger(bool b)
    {
        foreach (Room r in rooms)
        {
            r.GetComponent<BoxCollider>().isTrigger = b;
        }
    }

    /// <summary>
    /// Array of type Room containing all currently colliding Rooms.
    /// </summary>
    /// <returns>Room[]</returns>
    private Room[] CollidingRooms()
    {
        List<Room> result = new List<Room>();
        foreach(Room r in rooms)
        {
            if (r.collides)
            {
                result.Add(r);
            }
        }
        return result.ToArray();
    }

    /// <summary>
    /// Returns the biggest room by sorting rooms by size
    /// </summary>
    /// <returns>Room</returns>
    private Room getBiggestRoom()
    {
        return getRoomsSorted().Last();
    }

    private List<Room> GetRoomsByType(string type)
    {
        List<Room> results = new List<Room>();
        foreach (Room r in rooms)
        {
            if(r.type == Room.roomTypes[type])
            {
                results.Add(r);
            }
        }
        return results;
    }

    private List<Room> GetRoomsByType(string type, List<Room> fromList)
    {
        List<Room> results = new List<Room>();
        foreach (Room r in fromList)
        {
            if (r.type == Room.roomTypes[type])
            {
                results.Add(r);
            }
        }
        return results;
    }

    private List<Room> getRoomsSorted()
    {
        return rooms.OrderBy(x => (x.size.x * x.size.z)).ToList<Room>();
    }

    //private void RoomsWereChecked(bool wasChecked) {
    //    foreach (Room r in rooms)
    //    {
    //        r.wasChecked = wasChecked;
    //    }
    //}

}