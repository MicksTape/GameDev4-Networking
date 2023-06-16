using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserInfo : MonoBehaviour {

    [SerializeField]
    private string userId;
    public string UserId {
        get { return userId; }
        set { userId = value; }
    }

    [SerializeField]
    private string userName;
    public string UserName {
        get { return userName; }
        set { userName = value; }
    }

    [SerializeField]
    private string userPassword;
    public string UserPassword {
        get { return userPassword; }
        set { userPassword = value; }
    }

    [SerializeField]
    private int score;
    public int Score {
        get { return score; }
        set { score = value; }
    }

    // Set the player's username and password
    public void SetCredentials(string username, string password) {
        UserName = username; // Set the username
        UserPassword = password; // Set the password
    }

    // Set the player's user ID
    public void SetID(string id) {
        UserId = id; // Set the user ID
    }

    // Set the player's score
    public void SetScore(int myScore) {
        Score = myScore; // Set the score
    }

}
