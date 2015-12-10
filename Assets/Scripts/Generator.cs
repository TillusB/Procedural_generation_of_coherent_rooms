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
    public Plane basePlane;

    private RandomUtility randomUtility;

    //Methods
    void Start () {
        randomUtility = gameObject.AddComponent<RandomUtility>();
        StartCoroutine(FindRandomPos());
	}
    int roomcount = 0;
    void Update () {
        if(roomcount <= 5)
        {
            StartCoroutine(FindRandomPos());
            roomcount++;
        }
	}
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.grey;
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

    IEnumerator FindRandomPos()
    {
        // Try random pos
        // For (as long as it doesnt work)
        //  Wait one frame
        //  Move if necessary
        // repeat
        Room newRoom = CreateRoom(randomUtility.RandomVector(1f, 5f), randomUtility.RandomVector(-10f, 10f));
        yield return null;
        //newRoom.transform.Translate(new Vector3(0, 10, 0));
        //yield return null;
        //newRoom.transform.Translate(new Vector3(0, -10, 0));
        Vector3 direction = randomUtility.RandomDirection();

        while (newRoom.collides)
        {
            newRoom.Push(direction);
            yield return null;
        }

        yield return null;
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
