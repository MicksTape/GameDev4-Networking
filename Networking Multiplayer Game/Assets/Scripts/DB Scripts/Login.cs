using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Login : MonoBehaviour {

    [SerializeField] private TMP_InputField usernameInput; // Input field for username
    [SerializeField] private TMP_InputField passwordInput; // Input field for password
    [SerializeField] private Button loginButton; // Button for login
    [SerializeField] private Button registerButton; // Button for registration
    [SerializeField] private GameObject registerWindow; // Registration window
    public TMP_Text errorBox; // Text box for displaying errors

    private void Start() {
        errorBox.text = ""; // Clear error box text
        passwordInput.contentType = TMP_InputField.ContentType.Password; // Set password input field type to hide password characters

        // Add click event listener for login button
        loginButton.onClick.AddListener(() => {
            StartCoroutine(Main.Instance.web.Login(usernameInput.text, passwordInput.text)); // Start login coroutine with provided username and password
        });

        // Add click event listener for register button
        registerButton.onClick.AddListener(() => {
            this.gameObject.SetActive(false); // Deactivate current login window
            registerWindow.SetActive(true); // Activate registration window
        });
    }
}
