using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public RectTransform healthBarFill;

    public enum Menu { Main, Pause, End, HUD, Loadout };

    [SerializeField]
    List<MenuPair> menuList;
    Dictionary<Menu, GameObject> menus;
    Menu activeMenu;


    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
        CreateMenuDict();


        GoToMenu(Menu.HUD);
    }

    void Update()
    {
        
    }

    void CreateMenuDict()
    {
        menus = new Dictionary<Menu, GameObject> ();
        foreach(MenuPair pair in menuList)
        {
            menus.Add(pair.menuName, pair.menuObject);
            pair.menuObject.SetActive(false);
        }
    }


    public void SetHealthBarFill(float healthPercentage)
    {
        Vector3 newScale = healthBarFill.localScale;
        newScale.x = Mathf.Clamp01(healthPercentage);
        healthBarFill.localScale = newScale;
    }

    public void GoToMenu(Menu menu)
    {
        menus[activeMenu].SetActive(false);
        menus[menu].SetActive(true);
        activeMenu = menu;
    }

    [Serializable]
    public struct MenuPair
    {
        public Menu menuName;
        public GameObject menuObject;
    }


}
