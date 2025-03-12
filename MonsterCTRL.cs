using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
//using UnityEngine.InputSystem.Android;

public class MonsterCTRL : MonoBehaviour
{
    private SkinnedMeshRenderer _renderer;
    [SerializeField] private Texture[] textureSkin;
    public enum State { IDLE, TRACE, ATTACK, DEAD};

    [SerializeField] private Transform TargetTr;
    private Transform MonsterTr;
    private NavMeshAgent agent;

    [SerializeField] private bool isDied = false;

    [SerializeField] State MonsterState = State.IDLE;  // Monster의 기본 상태를 IDLE로 설정.

    [SerializeField] private float attackRange = 1.6f;
    [SerializeField] private float traceRange = 10.0f;

    private Animator animator;

    [SerializeField] private int MonsterHP = 50;

    // Start is called before the first frame update
    void Start()
    {
        _renderer = GetComponentInChildren<SkinnedMeshRenderer>();  // Renderer

        int idx = Random.Range(0, textureSkin.Length);

        _renderer.material.mainTexture = textureSkin[idx];  //  끝

        MonsterTr = GetComponent<Transform>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        TargetTr = GameObject.FindWithTag("Player").transform;

        //StartCoroutine(CheckMonsterState());
        //StartCoroutine(MonsterAction());
    }

    // Update is called once per frame
    void Update()
    {
        if (!isDied)
        {
            float distance = Vector3.Distance(MonsterTr.position, TargetTr.position);

            if (distance <= traceRange)
            {
                MonsterState = State.TRACE;
                agent.destination = TargetTr.position;
                agent.isStopped = false;

                animator.SetBool("isTrace", true);
                
            }

            if (distance <= attackRange)
            {
                MonsterState = State.ATTACK;
                animator.SetBool("isAttack", true);
            }

            if(distance > traceRange) 
            {
                MonsterState = State.IDLE;
                agent.isStopped = true;
                animator.SetBool("isTrace", false);
                int randomInt = Random.Range(0, 2);
                bool randomBool = (randomInt == 1);

                animator.SetBool("Shout", randomBool);

            }
        }

        if(MonsterHP <= 0)
        {
            MonsterState = State.DEAD;
            isDied = true;
            agent.isStopped = true;

            animator.SetBool("isDied", true);
            GetComponent<CapsuleCollider>().enabled = false;  // 충돌 체크도 되면 안 됨.

            Destroy(gameObject, 3.0f);
        }
    }
    /*
    IEnumerator CheckMonsterState()
    {
        while (!isDied)
        {
            yield return new WaitForSeconds(0.5f);

            float distance = Vector3.Distance(MonsterTr.position, TargetTr.position);

            if(distance <= attackRange) 
            {
                MonsterState = State.ATTACK;

                
            }

            else if (distance <= traceRange)
            {
                MonsterState = State.TRACE;
            }

            else
            {
                MonsterState = State.IDLE;
                // 추가해야 될 것: 이 경우에 PATROL 상태 추가. IDLE이랑 PATROL 왔다갔다 하기.
                
            }

            // if (isDied)이면, State를 DEAD로 바꾸고 while문 종료?
        }

    }
    */
    /*
    IEnumerator MonsterAction()
    {
        while (!isDied)
        {
            yield return new WaitForSeconds(0.5f);
            switch (MonsterState)
            {
                case State.IDLE:
                    agent.isStopped = true;
                    animator.SetBool("isTrace", false);
                    break;

                case State.TRACE:
                    agent.isStopped = false;
                    agent.destination = TargetTr.position;

                    animator.SetBool("isTrace", true);
                    Debug.Log("추적 중");
                    break;

                case State.ATTACK:
                    animator.SetBool("isAttack", true);
                    break;

                case State.DEAD:
                    isDied = true;
                    agent.isStopped = true;

                    animator.SetBool("isDied", true);
                    GetComponent<CapsuleCollider>().enabled = false;  // 충돌 체크도 되면 안 됨.

                    Destroy(gameObject, 1.0f);

                    break;

            }

        }
    }
    */
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Bullet"))
        {
            MonsterHP -= 10;
            Debug.Log("Monster's HP: " + MonsterHP);
            animator.SetTrigger("Attacked");
        }
    }


    // 수업 내용 *****
    
    void OnPlayerDie()
    {
        StopAllCoroutines();

        animator.SetTrigger("MonsterWin");
        agent.isStopped = true;
    }

    private void OnEnable()
    {
        PlayerCTRL.OnPlayerDie += OnPlayerDie;
    }

    private void OnDisable()
    {
        PlayerCTRL.OnPlayerDie -= OnPlayerDie;
    }
    
}
