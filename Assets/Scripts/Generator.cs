using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Generator : MonoBehaviour {
    private GameObject defaultRoom;
    public Room room;
    // Use this for initialization
    void Start () {
        Room room1 = CreateRoom(new Vector3(2, 2, 2), new Vector3(0, 0, 2));
        Room room2 = CreateRoom(new Vector3(1, 1, 1), new Vector3(0, 0, 0));
        room1.AddNeighbour(room2);
	}
	
	// Update is called once per frame
	void Update () {

	}

    public Room CreateRoom(Vector3 size, Vector3 pos)
    {
        defaultRoom = new GameObject();
        defaultRoom.AddComponent<Room>();
        room = defaultRoom.GetComponent<Room>();

        room.size = size;
        room.position = pos;
        return room;
    }

}
