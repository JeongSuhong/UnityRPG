﻿using UnityEngine;
using System.Collections;

public class CaraterInfo_Action : MonoBehaviour
{

    public int Index;
    public UILabel Label_Name;
    public UISprite CharaterIcon;
    public UILabel Label_Attack;
    public UILabel Label_Defense;
    public GameObject Types;
    public GameObject Stars;
    public string Equipment;
    public GameObject Check;

    void Awake()
    {
        if (GameManager.Get_Inctance().Check_SelectCharater(Index))
        {
            Check.SetActive(true);
        }
        else
        {
            Check.SetActive(false);
        }
    }

    public void Set_CharaterInfo(int index, string name, int attack, int defense, string type, int star)
    {
        Index = index;
        Label_Name.text = name;
        CharaterIcon.spriteName = name;
        Label_Attack.text = attack.ToString();
        Label_Defense.text = defense.ToString();
        Types.transform.FindChild(type).gameObject.SetActive(true);

        for (int i = 0; i < star; i++)
        {
            Stars.transform.GetChild(i).gameObject.SetActive(true);
        }

    }

    public void Click_Charater()
    {
        if (GameManager.Get_Inctance().SelectCharater(Index, !Check.activeSelf) == false)
        {
            return;
        }

        Check.SetActive(!Check.activeSelf);
    }
}