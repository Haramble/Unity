using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public Vector2 inputVec;
    public float speed;
    public Scanner scanner;
    public Hand[] hands;
    public RuntimeAnimatorController[] animCon;

    Rigidbody2D rigid;
    SpriteRenderer spriter;
    Animator anim;

    private bool canDash = true;
    private bool isDashing;
    private float dashingPower = 4f;
    private float dashingTime = 0.2f;
    private float dashingCooldown = 1f;
    private Vector2 dashDirection;

    [SerializeField] private TrailRenderer tr;



    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        scanner = GetComponent<Scanner>();
        hands = GetComponentsInChildren<Hand>(true);
    }
    //캐릭에 따른 에니메이션 불러오기
    private void OnEnable()
    {
        speed *= Character.Speed;
        anim.runtimeAnimatorController = animCon[GameManager.instance.playerId];
    }

    void OnMove(InputValue value)
    {
        inputVec = value.Get<Vector2>();

    }

    void Update()
    {
        // 대시 중이라면 이동 입력을 무시합니다.
        if (isDashing)
        {
            inputVec = dashDirection;
            return;
        }

        // 이동 입력 처리
        inputVec.x = Input.GetAxisRaw("Horizontal");
        inputVec.y = Input.GetAxisRaw("Vertical");

        // 대시 입력 처리
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            dashDirection = inputVec.normalized;
            StartCoroutine(Dash());
        }
    }

    //생명주기 함수
    private void FixedUpdate()
    {
        if (isDashing)
        {
            return;
        }

        // 대시 중이 아닐 때 위치 이동
        Vector2 nextVec = inputVec.normalized * speed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec);
    }

    
    private void LateUpdate()
    {
        
        anim.SetFloat("Speed", inputVec.magnitude);

        if (inputVec.x != 0)
        {
            spriter.flipX = inputVec.x < 0;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!GameManager.instance.isLive)
            return;
        GameManager.instance.health -= Time.deltaTime * 10;

        if (GameManager.instance.health<0)
        {
            for(int index = 2; index < transform.childCount; index++)
            {
                transform.GetChild(index).gameObject.SetActive(false);
            }

            anim.SetTrigger("Dead");
            GameManager.instance.GameOver();
        }

    }



    //대시

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;

        // 플레이어의 현재 위치를 저장합니다.
        Vector2 startPosition = rigid.position;

        // 대시 방향과 dashingPower를 이용하여 대시 목적지를 계산합니다.
        Vector2 dashDestination = rigid.position + dashDirection * dashingPower;

        float elapsedTime = 0f;

        while (elapsedTime < dashingTime)
        {
            // Lerp 함수를 이용하여 부드러운 대시 움직임을 생성합니다.
            rigid.MovePosition(Vector2.Lerp(startPosition, dashDestination, elapsedTime / dashingTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 대시가 끝난 후 플레이어를 목적지로 이동시킵니다.
        rigid.MovePosition(dashDestination);

        // 대시가 끝난 후에 이동 입력을 다시 활성화합니다.
        isDashing = false;

        // 대시 후 플레이어가 다시 대시할 수 있도록 대시 쿨다운을 설정합니다.
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }
}


