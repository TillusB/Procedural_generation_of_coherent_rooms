using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// The Generator is the heart of the program.
/// It creates rooms in empty spaces, moves them together and decides where to put doors.
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
    public System.Collections.Generic.List<Room> rooms = new System.Collections.Generic.List<Room>();
    public System.Collections.Generic.List<Door> doors = new System.Collections.Generic.List<Door>();
    public int amountOfRooms = 0;
    public Vector2 minMaxRoomDimensions = Vector2.zero;
    public Vector2 minMaxWidth = Vector2.zero;
    public Vector2 minMaxHeight = Vector2.zero;
    public Plane basePlane;

    private bool generating = false;
    private RandomUtility randomUtility;

    //Methods
    void Start () {
        randomUtility = gameObject.AddComponent<RandomUtility>();
        StartCoroutine(CreateRooms(amountOfRooms));
        //StartCoroutine(TestNTimes(50));
    }

    void Update () {
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
        defaultRoom.AddComponent<Room>();
        Room room = defaultRoom.GetComponent<Room>();

        room.size = size;
        room.position = pos;
        rooms.Add(room);
        return room;
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
            Room newRoom = CreateRoom(randomUtility.RandomVector(minMaxRoomDimensions.x, minMaxRoomDimensions.y), 
                randomUtility.RandomVector(minMaxWidth.x, minMaxWidth.y, minMaxHeight.x, minMaxHeight.y));
            newRoom.gameObject.name = "Room" + roomsCreated;
            yield return null;
            Debug.Log("Room " + roomsCreated + " : " + newRoom.transform.rotation.ToString());
            while (newRoom.collides)
            {
                newRoom.Push(GetDirectionAwayFrom(newRoom.transform.position, newRoom.otherRoom.transform.position));
                Debug.Log("CreateRooms Push: " + newRoom.name + " collides with " + newRoom.otherRoom.ToString());
                yield return null;
            }
            roomsCreated++;
        }
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
         SetRoomsTrigger(false);
        while (!ready)
        {
            foreach (Room r in rooms)
            {
                while (r.collides)
                {
                    r.Push(GetDirectionAwayFrom(r.transform.position, r.otherRoom.transform.position));
                    yield return null;
                }
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
                Debug.Log("No Collisionswhatsoever" + CollidingRooms().Length);
            }
            /*else
            {
                ready = false;
            }*/
            else
            {
                ready = false;
                while (CollidingRooms().Length > 0)
                {
                    foreach(Room r in CollidingRooms())
                    {
                        r.collides = true;
                        if (r.collides)
                        {
                            r.Push(GetDirectionAwayFrom(r.transform.position, r.otherRoom.transform.position));
                            yield return null;
                        }
                    }
                }
            }
        }
        generating = false;
        SetRoomsTrigger(false);
        Debug.Log("YAY");
        StartCoroutine(PushRoomsToCenter());
    }

    /// <summary>
    /// Pushes all rooms towards the center room TBI
    /// </summary>
    /// <returns></returns>
    private IEnumerator PushRoomsToCenter()
    {
        while (generating)
        {
            yield return null;
        }
        Room biggest = getBiggestRoom();

        foreach(Room r in rooms)
        {
            if(r == biggest)
            {
                continue;
            }

            while (!r.collides)
            {
                
                r.Push(-GetDirectionAwayFrom(r.transform.position, biggest.transform.position));
                yield return null;
            }
            yield return null;
        }
        Debug.Log("Done");

        //TODO TEST TEST
        foreach(Room r in rooms)
        {
            foreach(Room n in r.neighbours)
            {
                AddDoor(r, n);
            }
        }
        yield return null;
    }

    void OnCollisionEnter(Collision coll)
    {
        Debug.Log("BOOP");
    }

    /// <summary>
    /// Creates n levels and saves them as prefabs.
    /// </summary>
    /// <param name="n">Amount of testruns</param>
    IEnumerator TestNTimes(int n)
    {
        for(int i = 0; i < n; i++)
        {
            StartCoroutine(CreateRooms(amountOfRooms));
            GameObject test = GameObject.Find("Rooms");
            while (generating)
            {
                yield return null;
            }
            PrefabUtility.CreatePrefab("Assets/Prefabs/" + test.name + i + ".prefab", test, ReplacePrefabOptions.Default);
            Debug.Log("Saved!");
            foreach(Transform child in test.GetComponentInChildren<Transform>())
            {
                Destroy(child.gameObject);
            }
        }
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
            throw new System.Exception("ERROR: Trying to add door between non-adjacent rooms!");
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
        List<Room> sortedRooms = rooms.OrderBy(x => (x.size.x * x.size.z)).ToList<Room>();
        Debug.Log("smallest : " + sortedRooms[0].name);
        Debug.Log("biggest : " + sortedRooms[sortedRooms.Count()-1]);
        return sortedRooms.Last<Room>();
    }
}