using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

[Serializable]
public class jsonDataClass {
    public string id;
    public string username;
    public string password;
    public int score;
}

public class GetUserInfo : MonoBehaviour {
    // UI variables
    [SerializeField] private TMP_Text usernameText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private Button accountSettingsButton;
    [SerializeField] private Button highscoresButton;
    [SerializeField] private Button playButton;
    [SerializeField] private Button logoutButton;
    [SerializeField] private GameObject loginWindow;
    [SerializeField] private GameObject lobbyWindow;

    private string userId;
    private string userName;
    private string userPassword;
    private int currentScore;
    private string sessionId;

    private Action<string> createUserInfoCallback;

    private void Start() {
        // Retrieve user information
        userId = Main.Instance.userInfo.UserId;
        userName = Main.Instance.userInfo.UserName;
        userPassword = Main.Instance.userInfo.UserPassword;
        currentScore = Main.Instance.userInfo.Score;
        sessionId = Main.Instance.gameManager.sessionID;


        // Callback for creating user information
        createUserInfoCallback = jsonArray => {
            StartCoroutine(CreateUserInfo(jsonArray));
        };

        // Fetch user information every 5 seconds
        StartCoroutine(FetchUserInfo(5f));

        // Open URL to edit profile
        accountSettingsButton.onClick.AddListener(() => {
            string url = "https://studenthome.hku.nl/~mick.teunissen/edit_profile.php?id=" + userId + "&username=" + userName + "&password=" + Main.Instance.userInfo.UserPassword + "&session_id=" + sessionId;
            Application.OpenURL(url);
        });

        // Open URL to view highscores
        highscoresButton.onClick.AddListener(() => {
            string url = "https://studenthome.hku.nl/~mick.teunissen/highscores.php?&session_id=" + sessionId;
            Application.OpenURL(url);
        });

        // Logout button action
        logoutButton.onClick.AddListener(() => {
            // Disable checking for user info
            Main.Instance.canCheck = false;

            // Show login window and hide current window
            loginWindow.SetActive(true);
            gameObject.SetActive(false);

            // Stop fetching user info
            StopCoroutine(FetchUserInfo(5f));

            // Reset user info
            userId = "";
            userName = "";
            userPassword = "";
            currentScore = 0;

            // Update user info in Main.Instance
            Main.Instance.userInfo.SetID(userId);
            Main.Instance.userInfo.SetCredentials(userName, userPassword);
            Main.Instance.userInfo.SetScore(currentScore);
        });

        // Play button action
        playButton.onClick.AddListener(() => {
            // Show lobby window and hide current window
            lobbyWindow.SetActive(true);
            gameObject.SetActive(false);
        });
    }

    private void Update() {
        if (gameObject.activeInHierarchy && !Main.Instance.canCheck) {
            // Enable checking for user info
            Main.Instance.canCheck = true;

            // Retrieve user information
            userId = Main.Instance.userInfo.UserId;
            userName = Main.Instance.userInfo.UserName;
            userPassword = Main.Instance.userInfo.UserPassword;
            currentScore = Main.Instance.userInfo.Score;

            // Update callback for creating user information
            createUserInfoCallback = jsonArray => {
                StartCoroutine(CreateUserInfo(jsonArray));
            };

            // Fetch user information
            StartCoroutine(Main.Instance.web.GetUserInfo(userId, createUserInfoCallback));

            // Fetch user information every 5 seconds
            StartCoroutine(FetchUserInfo(5f));
        }
    }

    private IEnumerator FetchUserInfo(float time) {
        while (true) {
            // Fetch user information
            StartCoroutine(Main.Instance.web.GetUserInfo(userId, createUserInfoCallback));
            yield return new WaitForSeconds(time);
        }
    }

    private IEnumerator CreateUserInfo(string jsonArray) {
        // Deserialize JSON data
        jsonDataClass jsnData = JsonUtility.FromJson<jsonDataClass>(jsonArray);

        // Update user credentials and UI text
        Main.Instance.userInfo.SetCredentials(jsnData.username, jsnData.password);
        usernameText.text = "Hello, " + jsnData.username + "!";
        Main.Instance.userInfo.SetScore(jsnData.score);
        scoreText.text = "Battles won: " + jsnData.score.ToString();

        yield return null;
    }
}
