using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

        public Door(Room room1, Room room2)
        {
            connection[0] = room1;
            connection[1] = room2;
        }

        public void Destroy()
        {
            Object.Destroy(this);
        }

        public void DrawLine()
        {
            Gizmos.DrawLine(connection[0].position, connection[1].position);
        }
    }

    //Attributes
    public System.Collections.Generic.List<Room> rooms = new System.Collections.Generic.List<Room>();
    public System.Collections.Generic.List<Door> doors = new System.Collections.Generic.List<Door>();

    //Methods
    void Start () {
        Room room1 = CreateRoom(new Vector3(2, 2, 2), new Vector3(0, 0, 0));
        Room room2 = CreateRoom(new Vector3(1, 1, 1), new Vector3(0, 0, -2));
        room1.AddNeighbour(room2);
        AddDoor(room1, room2);
        Room room3 = CreateRoom(new Vector3(1, 1, 1), new Vector3(-2, 0, 0));
        room1.AddNeighbour(room3);
        AddDoor(room1, room3);
        doors.RemoveAt(doors.Count - 1);
	}

    void Update () {
        
	}
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        foreach(Room r in rooms)
        {
            Gizmos.DrawCube(r.position, r.size);
        }
        Gizmos.color = Color.red;
        foreach (Door d in doors)
        {
            d.DrawLine();
        }
    }

    // Neuen Raum erzeugen: Leeres GO anlegen & Room anhängen. Room Attribute setzen.
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

    public void AddDoor(Room FromRoom, Room ToRoom)
    {
        if (FromRoom.neighbours.Contains(ToRoom))
        {
            Door door = new Door(FromRoom, ToRoom);
            doors.Add(door);
            FromRoom.doors.Add(doors[doors.Count-1]);
            ToRoom.doors.Add(doors[doors.Count-1]);
        }
        else
        {
            throw new System.Exception("ERROR: Trying to add door between non-adjacent rooms!");
        }
    }

}
