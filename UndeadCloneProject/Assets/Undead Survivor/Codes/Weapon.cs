using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public int id;
    public int prefabId;
    public float damage;
    public int count;
    public float speed;

    // 하드코딩 
    public bool weaponFlag = true;
    public int rotateCount = 0;

    float timer;
    Player player;

    private void Awake()
    {
        player = GameManager.instance.player;

        // 하드코딩 
        this.weaponFlag = true;
        this.rotateCount = 0;
    }

    private void Update()
    {
        if (!GameManager.instance.isLive)
            return;

        switch (id)
        {
            case 0:
                //transform.Rotate(Vector3.back * speed * Time.deltaTime);
                timer += Time.deltaTime;

                if (weaponFlag == true && timer > 0.3f)
                {
                    timer = 0f;
                    for (int i=0; i< transform.childCount; i++)
                    {
                        transform.GetChild(i).GetComponent<Bullet>().BulletDelete();
                    }
                    weaponFlag = false;
                } else if (weaponFlag == false && timer > 0.3f)
                {
                    timer = 0f;
                    Batch();
                    weaponFlag = true;
                }

                break;
            case 1:
                timer += Time.deltaTime;

                if (timer > speed)
                {
                    timer = 0f;
                    Fire();
                    //HwaSan();
                }
                break;
            default:
                break;
        }

        if (Input.GetButtonDown("Jump"))
        {
            LevelUp(3, 1);
        }
    }

    public void LevelUp(float damage, int count)
    {
        this.damage += damage;
        this.count += count;

        if (id == 0)
        {
            Batch();
        }

        player.BroadcastMessage("ApplyGear", SendMessageOptions.DontRequireReceiver);
    }

    public void Init(ItemData data)
    {
        // Basic Set
        name = "Weapon " + data.itemId;
        transform.parent = player.transform; // 무기는 플레이어의 자식 오브젝트로 생성해야됨.
        transform.localPosition = Vector3.zero; // 위치가 벗어날 수도 있어서 위치 맞춰주기

        // Property Set
        id = data.itemId;
        damage = data.baseDamage;
        count = data.baseCount;

        for (int index=0; index < GameManager.instance.pool.prefabs.Length; index++)
        {
            if (data.projectile == GameManager.instance.pool.prefabs[index])
            {
                prefabId = index;
                break;
            }
        }

        switch (id)
        {
            case 0:
                speed = 150; // 근접무기 회전 속도
                Batch();
                break;
            default:
                //speed = 0.4f; // 원거리무기 연사 속도
                speed = 0.4f; // 화산 공격 딜레이 
                break;
        }

        // Hand Set
        Hand hand = player.hands[(int)data.itemType];
        hand.spriter.sprite = data.hand;
        hand.gameObject.SetActive(true);

        player.BroadcastMessage("ApplyGear", SendMessageOptions.DontRequireReceiver);
    }

    void Batch()
    {
        Vector3[] batchRot = {
            Vector3.forward * 360 * 7 / 8,
            Vector3.forward * 360 * 6 / 8,
            Vector3.forward * 360 * 5 / 8,

            Vector3.forward * 360 * 3 / 8,
            Vector3.forward * 360 * 2 / 8,
            Vector3.forward * 360 * 1 / 8
        };

        for (int i = 0; i < count; i++)
        {
            Transform bullet;

            if (i < transform.childCount)
            {
                bullet = transform.GetChild(i);
                transform.GetChild(i).GetComponent<Bullet>().BulletActive();
            }
            else
            {
                bullet = GameManager.instance.pool.Get(prefabId).transform;
                bullet.parent = transform; // 무기 prefab 의 부모를 Weapon 으로 변경
            }

            bullet.localPosition = Vector3.zero;
            bullet.localRotation = Quaternion.identity;

            //Vector3 rotVec = Vector3.forward * 360 * i / count;
            Vector3 rotVec = batchRot[(rotateCount + i) % 6];
            Debug.Log((rotateCount + i) % 6);
            bullet.Rotate(rotVec);
            bullet.Translate(bullet.up * 1.5f, Space.World);

            bullet.GetComponent<Bullet>().Init(damage, -100, Vector3.zero); // -100 is Infinity Per.

        }
        rotateCount = (rotateCount + 1) % 6;
    }

    void HwaSan()
    {
        Vector3[] hwaSanDir = {
            new Vector3(1,1,0),
            new Vector3(1,0,0),
            new Vector3(1,-1,0),
            new Vector3(-1,1,0),
            new Vector3(-1,0,0),
            new Vector3(-1,-1,0)
        };
        Vector3[] dirs = GetShuffleList<Vector3>(hwaSanDir);

        for (int i=0; i<=count; i++)
        {
            Vector3 dir = dirs[i].normalized;
            Transform bullet = GameManager.instance.pool.Get(prefabId).transform;
            bullet.position = transform.position;
            bullet.rotation = Quaternion.FromToRotation(Vector3.up, dir);

            bullet.GetComponent<Bullet>().Init(damage, count, dir);
        }
    }

    void Fire()
    {
        if (player.scanner.nearestTarget == null)
            return;

        Vector3 targetPos = player.scanner.nearestTarget.position;
        Vector3 dir = targetPos - transform.position;
        dir = dir.normalized;

        Transform bullet = GameManager.instance.pool.Get(prefabId).transform;
        bullet.position = transform.position;
        bullet.rotation = Quaternion.FromToRotation(Vector3.up, dir);

        bullet.GetComponent<Bullet>().Init(damage, count, dir);
    }

    public T[] GetShuffleList<T>(T[] array)
    {
        int random1, random2;
        T temp;

        for (int i = 0; i < array.Length; ++i)
        {
            random1 = Random.Range(0, array.Length);
            random2 = Random.Range(0, array.Length);

            temp = array[random1];
            array[random1] = array[random2];
            array[random2] = temp;
        }

        return array;
    }
}
