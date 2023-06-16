using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Register : MonoBehaviour {

    [SerializeField] TMP_InputField usernameInput; // Input field for username
    [SerializeField] TMP_InputField passwordInput; // Input field for password
    [SerializeField] TMP_InputField repeatPasswordInput; // Input field for repeat password
    [SerializeField] Button submitButton; // Button for registration submission
    [SerializeField] Button backButton; // Button for going back to login
    [SerializeField] GameObject loginWindow; // Login window game object
    public TMP_Text errorBox; // Text box for displaying errors
    private void Start() {
        errorBox.text = ""; // Clear error box text
        passwordInput.contentType = TMP_InputField.ContentType.Password; // Hide password characters
        repeatPasswordInput.contentType = TMP_InputField.ContentType.Password; // Repeat hide password characters

        // Add click event listener for submit button
        submitButton.onClick.AddListener(() => {
            StartCoroutine(Main.Instance.web.Register(usernameInput.text, passwordInput.text, repeatPasswordInput.text)); // Start registration coroutine
        });

        // Add click event listener for back button
        backButton.onClick.AddListener(() => {
            loginWindow.SetActive(true); // Activate login window
            this.gameObject.SetActive(false); // Deactivate current registration window
        });
    }
}
