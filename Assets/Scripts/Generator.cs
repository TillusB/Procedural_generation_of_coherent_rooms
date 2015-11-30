using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Generator : MonoBehaviour {
    public List<Room> rooms;
    public GameObject basePlane;
    public int roomCount;
    private float height;
    private float width;
    private float x;
    private float y;
    private int roomsCreated = 0;
    private Room currentRoom;

    // Use this for initialization
    void Start () {
<<<<<<< HEAD
       GameObject.Instantiate(basePlane);
        height = basePlane.transform.localScale.y*10;
        width = basePlane.transform.localScale.x*10;
        x = -width / 2;
        y = -height / 2;
        currentRoom = rooms[Random.Range(0,rooms.Count)];
=======

>>>>>>> oldstate
	}
	
	// Update is called once per frame
	void Update () {
<<<<<<< HEAD
        Debug.Log(x + ", " + y);
        if (roomsCreated < roomCount)
        {
            checkNextPos();
            
        }
        else Debug.Log("DONE");
=======

>>>>>>> oldstate
	}


    private void checkNextPos()
    {
        Collider[] c = Physics.OverlapSphere(new Vector3(x, y, 0), 1);
        if (c.Length > 1)
        {
            Debug.Log(c[0].gameObject.name);
            incrementPos();
            return;
        }
        else
        {
            GameObject.Instantiate(currentRoom, new Vector3(x,y,0), Quaternion.identity);
            Debug.Log("Instantiated at " + x + ", " + y);
            roomsCreated++;
            currentRoom = rooms[Random.Range(0, rooms.Count)];
            x = -width / 2;
            y = -height / 2;
            return;
        }
    }
    
    private void incrementPos()
    {
        if (x < width)
        {
            x++;
        }
        else
        {
            x = -width / 2;
            if (y < height)
            {
                y++;
            }
            else Debug.LogError("No free space found!");
        }
    }
}
