using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class Barrel : MonoBehaviour
{
    [SerializeField] private GameObject YellowParticle;

    public GameObject explosionParticle;
    private int Life = 5;

    // Start is called before the first frame update
    void Start()
    {
        explosionParticle.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Bullet"))
        {
            Life--;
            Instantiate(YellowParticle, other.gameObject.transform.position, Quaternion.identity);

            if (Life <= 0)
            {
                explosionParticle.SetActive(true);
                ExpBarrel();  //  폭파 처리하는 함수
            }
            Debug.Log("Collision");
        }
    }

    private void ExpBarrel()
    {
        GameObject explosion = Instantiate(explosionParticle, transform.position, Quaternion.identity);


        // Collider[] colls = Physics.OverlapSphere(transform.position, 20f);  // 폭파 반경이 n미터. n미터 안에 있는 Collider들을 list에 담음.
        /*
        foreach (Collider coll in colls)
        {
            Rigidbody rbody = coll.GetComponent<Rigidbody>();
            if (rbody != null)
            {
                rbody.mass = 1.0f;
                rbody.AddExplosionForce(1000f, transform.position, 50f, 300f);  // 폭파하는 힘, 위치, 반경, upward 방향

            }
        }*/
        
        Destroy(explosion, 10f);


        Destroy(gameObject);  // gameObject 소문자니까 자기 자신. 5는 5초 뒤에 destroy 됨을 의미함.

    }
}
