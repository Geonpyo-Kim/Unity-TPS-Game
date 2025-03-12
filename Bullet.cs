using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Rigidbody bulletRigidbody;
    private float Speed;
    //[SerializeField] private Transform PurpleParticle;
    //[SerializeField] private Transform RedParticle;  // Monster 맞았을 때

    private void Awake()
    {
        bulletRigidbody = GetComponent<Rigidbody>();
    }
    // Start is called before the first frame update
    void Start()
    {
        Speed = 100f;
        bulletRigidbody.velocity = transform.forward * Speed;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Monster"))  // Monster를 제외한 다른 물체를 맞추었을 경우
        {
            //Instantiate(PurpleParticle, transform.position, Quaternion.identity);
        }

        else  //  Monster를 맞추었을 경우 -> 빨간색 피처럼.
        {
            //Instantiate(RedParticle, transform.position, Quaternion.identity);
        }
        if (other.gameObject.CompareTag("Wall"))
        {
            bulletRigidbody.velocity = -transform.forward * Speed;
        }
        
        Destroy(gameObject, 0.1f);
    }

}
