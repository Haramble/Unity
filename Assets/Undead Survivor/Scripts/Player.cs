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
    //ĳ���� ���� ���ϸ��̼� �ҷ�����
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
        // ��� ���̶�� �̵� �Է��� �����մϴ�.
        if (isDashing)
        {
            inputVec = dashDirection;
            return;
        }

        // �̵� �Է� ó��
        inputVec.x = Input.GetAxisRaw("Horizontal");
        inputVec.y = Input.GetAxisRaw("Vertical");

        // ��� �Է� ó��
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            dashDirection = inputVec.normalized;
            StartCoroutine(Dash());
        }
    }

    //�����ֱ� �Լ�
    private void FixedUpdate()
    {
        if (isDashing)
        {
            return;
        }

        // ��� ���� �ƴ� �� ��ġ �̵�
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



    //���

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;

        // �÷��̾��� ���� ��ġ�� �����մϴ�.
        Vector2 startPosition = rigid.position;

        // ��� ����� dashingPower�� �̿��Ͽ� ��� �������� ����մϴ�.
        Vector2 dashDestination = rigid.position + dashDirection * dashingPower;

        float elapsedTime = 0f;

        while (elapsedTime < dashingTime)
        {
            // Lerp �Լ��� �̿��Ͽ� �ε巯�� ��� �������� �����մϴ�.
            rigid.MovePosition(Vector2.Lerp(startPosition, dashDestination, elapsedTime / dashingTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // ��ð� ���� �� �÷��̾ �������� �̵���ŵ�ϴ�.
        rigid.MovePosition(dashDestination);

        // ��ð� ���� �Ŀ� �̵� �Է��� �ٽ� Ȱ��ȭ�մϴ�.
        isDashing = false;

        // ��� �� �÷��̾ �ٽ� ����� �� �ֵ��� ��� ��ٿ��� �����մϴ�.
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }
}


