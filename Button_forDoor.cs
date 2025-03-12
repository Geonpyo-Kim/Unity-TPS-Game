using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button_forDoor : MonoBehaviour
{
    private Transform Button;

    [SerializeField] private GameObject Door1;
    [SerializeField] private GameObject Door2;
    [SerializeField] private GameObject Player;

    [SerializeField] private float DoorSpeed;

    private bool isButtonPressed = false;

    // Start is called before the first frame update
    void Start()
    {
        Button = GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        if(isButtonPressed)
        {
            LiftDoors();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject[] Monsters = GameObject.FindGameObjectsWithTag("Monster");

        // Player가 버튼을 누를 수 있는 (문을 열 수 있는) 조건
        if((Monsters.Length == 9 || Monsters.Length == 5) &&  collision.gameObject.CompareTag("Player"))
        {
            isButtonPressed = true;

            Button.position = new Vector3(Button.position.x, - 0.68f, Button.position.z);

        }
    }

    private void LiftDoors()
    {
        Door1.transform.Translate(Vector3.right * DoorSpeed * Time.deltaTime);
        Door2.transform.Translate(- Vector3.right * DoorSpeed * Time.deltaTime);

        float currentZ1 = Door1.transform.position.z;

        if(currentZ1 < -17)
        {
            isButtonPressed = false;
        }

    }
}
