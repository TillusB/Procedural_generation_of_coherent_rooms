using UnityEngine;
using System.Collections;

public class Room : MonoBehaviour {

    private System.Collections.Generic.Dictionary<string, int> roomType = new System.Collections.Generic.Dictionary<string, int>() {
        {"Public Room", 100},
        {"Private Room", 1},
        {"Hallway", 100},
    };

    public Vector3 size = new Vector3 (1,1,1);
    public Vector3 position = new Vector3 (0,0,0);
    public System.Collections.Generic.List<Room> neighbours = new System.Collections.Generic.List<Room>();

    void Start () {
        transform.parent = GameObject.Find("Rooms").transform;
        gameObject.AddComponent<MeshRenderer>();
        gameObject.AddComponent<BoxCollider>();
        gameObject.transform.position = position;
        gameObject.transform.localScale = size;
    }

    void Update () {
	}

    public void AddNeighbour(Room newNeighbour)
    {
        neighbours.Add(newNeighbour);
        if (!newNeighbour.neighbours.Contains(this))
        {
            newNeighbour.AddNeighbour(this);
        }
        else return;
    }

    public void SetRoomType(string type)
    {

    }
}
