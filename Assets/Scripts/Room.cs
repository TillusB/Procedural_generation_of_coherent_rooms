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
    private Dictionary<string, int> roomTypes = new Dictionary<string, int>();
    public int type = 0;

    //Methods
    void Start () {
        roomTypes.Add("undefined", 0);
        roomTypes.Add("public", 1);
        roomTypes.Add("private", 2);
        
        transform.parent = GameObject.Find("Rooms").transform; // Root Object um alle Collider einzublenden
        gameObject.AddComponent<MeshRenderer>();
        gameObject.AddComponent<BoxCollider>();
        rb = gameObject.AddComponent<Rigidbody>();
        rb.mass = 0;
        rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
        gameObject.transform.position = position;
        gameObject.transform.localScale = size;
        //gameObject.GetComponent<BoxCollider>().isTrigger = true;
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    void Update () {
        if (!collides) color = Color.green;
        rb.velocity = Vector3.zero;
        transform.rotation = Quaternion.Euler(Vector3.zero);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = color;
        Gizmos.DrawCube(transform.position, size);
    }

    void OnTriggerEnter(Collider other)
    {
        color = Color.red;
        if (otherRoom == null)
        {
            otherRoom = other.gameObject.GetComponent<Room>();
            neighbours.Add(otherRoom);
        }
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
        if (otherRoom == null)
        {
            otherRoom = other.gameObject.GetComponent<Room>();
        }
        collides = true;
    }

    void OnCollisionEnter(Collision other)
    {
        color = Color.red;
        if(otherRoom == null)
        {
            otherRoom = other.gameObject.GetComponent<Room>();
            neighbours.Add(otherRoom);

        }
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
        if (otherRoom == null)
        {
            otherRoom = other.gameObject.GetComponent<Room>();
        }
        collides = true;
    }

    /// <summary>
    /// Adds a room to this rooms neighbours array.
    /// Conditionally recursive.
    /// </summary>
    /// <param name="newNeighbour">Room that should be added as a neighbour of this Room</param>
    public void AddNeighbour(Room newNeighbour)
    {
        neighbours.Add(newNeighbour);
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
}
