using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerBehaviour : MonoBehaviour
{
    public Transform bulletSpawn;
    public GameObject bullet;
    public int fireRate; 
    
    public float walkingSpeed = 7.5f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    Vector3 moveDirection = Vector3.zero;

    public BulletManager bulletManager;

    [Header("Movement")] 
    public float speed;
    public bool isGrounded;

    public RigidBody3D body;
    public CubeBehaviour cube;
    public Camera playerCam;

    public AudioSource shootSound;

    int lastFrame = 0;

    bool canMove = false;

    void Start()
    {
        Time.timeScale = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (canMove)
        {
            _Fire();
            _Move();
        }

        if(Input.GetKeyDown(KeyCode.B))
        {
            SceneManager.LoadScene("Start");
        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            if(canMove)
            {
                Time.timeScale = 0;
                canMove = false;
            }
            else
            { 
                Time.timeScale = 1;
                canMove = true;
            }
        }
    }
    //float speedScalar = 0.0001f;
    //float jumpScalar = 0.03f;
    private void _Move()
    {
        Vector3 forward = playerCam.transform.TransformDirection(Vector3.forward);
        Vector3 right = playerCam.transform.TransformDirection(Vector3.right);
        float curSpeedX = canMove ? walkingSpeed * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? walkingSpeed * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (Input.GetKey(KeyCode.Space) && canMove && isGrounded)
        {
            moveDirection.y = jumpSpeed;
            isGrounded = false;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        if (!isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }
        else
            moveDirection.y = 0;
        Vector3 moveDir = (moveDirection * Time.deltaTime);

        transform.position += moveDir;
    }


    private void _Fire()
    {
        if (Input.GetMouseButton(0))
        {
            // delays firing
            if (Time.frameCount - lastFrame >= fireRate)
            {
                var tempBullet = bulletManager.GetBullet(bulletSpawn.position, bulletSpawn.forward);
                tempBullet.transform.SetParent(bulletManager.gameObject.transform);
                lastFrame = Time.frameCount;
                shootSound.Play();
            }
        }
    }

    void FixedUpdate()
    {
        GroundCheck();
    }

    private void GroundCheck()
    {
        isGrounded = cube.isGrounded;
    }

}
