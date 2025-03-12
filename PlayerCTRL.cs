using Cinemachine;
using System.Collections;
using System.Collections.Generic;
//using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
//using UnityEngine.InputSystem.XR;

public class PlayerCTRL : MonoBehaviour
{
    private float dirX = 0;
    private float dirZ = 0;

    private float moveSpeed = 6f;
    private float walkSpeed = 6f;
    private float runSpeed = 11.5f;

    private Rigidbody myRigid;

    [SerializeField] private int HP_Player;

    [SerializeField] private GameObject Treasure_open1;
    [SerializeField] private GameObject Treasure_open2;
    [SerializeField] private GameObject Treasure_open3;

    private int NumOfTreasure = 0;

    private bool isGameEnded = false;  // Player가 죽거나, 보물 3개 다 먹었을 경우 true로 전환하기.

    private Animator playerAnimator;

    // 카메라 회전 + aim
    private const float _threshold = 0.01f;
    [SerializeField] private bool LockCameraPosition = false;
    private bool IsCurrentDeviceMouse = false;

    [SerializeField] private GameObject CinemachineCameraTarget;
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;

    [SerializeField] private float TopClamp = 70.0f;
    [SerializeField] private float BottomClamp = -30.0f;
    [SerializeField] private float CameraAngleOverride = 0.0f;

    private float Sensitivity = 1.0f;
    private float NormalSensitivity = 2.0f;
    private float AimSensitivity = 1.0f;

    // Camera 회전, AimCamera 등

    private Vector3 mouseWorldPosition = Vector3.zero;
    [SerializeField] private CinemachineVirtualCamera aimVirtualCamera;

    [SerializeField] private LayerMask aimColliderLayerMask = new LayerMask();

    [SerializeField] private Transform debugTransform;


    // Bullet 
    [SerializeField] private Transform BulletPrefab;
    [SerializeField] private Transform SpawnBulletPosition;

    private float lastShootTime; // 마지막으로 총알을 발사한 시간
    public float shootCooldown = 0.5f; // 발사 간격 (초)

    // Bullet - MuzzleFlash
    [SerializeField] private ParticleSystem MuzzleFlash;

    /*
    // Audio
    [SerializeField] private AudioClip fireSound;
    private AudioSource audioSource;
    
    [Range(0, 1)] public float FireAudioVolume = 0.5f;
    */


    // 마지막 수업 내용 ***************************************************

    public delegate void PlayerDieHandler();

    public static event PlayerDieHandler OnPlayerDie;


    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;


        myRigid = GetComponent<Rigidbody>();

        Treasure_open1.SetActive(false);
        Treasure_open2.SetActive(false);
        Treasure_open3.SetActive(false);

        playerAnimator = GetComponent<Animator>();

        _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

        //audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        Mouse_Camera();

