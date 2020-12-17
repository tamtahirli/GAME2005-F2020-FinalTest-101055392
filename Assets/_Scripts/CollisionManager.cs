using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CollisionManager : MonoBehaviour
{
    public CubeBehaviour[] cubes;
    public BulletBehaviour[] spheres;

    private static Vector3[] faces;

    public AudioSource bounceSound;

    // Start is called before the first frame update
    void Start()
    {
        cubes = FindObjectsOfType<CubeBehaviour>();

        faces = new Vector3[]
        {
            Vector3.left, Vector3.right,
            Vector3.down, Vector3.up,
            Vector3.back , Vector3.forward
        };
    }

    // Update is called once per frame
    void Update()
    {
        spheres = FindObjectsOfType<BulletBehaviour>();
        cubes = FindObjectsOfType<CubeBehaviour>();

        // check each AABB with every other AABB in the scene
        for (int i = 0; i < cubes.Length; i++)
        {
            for (int j = 0; j < cubes.Length; j++)
            {
                if (i != j)
                {
                    CheckAABBs(cubes[i], cubes[j]);
                }
            }
            foreach(var sphere in spheres)
            {
                if(cubes[i].name != "Player")
                    AABBCheck(sphere, cubes[i]);
            }
        }

    }
    
    // This helper function reflects the bullet when it hits an AABB face
    private static void Reflect(BulletBehaviour s)
    {
        if ((s.collisionNormal == Vector3.forward) || (s.collisionNormal == Vector3.back))
        {
            s.direction = new Vector3(s.direction.x, s.direction.y, -s.direction.z);
        }
        else if ((s.collisionNormal == Vector3.right) || (s.collisionNormal == Vector3.left))
        {
            s.direction = new Vector3(-s.direction.x, s.direction.y, s.direction.z);
        }
        else if ((s.collisionNormal == Vector3.up) || (s.collisionNormal == Vector3.down))
        {
            s.direction = new Vector3(s.direction.x, -s.direction.y, s.direction.z);
        }
    }

    private static float pushSpeed = 0.1f;

    public static void CheckAABBs(CubeBehaviour a, CubeBehaviour b)
    {
        Contact contactB = new Contact(b);

        if ((a.min.x <= b.max.x && a.max.x >= b.min.x) &&
            (a.min.y <= b.max.y && a.max.y >= b.min.y) &&
            (a.min.z <= b.max.z && a.max.z >= b.min.z))
        {
            // determine the distances between the contact extents
            float[] distances = {
                (b.max.x - a.min.x),
                (a.max.x - b.min.x),
                (b.max.y - a.min.y),
                (a.max.y - b.min.y),
                (b.max.z - a.min.z),
                (a.max.z - b.min.z)
            };

            float penetration = float.MaxValue;
            Vector3 face = Vector3.zero;

            // check each face to see if it is the one that connected
            for (int i = 0; i < 6; i++)
            {
                if (distances[i] < penetration)
                {
                    // determine the penetration distance
                    penetration = distances[i];
                    face = faces[i];
                }
            }
            
            // set the contact properties
            contactB.face = face;
            contactB.penetration = penetration;


            // check if contact does not exist
            if (!a.contacts.Contains(contactB))
            {
                // remove any contact that matches the name but not other parameters
                for (int i = a.contacts.Count - 1; i > -1; i--)
                {
                    if (a.contacts[i].cube.name.Equals(contactB.cube.name))
                    {
                        a.contacts.RemoveAt(i);
                    }
                }
                var rigidBody = b.gameObject.GetComponent<RigidBody3D>();

                if (contactB.face == Vector3.down)
                {
                    if(a.gameObject.GetComponent<RigidBody3D>() != null)
                        a.gameObject.GetComponent<RigidBody3D>().Stop();
                    a.isGrounded = true;
                }

                if (a.name == "Player" && rigidBody.bodyType == BodyType.DYNAMIC)
                {
                    Vector3 forward = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z);
                    b.gameObject.transform.position += forward * pushSpeed;
                    rigidBody.velocity = forward * pushSpeed;
                }
                else if(rigidBody.bodyType == BodyType.DYNAMIC && a.name != "Player" && b.name != "Player")
                {
                    if (contactB.face == Vector3.forward)
                    {
                        b.gameObject.transform.position += Vector3.forward;
                    }
                    else if (contactB.face == Vector3.back)
                    {
                        b.gameObject.transform.position -= Vector3.forward;
                    }
                    else if (contactB.face == Vector3.left)
                    {
                        b.gameObject.transform.position += Vector3.left;
                    }
                    else if (contactB.face == Vector3.right)
                    {
                        b.gameObject.transform.position += Vector3.right;
                    }
                }

                // add the new contact
                a.contacts.Add(contactB);
                a.isColliding = true;
                
            }
        }
        else
        {
            if (a.contacts.Exists(x => x.cube.gameObject.name == b.gameObject.name))
            {
                a.contacts.Remove(a.contacts.Find(x => x.cube.gameObject.name.Equals(b.gameObject.name)));
                a.isColliding = false;

                if (a.gameObject.GetComponent<RigidBody3D>().bodyType == BodyType.DYNAMIC)
                {
                    a.gameObject.GetComponent<RigidBody3D>().isFalling = true;
                    a.isGrounded = false;
                }
            }
        }
    }

    struct Manifold
    {
        public Vector3 normal;
        public bool result;
        public float depth;
    }

    private Manifold AABBCheck(BulletBehaviour Cube1, CubeBehaviour Cube2)
    {
        Manifold result = new Manifold();
        GameObject a = Cube1.gameObject;
        GameObject b = Cube2.gameObject;

        MeshFilter aMF = a.GetComponent<MeshFilter>();
        MeshFilter bMF = b.GetComponent<MeshFilter>();

        Bounds aB = aMF.mesh.bounds;
        Bounds bB = bMF.mesh.bounds;

        var min1 = Vector3.Scale(aB.min, a.transform.localScale) + a.transform.position;
        var max1 = Vector3.Scale(aB.max, a.transform.localScale) + a.transform.position;

        var min2 = Vector3.Scale(bB.min, b.transform.localScale) + b.transform.position;
        var max2 = Vector3.Scale(bB.max, b.transform.localScale) + b.transform.position;

        if (!((min1.x <= max2.x && max1.x >= min2.x) &&
            (min1.y <= max2.y && max1.y >= min2.y) &&
            (min1.z <= max2.z && max1.z >= min2.z)))
        {
            result.result = false;
            return result;
        }

        Vector3 pos1 = a.transform.position;
        Vector3 pos2 = b.transform.position;
        Vector3 size1 = a.transform.localScale;
        Vector3 size2 = b.transform.localScale;

        Vector3[] faces = new Vector3[6];
        faces[0] = Vector3.left;
        faces[1] = Vector3.right;
        faces[2] = Vector3.down;
        faces[3] = Vector3.up;
        faces[4] = Vector3.back;
        faces[5] = Vector3.forward;

        float[] dists = new float[6];
        dists[0] = max1.x - min2.x;
        dists[1] = max2.x - min1.x;
        dists[2] = max1.y - min2.y;
        dists[3] = max2.y - min1.y;
        dists[4] = max1.z - min2.z;
        dists[5] = max2.z - min1.z;

        float min = 9999.9f;
        for (int i = 0; i < 6; i++)
        {
            if (dists[i] < min || i == 0)
            {
                result.normal = faces[i];
                result.depth = dists[i];
                min = dists[i];
            }
        }

        result.result = true;

        Cube1.collisionNormal = result.normal;
        Cube1.penetration = result.depth;

        Reflect(Cube1);
        if(!bounceSound.isPlaying)
            bounceSound.Play();

        return result;
    }
}
