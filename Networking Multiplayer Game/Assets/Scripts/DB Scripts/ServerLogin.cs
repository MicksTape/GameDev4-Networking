using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ServerLogin : MonoBehaviour {

    [SerializeField] private TMP_InputField serverIdInput; // Input field for username
    [SerializeField] private TMP_InputField serverPasswordInput; // Input field for password
    [SerializeField] private Button loginButton; // Button for login
    public TMP_Text errorBox; // Text box for displaying errors

    private void Start() {
        // SERVER LOGIN
        serverIdInput.text = "2023";
        serverPasswordInput.text = "hku2023";

        errorBox.text = ""; // Clear error box text
        serverPasswordInput.contentType = TMP_InputField.ContentType.Password; // Set password input field type to hide password characters

        // Add click event listener for login button
        loginButton.onClick.AddListener(() => {
            StartCoroutine(Main.Instance.web.ServerLogin(serverIdInput.text, serverPasswordInput.text)); // Start login coroutine with provided username and password
        });
    }
}
