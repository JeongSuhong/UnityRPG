﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonsterManager : MonoBehaviour {
    

    // Scene에 출연한 Monster를 담고있는 List
    public List<GameObject> Monsters = new List<GameObject>();

    private static MonsterManager instance = null;

    void Awake()
    {
        instance = this;

    }
    public static MonsterManager Get_Inctance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType(typeof(MonsterManager)) as MonsterManager;
        }

        if (null == instance)
        {
            GameObject obj = new GameObject("MonsterManager ");
            instance = obj.AddComponent(typeof(MonsterManager)) as MonsterManager;

            Debug.Log("Fail to get Manager Instance");
        }
        return instance;
    }

    // Player가 Monster출연범위 안에 들어오면 실행된다. PlayerManager에서 호출한다.
    // Monster들을 List에 넣고 Attack를 시작하게한다.
    public void Ready_Attack(GameObject Monster_Group)
    {
        Monsters.Clear();
        Monster_Group.GetComponent<BoxCollider>().enabled = false;

        //Monster_Group의 자식 == 출연할 몬스터
        for(int i = 0; i < Monster_Group.transform.childCount; i++)
        {
            GameObject Monster = Monster_Group.transform.GetChild(i).gameObject;

            // 만약 자식오브젝트가 Monster가 아니라면 continue한다.
            if (Monster.CompareTag("Monster") == false)
            {
                continue;
            }

            MonsterAction monster_action = Monster.GetComponent<MonsterAction>();
            PlayerManager.Get_Inctance().Set_ReTarget(monster_action);
            Monster.SetActive(true);
            Monsters.Add(Monster);

            //Monster들이 Attack을 시작하도록 함수를 호출한다.
            monster_action.StartSet_Attack();
        }
    }

    public void Ready_BossAttack(GameObject Monster_Group)
    {
        Monsters.Clear();
        Monster_Group.GetComponent<BoxCollider>().enabled = false;

        //Monster_Group의 자식 == 출연할 몬스터
        for (int i = 0; i < Monster_Group.transform.childCount; i++)
        {
            GameObject Monster = Monster_Group.transform.GetChild(i).gameObject;

            // 만약 자식오브젝트가 Monster가 아니라면 continue한다.
            if (Monster.CompareTag("Monster") == false)
            {
                continue;
            }

            MonsterAction monster_action = Monster.GetComponent<MonsterAction>();
            PlayerManager.Get_Inctance().Set_ReTarget(monster_action);

            Monster.SetActive(true);
            Monsters.Add(Monster);
        }
        GameStateManager.Get_Inctance().Set_Boss();
        StartCoroutine(Check_PlayerWin());
    }

    IEnumerator Check_PlayerWin()
    {
        while (true)
        {
            int DeadCheck = 0;
            for (int i = 0; i < Monsters.Count; i++)
            {
                if (Monsters[i].GetComponent<MonsterAction>().state.ToString().Contains("DEAD") == true)
                {
                    DeadCheck++;

                    if (DeadCheck == Monsters.Count)
                    {
                        GameStateManager.Get_Inctance().Set_Win();
                        yield break;
                    }
                }
            }

            yield return null;
        }
    }
    

    public void Set_ReAttack()
    {
        for(int i = 0; i < Monsters.Count; i++)
        {
            Monsters[i].GetComponent<MonsterAction>().StartSet_Attack();
        }
    }
    public void Set_ReAttack(GameObject Except_Obj)
    {
        for (int i = 0; i < Monsters.Count; i++)
        {
            if(Except_Obj == Monsters[i]) { continue; }
            Monsters[i].GetComponent<MonsterAction>().StartSet_Attack();
        }
    }


    public void Set_Idle()
    {
        for (int i = 0; i < Monsters.Count; i++)
        {
            if (Monsters[i].GetComponent<MonsterAction>().Check_Dead())
            {
                continue;
            }


            Monsters[i].GetComponent<MonsterAction>().Set_AniIdle();
        }
    }

    // Monster가 죽으면 실행되는 함수.
    // Monster가 다 죽으면 GM의 Set_Next()를 호출한다.
    public bool Check_Dead(GameObject monster)
    {
        PlayerManager.Get_Inctance().Check_Target();

        int DeadCount = 0;

        //Monster의 active가 false == Dead . 그러므로 DeadCount를 늘린다.
        for (int i = 0; i < Monsters.Count; i++)
        {
            if (Check_MonsterState(Monsters[i], "DEAD"))
            {
                DeadCount++;
            }
        }

        //Monster의 죽은 숫자가 List에 있는 몬스터숫자랑 동일하면 모든 Monster가 죽은거니 GM의 Set_Next()를 호출한다.
        if (DeadCount == Monsters.Count)
        {
            GameStateManager.Get_Inctance().Set_Next();
            return true;
        }

        return false;

    }
    
    //살아있는 Monster를 Player의 Tager으로 설정해주는 함수.
    public void Set_ReTarget(PlayerAction Player)
    {
        for (int i = 0; i < Monsters.Count; i++)
        {
            if (Check_MonsterState(Monsters[i], "DEAD") == false) 
            {
                Player.Target = Monsters[i].GetComponent<MonsterAction>();
            }
        }
    }

    public GameObject Get_AliveMonster()
    {
        for (int i = 0; i < Monsters.Count; i++)
        {
            if (Check_MonsterState(Monsters[i], "DEAD") == false)
            {
                return Monsters[i];
            }
        }

        return null;
    }

    public void Set_AllMonsterDamage(float Value)
    {
        for (int i = 0; i < Monsters.Count; i++)
        {
            if (Check_MonsterState(Monsters[i], "DEAD") == false)
            {
                Monsters[i].GetComponent<MonsterAction>().Set_Demage(Value, "Skill");
            }
        }
    }

    public void Set_ParticularTarget(GameObject Player, float Time)
    {
        for (int i = 0; i < Monsters.Count; i++)
        {
            if (Monsters[i] == null)
            {
                continue;
            }

            if (Player == null || Player.activeSelf == false)
            {
                return;
            }

            Monsters[i].GetComponent<MonsterAction>().Target = Player.GetComponent<PlayerAction>();
            Monsters[i].GetComponent<MonsterAction>().Set_StateProvocation();
        }

        Invoke("Check_Target", Time);
    }

    // Monster의 Target이 null이거나 active가 false면 PlayerManager에서 ReTarget함수를 실행시킨다. 
    public void Check_Target()
    {
        for (int i = 0; i < Monsters.Count; i++)
        {
            if (Monsters[i] == null)
            {
                continue;
            }

            MonsterAction Monster = Monsters[i].GetComponent<MonsterAction>();

            if (Monster.Check_StateProvocation())
            {
                PlayerManager.Get_Inctance().Set_ReTarget(Monster);
                Monster.StartSet_Attack();
               
                return;
            }
            else
            {   
                if (Monster.Target == null || Monster.Target.gameObject.activeSelf == false)
                {
                    PlayerManager.Get_Inctance().Set_ReTarget(Monster);
                    return;
                }
            }
        }
    }

    // 모든 Monster들의 Active를 조종하는 함수. type이 ON이면 true, OFF면 false한다.
    // 만약 target이 null이 아니라면 target만 type에 따라 active를 조종한다.
    public void Monsters_Active(GameObject target, string type)
    {
        // 모든 Monster들의 Active를 비활성화 한다. 
        if (type.Equals("OFF"))
        {
            for (int i = 0; i < Monsters.Count; i++)
            {

                // 만약 Target과 Monster가 같으면 넘긴다.
                if (Monsters[i] == target) { continue; }

                Monsters[i].GetComponent<MonsterAction>().Set_AniIdle();
                Monsters[i].SetActive(false);

            }
        }
        // 모든 Monster들의 Active를 활성화 한다.
        else if (type.Equals("ON"))
        {
            for (int i = 0; i < Monsters.Count; i++)
            {
                if (Monsters[i].GetComponent<MonsterAction>().state.ToString().Equals("DEAD")) continue;

                Monsters[i].SetActive(true);
            }
        }

        return;
    }
    public bool Check_MonsterState(GameObject Monster, string State)
    {
        MonsterAction monster = Monster.GetComponent<MonsterAction>();
        if (monster.state.ToString().Equals(State))
        {
            return true;
        }

        return false;
    }
}
