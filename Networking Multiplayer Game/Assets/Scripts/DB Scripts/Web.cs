using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Web : MonoBehaviour {
    // URL variables
    private const string loginURL = "https://studenthome.hku.nl/~mick.teunissen/login.php";
    private const string registerURL = "https://studenthome.hku.nl/~mick.teunissen/register.php";
    private const string saveScoreURL = "https://studenthome.hku.nl/~mick.teunissen/save_score.php";
    private const string saveGameURL = "https://studenthome.hku.nl/~mick.teunissen/save_game.php";
    private const string serverLoginURL = "https://studenthome.hku.nl/~mick.teunissen/server_login.php";
    private const string userInfoURL = "https://studenthome.hku.nl/~mick.teunissen/user_info.php";

    public IEnumerator ServerLogin(string serverId, string serverPassword) {
        WWWForm form = new WWWForm();
        form.AddField("serverId", serverId);
        form.AddField("serverPassword", serverPassword);

        using (UnityWebRequest www = UnityWebRequest.Post(serverLoginURL, form)) {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success) {
                Debug.Log(www.error);
            } else {
                Debug.Log(www.downloadHandler.text);

                // If we receive an error message
                if (www.downloadHandler.text.Contains("Wrong credentials") || www.downloadHandler.text.Contains("Server id does not exist")) {
                    Debug.Log("Try Again");
                    Main.Instance.serverLogin.errorBox.text = www.downloadHandler.text;
                } else {
                    // If we logged in correctly
                    //Main.Instance.serverLogin.errorBox.text = www.downloadHandler.text;
                    Main.Instance.gameManager.serverLoginWindow.SetActive(false);
                    Main.Instance.gameManager.loginWindow.SetActive(true);
                    Main.Instance.gameManager.sessionID = www.downloadHandler.text;
                    Main.Instance.gameManager.sessionText.text = "Session ID: " + www.downloadHandler.text;
                    Debug.Log("Session ID: " + www.downloadHandler.text);
                }
            }
        }
    }

    // Login and send to database
    public IEnumerator Login(string username, string password) {
        WWWForm form = new WWWForm();
        form.AddField("loginUser", username);
        form.AddField("loginPass", password);

        using (UnityWebRequest www = UnityWebRequest.Post(loginURL, form)) {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success) {
                Debug.Log(www.error);
            } else {
                Debug.Log(www.downloadHandler.text);

                // If we receive an error message
                if (www.downloadHandler.text.Contains("Wrong credentials") || www.downloadHandler.text.Contains("Username does not exist")) {
                    Debug.Log("Try Again");
                    Main.Instance.login.errorBox.text = www.downloadHandler.text;
                } else {
                    // If we logged in correctly
                    Main.Instance.userInfo.SetCredentials(username, password);
                    Main.Instance.userInfo.SetID(www.downloadHandler.text);

                    Main.Instance.userProfile.SetActive(true);
                    Main.Instance.login.gameObject.SetActive(false);
                    // Fetch user info
                }
            }
        }
    }

    // Register and send to database
    public IEnumerator Register(string username, string password, string repeatPassword) {
        WWWForm form = new WWWForm();
        form.AddField("registerUser", username);
        form.AddField("registerPass", password);
        form.AddField("registerPassRepeat", repeatPassword);

        using (UnityWebRequest www = UnityWebRequest.Post(registerURL, form)) {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success) {
                Debug.Log(www.error);
            } else {
                Debug.Log(www.downloadHandler.text);

                if (www.downloadHandler.text.Contains("Passwords don't match") || www.downloadHandler.text.Contains("has to be")) {
                    Main.Instance.register.errorBox.text = www.downloadHandler.text;
                } else {
                    // If we logged in correctly
                    Main.Instance.register.errorBox.text = www.downloadHandler.text;
                }
            }
        }
    }


    // Save the current score for each player
    public IEnumerator SaveScore(string userID, int userScore, string sessionId) {
        WWWForm form = new WWWForm();
        form.AddField("userID", userID);
        form.AddField("userScore", userScore);
        form.AddField("sessionId", sessionId);

        Debug.Log("My userId is " + userID + " with score " + userScore + " with sessionId " + sessionId);

        using (UnityWebRequest www = UnityWebRequest.Post(saveScoreURL, form)) {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success) {
                Debug.Log(www.error);
            } else {
                Debug.Log(www.downloadHandler.text);
            }
        }
    }

    // Save the game/match scores to the highscores
    public IEnumerator SaveGame(string user_p1, int score_p1, string user_p2, int score_p2, string sessionId) {
        WWWForm form = new WWWForm();
        form.AddField("username_player1", user_p1);
        form.AddField("score_player1", score_p1);
        form.AddField("username_player2", user_p2);
        form.AddField("score_player2", score_p2);
        form.AddField("sessionId", sessionId);

        using (UnityWebRequest www = UnityWebRequest.Post(saveGameURL, form)) {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success) {
                Debug.Log(www.error);
            } else {
                Debug.Log(www.downloadHandler.text);
            }
        }
    }


    // Fetch the user info in a json array
    public IEnumerator GetUserInfo(string userID, System.Action<string> callback) {
        WWWForm form = new WWWForm();
        form.AddField("userID", userID);

        using (UnityWebRequest www = UnityWebRequest.Post(userInfoURL, form)) {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success) {
                Debug.Log(www.error);
            } else {
                Debug.Log(www.downloadHandler.text);
                string jsonArray = www.downloadHandler.text;

                callback(jsonArray);
            }
        }
    }
}
