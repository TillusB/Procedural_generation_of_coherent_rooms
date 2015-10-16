using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Generator : MonoBehaviour {
    public List<Room> Rooms;
    public GameObject Base;
    private float height;
    private float width;
    private float x;
    private float y;

    // Use this for initialization
    void Start () {
        height = Base.transform.localScale.y;
        width = Base.transform.localScale.x;
        Test(5, Base);
        x = -width / 2;
        y = -height / 2;
	}
	
	// Update is called once per frame
	void Update () {
        checkNextPos();
	}



    void Test(int i, GameObject Base)
    {
        int roomscreated = 0;
        while(roomscreated < i)
        {
            var room = GameObject.Instantiate(Rooms[Random.Range(0, Rooms.Count - 1)]);
            findUnoccupiedPos(room);
            roomscreated++;
        }
    }


    private void findUnoccupiedPos(Room toBePlaced)
    {
        toBePlaced.gameObject.transform.position = new Vector3(-width / 2, -height / 2, 0);
        for(float y = -height/2; y <= height/2; y++)
        {
            for(float x = -width/2; x <= width/2; x++)
            {

                Debug.DrawLine(Vector3.zero, toBePlaced.transform.position);
                toBePlaced.transform.position = new Vector3(x, y, 0);
                if (toBePlaced.free)
                {
                    return;
                }
                else continue;
            }
        }

        if (!toBePlaced.free) Destroy(toBePlaced.gameObject);
    }
}
