using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Room objects contain all information about a room.
/// Bound, position, collider (,renderer?), doors and neighbours.
/// </summary>
public class Room : MonoBehaviour {
    //Attributes
    public Room otherRoom = null;
    public Vector3 size = new Vector3 (10,10,10);
    public Vector3 position = new Vector3 (0,0,0);
    public System.Collections.Generic.List<Room> neighbours = new System.Collections.Generic.List<Room>();
    public System.Collections.Generic.List<Generator.Door> doors = new System.Collections.Generic.List<Generator.Door>();
    public bool collides = false;
    private Color color;
    public Rigidbody rb;
    public static Dictionary<string, int> roomTypes = new Dictionary<string, int>();
    public int type;
    public BoxCollider collider;
    public BoxCollider trigger;
    public bool wasChecked = false;

    //Methods
    void Start () {
        roomTypes.TryGetValue("undefined", out type);
        transform.parent = GameObject.Find("Rooms").transform; // Root Object um alle Collider einzublenden
        collider = gameObject.AddComponent<BoxCollider>();
        trigger = gameObject.AddComponent<BoxCollider>();
        rb = gameObject.AddComponent<Rigidbody>();
        rb.mass = 0;
        rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
        gameObject.transform.position = position;
        gameObject.transform.localScale = size;
        gameObject.transform.localScale.Set(gameObject.transform.localScale.x % 5, gameObject.transform.localScale.y, gameObject.transform.localScale.z % 5);
        trigger.isTrigger = true;
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    void Update () {
        if (!collides) color = Color.green;
        if (wasChecked) color = Color.blue;
        if(rb != null) rb.velocity = Vector3.zero;
        transform.rotation = Quaternion.Euler(Vector3.zero);
        gameObject.transform.position.Set(gameObject.transform.position.x - gameObject.transform.position.x%5, gameObject.transform.position.y, gameObject.transform.position.z - gameObject.transform.position.z%5);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = color;
        Gizmos.DrawCube(transform.position, size);
    }

    void OnTriggerEnter(Collider other)
    {
        color = Color.red;
        otherRoom = other.gameObject.GetComponent<Room>();
        AddNeighbour(otherRoom);
        collides = true;
    }

    void OnTriggerExit(Collider other)
    {
        color = Color.green;
        neighbours.Remove(other.gameObject.GetComponent<Room>());
        otherRoom = null;
        collides = false;
    }

    void OnTriggerstay(Collider other)
    {
        color = Color.red;
        otherRoom = other.gameObject.GetComponent<Room>();
        AddNeighbour(otherRoom);
        collides = true;
    }

    void OnCollisionEnter(Collision other)
    {
        color = Color.red;

        otherRoom = other.gameObject.GetComponent<Room>();
        AddNeighbour(otherRoom);
        collides = true;
    }

    void OnCollisionExit(Collision other)
    {
        color = Color.green;
        neighbours.Remove(other.gameObject.GetComponent<Room>());

        otherRoom = null;
        collides = false;
    }

    void OnCollisionstay(Collision other)
    {
        color = Color.red;

        otherRoom = other.gameObject.GetComponent<Room>();
        AddNeighbour(otherRoom);
        collides = true;
    }

    /// <summary>
    /// Adds a room to this rooms neighbours array.
    /// Conditionally recursive.
    /// </summary>
    /// <param name="newNeighbour">Room that should be added as a neighbour of this Room</param>
    public void AddNeighbour(Room newNeighbour)
    {
        if (!neighbours.Contains(newNeighbour))
        {
            neighbours.Add(newNeighbour);
        }
        if (!newNeighbour.neighbours.Contains(this))
        {
            newNeighbour.AddNeighbour(this);
        }
        else return;
    }

    /// <summary>
    /// Move this Room into given direction.
    /// Normalises direction Vector3 to make sure the room wont jump.
    /// </summary>
    /// <param name="direction">Vector3 that points in the direction this Room should move in</param>
    public void Push(Vector3 direction)
    {
        color = Color.yellow;
        transform.Translate(direction);
    }

    /// <summary>
    /// TODO: Set this Rooms type (public/private/open/???)
    /// </summary>
    /// <param name="type">TODO: Type of this Room</param>
    public void SetRoomType(string roomtype)
    {
        int temp;
        if (roomTypes.TryGetValue(roomtype, out temp))
        {
            this.type = temp;
        }
        else this.type = roomTypes["undefined"];
    }

    public bool CollidesWith(Room r)
    {
        if (otherRoom == r) return true;
        return false;
    }

    public bool ConnectedTo(Room r)
    {
        //Debug.Log("Is " + gameObject.name + " connected to " + r.name + "?");
        wasChecked = true;
        bool found = false;

        if (neighbours.Contains(r))
        {
            found = true;
            Debug.LogError("Found IT!");
        }
        else
        {
            foreach(Room currentNeighbour in neighbours)
            {
                if (!currentNeighbour.wasChecked)
                {
                    found = currentNeighbour.ConnectedTo(r);
                }
            }
        }
        Debug.Log(gameObject.name + " connected to " + r.name + ": " + found);
        //Debug.Break();
        return found;
    }
}