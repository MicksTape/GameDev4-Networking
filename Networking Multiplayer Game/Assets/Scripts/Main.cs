
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour {
    public static Main Instance;
    public GameManager gameManager;
    public Web web;
    public UserInfo userInfo;
    public Login login;
    public Register register;
    public GameObject userProfile;
    public bool canCheck = false;

    private void Start() {
        Instance = this;
        InitializeComponents();
    }

    private void InitializeComponents() {
        web = GetComponent<Web>();
        userInfo = GetComponent<UserInfo>();
    }
}