        WalkAnimator();
        RunAnimator();
        CameraRotation();
        SpawnBullet();

    }

    private void Move()
    {
        dirX = Input.GetAxis("Horizontal");
        dirZ = Input.GetAxis("Vertical");

        /*if(dirX == 0 && dirZ == 0)
        {
            float dirY = Random.Range(-0.05f, 0.05f) * 0.3f;
            Vector3 moveY = transform.up * dirY;
            Vector3 velocity_Y = moveY;
            myRigid.MovePosition(transform.position + velocity_Y);
            return;
        }*/

        Vector3 moveX = transform.right * dirX;
        Vector3 moveZ = transform.forward * dirZ;

        Vector3 velocity = (moveX + moveZ).normalized * moveSpeed;

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) 
        {
            if(dirZ > 0 && Input.GetMouseButton(1) == false) 
            {
                moveSpeed = runSpeed; 
            }
            else { moveSpeed = walkSpeed; }
        }
        else { moveSpeed = walkSpeed; }

        myRigid.MovePosition(transform.position + velocity * Time.deltaTime);

    }

    private void Mouse_Camera()
    {
        mouseWorldPosition = Vector3.zero;

        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenter);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderLayerMask))
        {
            debugTransform.position = raycastHit.point;
            mouseWorldPosition = raycastHit.point;
        }

        if (Input.GetMouseButton(1))
        {
            aimVirtualCamera.gameObject.SetActive(true);

        }
        else
        {
            aimVirtualCamera.gameObject.SetActive(false);
        }

        Vector3 worldAimTarget = mouseWorldPosition;
        worldAimTarget.y = transform.position.y;
        Vector3 aimDirection = (worldAimTarget - transform.position).normalized;

        transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20f);
    }

    private void WalkAnimator()
    {
        if (dirX > 0.0f)
        {
            playerAnimator.SetBool("isWalkRight", true);
            playerAnimator.SetBool("isWalkLeft", false);
        }
        if (dirX < 0.0f)
        {
            playerAnimator.SetBool("isWalkLeft", true);
            playerAnimator.SetBool("isWalkRight", false);
        }
        if (dirX == 0.0f)
        {
            playerAnimator.SetBool("isWalkRight", false);
            playerAnimator.SetBool("isWalkLeft", false);
        }


        if (dirZ > 0.0f)
        {
            playerAnimator.SetBool("isWalkFront", true);
            playerAnimator.SetBool("isWalkBack", false);
        }
        if (dirZ < 0.0f)
        {
            playerAnimator.SetBool("isWalkBack", true);
            playerAnimator.SetBool("isWalkFront", false);
        }
        if (dirZ == 0.0f)
        {
            playerAnimator.SetBool("isWalkFront", false);
            playerAnimator.SetBool("isWalkBack", false);
        }
    }

    private void RunAnimator()
    {
        if (Input.GetMouseButton(1) == false && moveSpeed == runSpeed)
        {
            playerAnimator.SetBool("isRun", true);
        }
        else { playerAnimator.SetBool("isRun", false); }

    }

    private void CameraRotation()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        if(Input.GetMouseButtonDown(1)) 
        {
            Sensitivity = AimSensitivity;
        }
        else
        {
            Sensitivity = NormalSensitivity;
        }

        // if there is an input and camera position is not fixed
        if (Mathf.Sqrt(Mathf.Pow(mouseX, 2) + Mathf.Pow(mouseY, 2)) >= _threshold && !LockCameraPosition)
        {
            if (mouseX != 0 || mouseY != 0) { IsCurrentDeviceMouse = true; }
            //Don't multiply mouse input by Time.deltaTime;
            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            _cinemachineTargetYaw += mouseX * deltaTimeMultiplier * Sensitivity;
            _cinemachineTargetPitch += mouseY * deltaTimeMultiplier * Sensitivity;
        }

        // clamp our rotations so our values are limited 360 degrees
        _cinemachineTargetYaw = Mathf.Clamp(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = Mathf.Clamp(_cinemachineTargetPitch, BottomClamp, TopClamp);

        // Cinemachine will follow this target
        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(-_cinemachineTargetPitch - CameraAngleOverride,
            _cinemachineTargetYaw, 0.0f);
    }

    private void SpawnBullet()
    {
        if (Input.GetMouseButtonDown(0) && moveSpeed != runSpeed && CanShoot() == true)  
            // 달리고 있을 때는 총을 쏠 수 없다. + 쿨타임이 지나야 총을 쏠 수 있다.
        {
            Vector3 aimDir = (mouseWorldPosition - SpawnBulletPosition.position).normalized;
            Instantiate(BulletPrefab, SpawnBulletPosition.position, Quaternion.LookRotation(aimDir));

            ShootAnimator();

            //FireSound();

            lastShootTime = Time.time;

            MuzzleFlash.Play();

        }
        else
        {
            playerAnimator.SetBool("isShooting", false);
        }
    }
    private void ShootAnimator()  // 총 쏘는 animation
    {
        playerAnimator.SetTrigger("Shooting");
        playerAnimator.SetBool("isShooting", true);
    }

    private bool CanShoot()
    {
        // 마지막으로 발사한 후 일정 시간이 지났는지 확인
        return Time.time >= lastShootTime + shootCooldown;

    }

    /*
    private void FireSound()
    {
        Debug.Log("Fire");
        AudioSource.PlayClipAtPoint(fireSound, transform.position, FireAudioVolume);
    }
    */


    private void OnTriggerEnter(Collider other)
    {
        if (HP_Player > 0 && other.CompareTag("Punch")) { HP_Player -= 10; }

        if (HP_Player < 0) { HP_Player = 0; }

        Debug.Log("Player's Current HP: " + HP_Player);

        if (HP_Player == 0) 
        { 
            Debug.Log("You died...");

            playerAnimator.SetTrigger("PlayerDied");

            // Destroy(gameObject, 1.5f);

            // 수업 내용 ***************************************************
            GameManager.instance.isGameOver = true;

            OnPlayerDie();
            
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (Treasure_open1.activeSelf == false && collision.collider.CompareTag("Treasure_closed1"))
        {
            NumOfTreasure += 1;
            collision.gameObject.SetActive(false);
            Treasure_open1.SetActive(true);

            Debug.Log("Obtained Treasures: " + NumOfTreasure);
            Debug.Log("Remaining Treasures: " + (3 - NumOfTreasure));
        }

        if (Treasure_open2.activeSelf == false && collision.collider.CompareTag("Treasure_closed2"))
        {
            NumOfTreasure += 1;
            collision.gameObject.SetActive(false);
            Treasure_open2.SetActive(true);

            Debug.Log("Obtained Treasures: " + NumOfTreasure);
            Debug.Log("Remaining Treasures: " + (3 - NumOfTreasure));
        }

        if (Treasure_open3.activeSelf == false && collision.collider.CompareTag("Treasure_closed3"))
        {
            NumOfTreasure += 1;
            collision.gameObject.SetActive(false);
            Treasure_open3.SetActive(true);

            Debug.Log("Obtained Treasures: " + NumOfTreasure);
            Debug.Log("Remaining Treasures: " + (3 - NumOfTreasure));
        }

        if (NumOfTreasure == 3 && isGameEnded == false)
        {
            Debug.Log("Player Win!!!");
            isGameEnded = true;
        }
    }

}
