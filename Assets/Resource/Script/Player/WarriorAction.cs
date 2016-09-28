﻿using UnityEngine;
using System.Collections;

// 전사인 Warrior의 스크립트.
// 1:1근접공격을 하고 Warrior를 터치하면 도끼를 크게 휘두르는 스킬을 쓴다.
public class WarriorAction : PlayerAction
{
    public GameObject AttackEffect = null;                                                // 일반 공격 Effect
    public GameObject TouchAttack_Effect = null;                                    // 터치 공격 Effect
    public GameObject TouchAttack_Pos = null;                                       // 터치 공격시 Effect의 위치
    public GameObject SpecialSkill_Effect = null;                                       // 스페셜 스킬 Effect

    void Awake()
    {

        ani = GetComponent<Animator>();
     //   gameObject.SetActive(false);
    }

    public override void Set_Move()
    {
        ani.SetBool("Move", true);
        ani.SetBool("Attack", false);
    }
    public override void Set_Attack()
    {
        state = STATE.ATTACK;
        ani.SetBool("Move", false);
        ani.SetBool("Attack", true);
        StandPos = transform.position;

        StartCoroutine(C_Attack());
    }
    public override void Set_Idle()
    {
        StopAllCoroutines();

        ani.SetBool("Attack", false);
        ani.SetBool("Move", false);
        transform.position = StandPos;
        transform.rotation = Quaternion.identity;

        state = STATE.IDLE;

        
    }

    public override bool Set_Demage(float AttackDamage, string type)
    {
        Hp -= AttackDamage;
        UIManager.Get_Inctance().Set_Damage(gameObject, AttackDamage, type);

        if (Hp <= 0)
        {
            // 만약 Hp가 0이하면 관리자에게 죽었다고 보고한다.
            state = STATE.DEAD;
            // PlayerManager.Get_Inctance().Check_Dead(gameObject);
            return false;
        }

        return true;
    }

    IEnumerator C_Attack()
    {
        while (true)
        {
            // 상태가 Attack이 아닌경우 코루틴을 종료한다.
            if (state != STATE.ATTACK)
                yield break;

            // Target이 null일경우 MonsterManager에게 새로운 Target을 받아온다.
            if (Target == null)
            {
                MonsterManager.Get_Inctance().Set_ReTarget(this);
            }

            // Target이 있는 쪽을 바라본다.
            Vector3 target = Target.transform.position;
            target.y = transform.position.y;
            Vector3 v = target - transform.position;
            transform.rotation = Quaternion.LookRotation(v);

            // Target과의 거리가 1.5f보다 멀면 Target이 있는 방향으로 이동한다.
            if (Distance(Target.transform.position, transform.position) > 1.5f)
            {
                Set_Move();
                Target_Move(Target.gameObject.transform.position);
                yield return null;
            }
            // 3f보다 가까우면 공격한다.
            else
            {
                ani.SetBool("Attack", true);
            }

            yield return null;

        }
    }
    // Animation에서 실행시키는 Attack 함수.
    public void Monster_Attack()
    {
        Target.Set_Demage(Attack, null);
    }

    public override void Touch_Skill()
    {
        ani.SetTrigger("TouchSkill");
        ani.SetTrigger("Idle");
    }
    public override void Special_Skill()
    {
        // 만약 Player들이 IDLE or MOVE상태면 스킬이 작동되지 않는다.
        if (PlayerManager.Get_Inctance().state.ToString().Equals("IDLE") || PlayerManager.Get_Inctance().state.ToString().Equals("MOVE"))
            return;

        // Target이 null이거나 죽었을시 스킬이 작동되지 않는다.
        if (Target == null || Target.gameObject.activeSelf == false)
            return;

        StartCoroutine(C_Special_Skill());
    }
    IEnumerator C_Special_Skill()
    {
        // Target이 죽었을시 새로운 Target을 받는다.
        if (Target.gameObject.activeSelf == false)
            MonsterManager.Get_Inctance().Set_ReTarget(this);

        // ActionCamera에게 Warrior의 애니메이션을 실행시키게 한다.
        ActionCamera_Action.Get_Inctance().Set_preparation(transform, "Warrior");

        // Warrior을 제외한 모든 Player, Monster의 Active를 false시킨다.
        PlayerManager.Get_Inctance().Players_Active(gameObject, "OFF");
        MonsterManager.Get_Inctance().Monsters_Active(null, "OFF");

        yield return new WaitForSeconds(0.25f);

        ani.SetTrigger("SpecialSkill");

        yield return new WaitForSeconds(0.35f);

        ani.speed = 0f;

        yield return new WaitForSeconds(0.8f);

        ani.speed = 1f;

        // ActionCamera의 Camera를 끈다.
        ActionCamera_Action.Get_Inctance().CameraOff();

        // Warrior을 제외한 모든 Player, Monster의 Active를 true시킨다.
        PlayerManager.Get_Inctance().Players_Active(null, "ON");
        MonsterManager.Get_Inctance().Monsters_Active(null, "ON");

        // Target에게 데미지를 준다.
        Target.Set_Demage(Attack * 10.0f, "Skill");

        // Player들을 Attack상태로 바꾼다. ( Active 변환 때문.)
        PlayerManager.Get_Inctance().Set_Attack();

        yield break;
    }

    // 스페셜 스킬의 Effect를 실행시킨다.
    public void Set_SpecialSkill_Effect()
    {
        GameObject Effect = Instantiate(SpecialSkill_Effect, Vector3.zero, Quaternion.identity) as GameObject;

        Vector3 pos = TouchAttack_Pos.transform.position;
        pos.z += 2f;

        Effect.transform.position = pos;

    }
    // 터치 공격의 Effect를 실행시킨다.
    public void Set_TouchSkill_Effect()
    {
        GameObject Effect = Instantiate(TouchAttack_Effect, Vector3.zero, Quaternion.identity) as GameObject;
        Effect.transform.position = TouchAttack_Pos.transform.position;

    }

    // Target이 있는쪽으로 이동하는 함수.
    public override void Target_Move(Vector3 target)
    {
        target.y = transform.position.y;
        Vector3 v = target - transform.position;

        transform.rotation = Quaternion.LookRotation(v);
        transform.Translate(Vector3.forward * Time.deltaTime * 5f);
    }
    float Distance(Vector3 Target, Vector3 Player)
    {
        return Mathf.Abs(Vector3.Distance(Target, Player));
    }
}