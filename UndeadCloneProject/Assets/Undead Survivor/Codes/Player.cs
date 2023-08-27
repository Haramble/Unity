using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public Vector2 inputVec;
    public float speed;
    public Scanner scanner;
    public Hand[] hands;

    Rigidbody2D rigid;
    SpriteRenderer spriter;
    Animator anim;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        scanner = GetComponent<Scanner>();
        hands = GetComponentsInChildren<Hand>(true);
    }

    // Update is called once per frame
    // 하나의 프레임마다 한번씩 호출되는 생명주기 함수
    void Update()
    {
        if (!GameManager.instance.isLive)
            return;

        //inputVec.x = Input.GetAxis("Horizontal");
        //inputVec.x = Input.GetAxisRaw("Horizontal"); // 움직임이 명확해짐 
        //inputVec.y = Input.GetAxis("Vertical");
        //inputVec.y = Input.GetAxisRaw("Vertical"); // 움직임이 명확해짐 
    }

    void FixedUpdate()
    {
        if (!GameManager.instance.isLive)
            return;

        // 프레임마다 이동 속도가 다를 수 있음, 다른 프레임 환경에서도 이동거리는 같아야 함.
        // fixedDeltaTime : 물리 프레임 하나가 소비한 시간 
        //Vector2 nextVec = inputVec.normalized * speed * Time.fixedDeltaTime;
        Vector2 nextVec = inputVec * speed * Time.fixedDeltaTime;

        // 3. 위치 이동 = MovePosition (MovePosition 은 위치 이동이라 현재 위치도 더해야함)
        rigid.MovePosition(rigid.position + nextVec);

    }

    // 프레임이 종료 되기 전 실행되는 생명주기 함수 (다음 프레임으로 넘어가기 직전에 실행)
    void LateUpdate()
    {
        if (!GameManager.instance.isLive)
            return;

        anim.SetFloat("Speed", inputVec.magnitude);

        if (inputVec.x != 0) // 좌우가 눌러지고 있을 때 
        {
            spriter.flipX = inputVec.x < 0;
        }
    }

    void OnMove(InputValue value)
    {
        inputVec = value.Get<Vector2>();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!GameManager.instance.isLive)
            return;

        GameManager.instance.health -= Time.deltaTime * 10;

        if (GameManager.instance.health < 0)
        {
            for (int index=2; index < transform.childCount; index++)
            {
                transform.GetChild(index).gameObject.SetActive(false);
            }

            anim.SetTrigger("Dead");
            GameManager.instance.GameOver();
        }
    }
}
