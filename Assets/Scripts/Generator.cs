﻿using UnityEngine;
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
            Gizmos.DrawLine(connection[0].position, connection[1].position);
        }
    }

    //Attributes
    public System.Collections.Generic.List<Room> rooms = new System.Collections.Generic.List<Room>();
    public System.Collections.Generic.List<Door> doors = new System.Collections.Generic.List<Door>();
    public int amountOfRooms = 0;
    public Plane basePlane;

    private RandomUtility randomUtility;

    //Methods
    void Start () {
        randomUtility = gameObject.AddComponent<RandomUtility>();
        StartCoroutine(CreateRooms(amountOfRooms));
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
        int roomsCreated = 0;
        while(roomsCreated < amount)
        {
            Room newRoom = CreateRoom(randomUtility.RandomVector(1f, 5f), randomUtility.RandomVector(-10f, 10f));
            Vector3 direction = randomUtility.RandomDirectionLRUD();
            yield return new WaitForSeconds(.01f);
            while (newRoom.collides)
            {
                newRoom.Push(direction);
                yield return null;
            }
            roomsCreated++;
        }
        yield return null;
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
            FromRoom.doors.Add(doors[doors.Count-1]);
            ToRoom.doors.Add(doors[doors.Count-1]);
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
        return (otherPos - pos).normalized;
    }

}