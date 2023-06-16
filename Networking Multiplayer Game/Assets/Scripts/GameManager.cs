using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;
using Unity.Collections;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public enum Units {
    NONE = 0,
    INFANTRY = 1,
    ARCHER = 2,
    CAVALRY = 3
}

public class GameManager : MonoBehaviour {

    public static GameManager Instance { set; get; }

    [Header("Main")]
    [SerializeField] TMP_InputField clientIPAdressInput; // Input field for client IP address
    [SerializeField] Button hostButton; // Button for hosting a game
    [SerializeField] Button clientButton; // Button for joining a game as a client
    [SerializeField] Button waitingBackButton; // Button for going back from waiting screen
    [SerializeField] Button lobbyBackButton; // Button for going back from lobby screen
    public GameObject loginWindow; // Login window
    public GameObject registerWindow; // Register window
    public GameObject welcomeWindow; // Welcome window
    public GameObject lobbyWindow; // Lobby window
    public GameObject waitingWindow; // Waiting window
    public GameObject gameWindow; // Game window
    public GameObject infoWindow; // Info window
    public GameObject selectWindow; // Select window
    public GameObject resultsWindow; // Results window
    public GameObject quitWindow; // Quit window game object
    [SerializeField] private TMP_Text playerOneScoreText; // Text for player one's score
    [SerializeField] private TMP_Text playerTwoScoreText; // Text for player two's score
    public Server server; // Server object for networking
    public Client client; // Client object for networking

    [Header("Select Window")]
    [SerializeField] private Image timerFill; // Fill image for the timer countdown
    [SerializeField] private TMP_Text timerText; // Text for the timer countdown
    [SerializeField] private float selectUnitsDuration = 10f; // Duration of the countdown in seconds for selecting units
    [SerializeField] private float revealingUnitsDuration = 5f; // Duration of the countdown in seconds for revealing units
    [SerializeField] private float statusMessageDelay = 3f; // Delay duration in seconds for showing status messages
    [SerializeField] private float newRoundDuration = 5f; // Duration of the countdown in seconds for starting a new round
    private float statusTextPosition; // Position of the status text
    [SerializeField] private Button confirmButton; // Button for confirming unit selection
    [SerializeField] private Button infoButton; // Button for showing win conditions info
    [SerializeField] private Button infoExitButton; // Button for exiting the info window
    [SerializeField] private Button leaveButton; // Button for leaving the game
    [SerializeField] private Button exitButton; // Button for exiting the game
    public Button infantryButton; // Button for selecting infantry unit
    public Button archerButton; // Button for selecting archer unit
    public Button cavalryButton; // Button for selecting cavalry unit

    // Game Variables
    private Units selectedUnit; // Current selected unit
    private bool playerOneHasSelected = false; // Flag indicating if player one has selected a unit
    private bool playerTwoHasSelected = false; // Flag indicating if player two has selected a unit
    private int playerOneUnit, playerTwoUnit; // Units selected by player one and player two
    [SerializeField] private TMP_Text statusText; // Text for displaying status messages
    [SerializeField] private GameObject playerOneBanner, playerTwoBanner; // Banners for player one and player two
    [SerializeField] private GameObject playerOneWinBanner, playerTwoWinBanner; // Win banners for player one and player two
    [SerializeField] private Sprite infantrySprite, archerSprite, cavalrySprite, unknownSprite; // Sprites representing different unit types
    [SerializeField] private GameObject playerOneText, playerTwoText; // Text elements for player one and player two

    private Coroutine countdownCoroutine; // Coroutine for handling countdown
    private Coroutine battleCoroutine; // Coroutine for handling battle sequence

    private string winMessage = "You win!"; // Win message displayed to the player
    private string lostMessage = "Opponent won!"; // Loss message displayed to the player
    private string drawMessage = "It's a draw!"; // Draw message displayed to the player

    public int playerOneScore = 0; // Score of player one
    public int playerTwoScore = 0; // Score of player two

    public string username_player1 = ""; // Username of player one
    public string username_player2 = ""; // Username of player two

    // Networking variables
    private int playerCount = -1;
    private int currentTeam = -1;

    private void Awake() {
        Instance = this;
        
        RegisterEvents();
    }


    private void Start() {

        selectedUnit = Units.NONE;
        //clientIPAdressInput.text = "127.0.0.1"; // FOR TESTING PURPOSES

        // Enable and disable screens on start
        Main.Instance.gameManager.loginWindow.SetActive(true);
        Main.Instance.gameManager.registerWindow.SetActive(false);
        Main.Instance.gameManager.welcomeWindow.SetActive(false);
        Main.Instance.gameManager.waitingWindow.SetActive(false);
        Main.Instance.gameManager.selectWindow.SetActive(true);
        Main.Instance.gameManager.infoWindow.SetActive(false);
        Main.Instance.gameManager.resultsWindow.SetActive(false);
        Main.Instance.gameManager.lobbyWindow.SetActive(false);
        Main.Instance.gameManager.quitWindow.SetActive(false);
        statusText.gameObject.SetActive(false);
        statusTextPosition = statusText.rectTransform.anchoredPosition.y;

        // When pressed on back button in lobby, return to welcome screen
        lobbyBackButton.onClick.AddListener(() => {
            Main.Instance.gameManager.welcomeWindow.SetActive(true);
            Main.Instance.gameManager.lobbyWindow.SetActive(false);
            Main.Instance.canCheck = false;
        });

        // When pressed on the confirm button
        confirmButton.onClick.AddListener(() => {
            ConfirmChoice();
        });

        // Connect to host, and connect to ourselves (the client)
        hostButton.onClick.AddListener(() => {
            server.Init(1551); //1551
            client.Init("127.0.0.1", 1551);
            waitingWindow.SetActive(true);
            lobbyWindow.SetActive(false);
            //StartCoroutine(Main.instance.web.Login(usernameInput.text, passwordInput.text));
        });

        clientButton.onClick.AddListener(() => {
            client.Init(clientIPAdressInput.text, 1551);        
            //gameWindow.SetActive(true);
        });

        waitingBackButton.onClick.AddListener(() => {
            // When exiting host
            Invoke("ShutdownRelay", 1.0f);
            lobbyWindow.SetActive(true);
            waitingWindow.SetActive(false);
        });

        // When pressed on the leave button
        leaveButton.onClick.AddListener(() => {
            OnQuitButton();
        });

        // When pressed on the leave button
        exitButton.onClick.AddListener(() => {
            OnExitButton();
        });
    }

    public void UnitPlayerOneInfantry() {
        UnitPlayerOne(Units.INFANTRY);
    }

    public void UnitPlayerOneArcher() {
        UnitPlayerOne(Units.ARCHER);
    }

    public void UnitPlayerOneCavalry() {
        UnitPlayerOne(Units.CAVALRY);
    }

    // The selected unit of player 1
    public void UnitPlayerOne(Units units) {

        DeselectAllUnits();

        switch (units) {
            case Units.INFANTRY: // If selected unit is infantry
                selectedUnit = Units.INFANTRY; // Set unit to infantry
                infantryButton.gameObject.GetComponent<Outline>().enabled = true; // Enable black outline
                infantryButton.GetComponentInChildren<TMP_Text>().fontStyle = FontStyles.Bold; // Make text bold
                playerOneBanner.GetComponent<Image>().sprite = infantrySprite;
                break;
            case Units.ARCHER: // If selected unit is archer
                selectedUnit = Units.ARCHER; // Set unit to archer
                archerButton.gameObject.GetComponent<Outline>().enabled = true; // Enable black outline
                archerButton.GetComponentInChildren<TMP_Text>().fontStyle = FontStyles.Bold; // Make text bold
                playerOneBanner.GetComponent<Image>().sprite = archerSprite;
                break;
            case Units.CAVALRY: // If selected unit is cavalery
                selectedUnit = Units.CAVALRY; // Set unit to cavalery
                cavalryButton.gameObject.GetComponent<Outline>().enabled = true; // Enable black outline
                cavalryButton.GetComponentInChildren<TMP_Text>().fontStyle = FontStyles.Bold; // Make text bold
                playerOneBanner.GetComponent<Image>().sprite = cavalrySprite;
                break;
        }

        Debug.Log("Selected Unit: " + (int)selectedUnit);
    }

    private void DeselectAllUnits() {
        infantryButton.gameObject.GetComponent<Outline>().enabled = false;
        infantryButton.GetComponentInChildren<TMP_Text>().fontStyle = FontStyles.Normal;
        archerButton.gameObject.GetComponent<Outline>().enabled = false;
        archerButton.GetComponentInChildren<TMP_Text>().fontStyle = FontStyles.Normal;
        cavalryButton.gameObject.GetComponent<Outline>().enabled = false;
        cavalryButton.GetComponentInChildren<TMP_Text>().fontStyle = FontStyles.Normal;
        playerOneBanner.GetComponent<Image>().sprite = unknownSprite;
    }


    // Countdown timer match
    private IEnumerator StartCountdown() {
        float counter = selectUnitsDuration;

        while (counter > 0f) {
            // Update the timer text
            timerText.text = counter.ToString("F0");

            // Update the timer fill amount
            timerFill.fillAmount = counter / selectUnitsDuration;

            // Wait 1 second
            yield return new WaitForSeconds(1f);
            counter--;
        }

        // Reset the counter to its original value

        // Countdown finished
        timerText.text = "0";
        timerFill.fillAmount = 0f;
        Debug.Log("Countdown finished!");
        
        // If player has not selected a unit in time, choose random unit
        if (selectedUnit == Units.NONE) {
            selectedUnit = (Units)Random.Range(1, 3);
        }

        UnitPlayerOne(selectedUnit);
        ConfirmChoice();
    }


    private IEnumerator Battle(NetMessage msg) {
        NetMakeMove mm = msg as NetMakeMove;
        float counter = revealingUnitsDuration;

        statusText.gameObject.SetActive(true);

        while (counter > 0f) {
            // Update the statusbar text
            statusText.text = "The battle will start in " + counter.ToString("F0") + "...";

            // Wait 1 second
            yield return new WaitForSeconds(1f);
            counter--;
        }

        // Change player2 unit UI
        if (mm.teamId == currentTeam) {
            int playerUnit = (mm.teamId == 1) ? playerOneUnit : playerTwoUnit;

            if (playerUnit == 1) // Infantry
                playerTwoBanner.GetComponent<Image>().sprite = infantrySprite;
            else if (playerUnit == 2) // Archer
                playerTwoBanner.GetComponent<Image>().sprite = archerSprite;
            else if (playerUnit == 3) // Cavalry
                playerTwoBanner.GetComponent<Image>().sprite = cavalrySprite;
        } else {
            int unitNW = mm.unitNW;

            if (unitNW == 1) // Infantry
                playerTwoBanner.GetComponent<Image>().sprite = infantrySprite;
            else if (unitNW == 2) // Archer
                playerTwoBanner.GetComponent<Image>().sprite = archerSprite;
            else if (unitNW == 3) // Cavalry
                playerTwoBanner.GetComponent<Image>().sprite = cavalrySprite;
        }

        // Determine the winner
        DetermineWinner(msg);
    }


    // When someone quits/leaves the game
    public void OnQuitButton() {
        // Net implementation
        NetQuitGame qm = new NetQuitGame();
        qm.teamId = currentTeam;
        qm.hasQuit = currentTeam;
        Client.Instance.SendToServer(qm);

        // Save score
        if (qm.teamId == 0) {
            StartCoroutine(Main.Instance.web.SaveScore(Main.Instance.userInfo.UserId, Main.Instance.userInfo.Score + playerOneScore));
        } else {
            StartCoroutine(Main.Instance.web.SaveScore(Main.Instance.userInfo.UserId, Main.Instance.userInfo.Score + playerTwoScore));
        }

        Main.Instance.gameManager.lobbyWindow.SetActive(true);
        Main.Instance.gameManager.gameWindow.SetActive(false);
        Main.Instance.gameManager.selectWindow.SetActive(true);
        Main.Instance.gameManager.infoWindow.SetActive(false);
        Main.Instance.gameManager.resultsWindow.SetActive(false);

        // Save game data to highscores
        StartCoroutine(Main.Instance.web.SaveGame(username_player1, playerOneScore, username_player2, playerTwoScore));

        Invoke("ShutdownRelay", 1.0f);

        // Reset some values
        //playerCount = -1;
        //currentTeam = -1;
    }

    public void OnInfoButton() {
        Main.Instance.gameManager.infoWindow.SetActive(true);

    }
    public void OnInfoExitButton() {
        Main.Instance.gameManager.infoWindow.SetActive(false);

    }


    public void OnExitButton() {
        NetQuitGame qm = new NetQuitGame();
        qm.teamId = currentTeam;
        qm.hasQuit = currentTeam;
        Client.Instance.SendToServer(qm);

        Main.Instance.gameManager.lobbyWindow.SetActive(true);
        Main.Instance.gameManager.gameWindow.SetActive(false);
        Main.Instance.gameManager.selectWindow.SetActive(true);
        Main.Instance.gameManager.infoWindow.SetActive(false);
        Main.Instance.gameManager.resultsWindow.SetActive(false);
        Main.Instance.gameManager.quitWindow.SetActive(false);

        Invoke("ShutdownRelay", 1.0f);

        // Reset some values
        playerCount = -1;
        currentTeam = -1;
    }

    public void DetermineWinner(NetMessage msg) {

        NetMakeMove mm = msg as NetMakeMove;

        // Draw
        if (playerOneUnit == playerTwoUnit) {
            statusText.text = drawMessage;
        } else if (playerOneUnit == 1 && playerTwoUnit == 3) { // Infantry vs Cavalry = Infantry wins
            VictoryPlayerOne(msg);
        } else if (playerTwoUnit == 1 && playerOneUnit == 3) { // Infantry vs Cavalry = Infantry wins
            VictoryPlayerTwo(msg);
        } else if (playerOneUnit == 2 && playerTwoUnit == 1) { // Archer vs Infantry = Archer wins
            VictoryPlayerOne(msg);
        } else if (playerTwoUnit == 2 && playerOneUnit == 1) { // Archer vs Infantry = Archer wins
            VictoryPlayerTwo(msg);
        } else if (playerOneUnit == 3 && playerTwoUnit == 2) { // Cavalry vs Archer = Cavalry wins
            VictoryPlayerOne(msg);
        } else if (playerTwoUnit == 3 && playerOneUnit == 2) { // Cavalry vs Archer = Cavalry wins
            VictoryPlayerTwo(msg);
        } else {
            statusText.text = drawMessage;
        }

        // Start new game in x seconds
        StartCoroutine(StartNewRound());
    }

    private void VictoryPlayerOne(NetMessage msg) {
        NetMakeMove mm = msg as NetMakeMove;

        if (mm.teamId == currentTeam) {
            if (mm.teamId == 1) {
                statusText.text = lostMessage;
                statusText.color = new Color32(217, 67, 52, 255);
                playerOneBanner.GetComponent<Image>().color = new Color32(255, 255, 255, 50);
                playerTwoText.GetComponent<TMP_Text>().color = new Color32(217, 67, 52, 255);
                playerTwoWinBanner.SetActive(true);
                playerOneScore += 1;
                playerTwoScoreText.text = playerOneScore.ToString();
            } else {
                statusText.text = winMessage;
                statusText.color = new Color32(76, 176, 80, 255);
                playerTwoBanner.GetComponent<Image>().color = new Color32(255, 255, 255, 50);
                playerOneText.GetComponent<TMP_Text>().color = new Color32(76, 176, 80, 255);
                playerOneWinBanner.SetActive(true);
                playerOneScore += 1;
                playerOneScoreText.text = playerOneScore.ToString();
            }
        } else {
            if (mm.teamId == 1) {
                statusText.text = winMessage;
                statusText.color = new Color32(76, 176, 80, 255);
                playerTwoBanner.GetComponent<Image>().color = new Color32(255, 255, 255, 50);
                playerOneText.GetComponent<TMP_Text>().color = new Color32(76, 176, 80, 255);
                playerOneWinBanner.SetActive(true);
                playerOneScore += 1;
                playerOneScoreText.text = playerOneScore.ToString();
            } else {
                statusText.text = lostMessage;
                statusText.color = new Color32(217, 67, 52, 255);
                playerOneBanner.GetComponent<Image>().color = new Color32(255, 255, 255, 50);
                playerTwoText.GetComponent<TMP_Text>().color = new Color32(217, 67, 52, 255);
                playerTwoWinBanner.SetActive(true);
                playerOneScore += 1;
                playerTwoScoreText.text = playerOneScore.ToString();
            }
        }
    }

    private void VictoryPlayerTwo(NetMessage msg) {
        NetMakeMove mm = msg as NetMakeMove;

        if (mm.teamId == currentTeam) {
            if (mm.teamId == 0) {
                statusText.text = lostMessage;
                statusText.color = new Color32(217, 67, 52, 255);
                playerOneBanner.GetComponent<Image>().color = new Color32(255, 255, 255, 50);
                playerTwoText.GetComponent<TMP_Text>().color = new Color32(217, 67, 52, 255);
                playerTwoWinBanner.SetActive(true);
                playerTwoScore += 1;
                playerTwoScoreText.text = playerTwoScore.ToString();
            } else {
                statusText.text = winMessage;
                statusText.color = new Color32(76, 176, 80, 255);
                playerTwoBanner.GetComponent<Image>().color = new Color32(255, 255, 255, 50);
                playerOneText.GetComponent<TMP_Text>().color = new Color32(76, 176, 80, 255);
                playerOneWinBanner.SetActive(true);
                playerTwoScore += 1;
                playerOneScoreText.text = playerTwoScore.ToString();
            }
        } else {
            if (mm.teamId == 0) {
                statusText.text = winMessage;
                statusText.color = new Color32(76, 176, 80, 255);
                playerTwoBanner.GetComponent<Image>().color = new Color32(255, 255, 255, 50);
                playerOneText.GetComponent<TMP_Text>().color = new Color32(76, 176, 80, 255);
                playerOneWinBanner.SetActive(true);
                playerTwoScore += 1;
                playerOneScoreText.text = playerTwoScore.ToString();
            } else {
                statusText.text = lostMessage;
                statusText.color = new Color32(217, 67, 52, 255);
                playerOneBanner.GetComponent<Image>().color = new Color32(255, 255, 255, 50);
                playerTwoText.GetComponent<TMP_Text>().color = new Color32(217, 67, 52, 255);
                playerTwoWinBanner.SetActive(true);
                playerTwoScore += 1;
                playerTwoScoreText.text = playerTwoScore.ToString();
            }
        }
    }

    private IEnumerator StartNewRound() {

        // Show the win/loss/draw message for X seconds, then start countdown to next round
        yield return new WaitForSeconds(statusMessageDelay);

        Main.Instance.gameManager.resultsWindow.SetActive(false);
        statusText.gameObject.SetActive(true);
        statusText.rectTransform.anchoredPosition = new Vector2(statusText.rectTransform.anchoredPosition.x, 0);
        statusText.fontSize = 80;

        // Reset statusbar color to white
        statusText.color = new Color32(255, 255, 255, 255);

        float counter = newRoundDuration;

        while (counter > 0f) {
            // Update the timer text
            statusText.text = "Next round in " + counter.ToString("F0") + "...";

            // Wait 1 second
            yield return new WaitForSeconds(1f);
            counter--;
        }

        // Set player choices back to none
        selectedUnit = Units.NONE;

        // Net implementation
        NetMakeMove mm = new NetMakeMove();
        mm.unitNW = (int)selectedUnit;
        mm.teamId = currentTeam;
        Client.Instance.SendToServer(mm);

        // Save score
        if (mm.teamId == 0) {
            StartCoroutine(Main.Instance.web.SaveScore(Main.Instance.userInfo.UserId, Main.Instance.userInfo.Score + playerOneScore));
        } else {
            StartCoroutine(Main.Instance.web.SaveScore(Main.Instance.userInfo.UserId, Main.Instance.userInfo.Score + playerTwoScore));
        }

        // Reset outlines
        DeselectAllUnits();

        // Change to game window
        Main.Instance.gameManager.gameWindow.SetActive(true);
        Main.Instance.gameManager.selectWindow.SetActive(true);
        Main.Instance.gameManager.infoWindow.SetActive(false);
        Main.Instance.gameManager.resultsWindow.SetActive(false);

        // Reset all UI changes
        statusText.text = "";
        statusText.color = new Color32(255, 255, 255, 255);
        statusText.rectTransform.anchoredPosition = new Vector2(statusText.rectTransform.anchoredPosition.x, statusTextPosition);
        statusText.fontSize = 32;
        statusText.gameObject.SetActive(false);
        playerOneWinBanner.SetActive(false);
        playerTwoWinBanner.SetActive(false);
        playerOneBanner.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
        playerTwoBanner.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
        playerOneBanner.GetComponent<Image>().sprite = unknownSprite;
        playerTwoBanner.GetComponent<Image>().sprite = unknownSprite;
        playerOneText.GetComponent<TMP_Text>().color = new Color32(255, 255, 255, 255);
        playerTwoText.GetComponent<TMP_Text>().color = new Color32(255, 255, 255, 255);

        countdownCoroutine = StartCoroutine(StartCountdown());
    }


    public void ConfirmChoice() {
        if (selectedUnit != Units.NONE) {

            // Stop the countdown coroutine if it is running
            if (countdownCoroutine != null) {
                StopCoroutine(countdownCoroutine);
            }

            // Stop the battle coroutine if it is running
            if (battleCoroutine != null) {
                StopCoroutine(battleCoroutine);
            }

            // Net implementation
            NetMakeMove mm = new NetMakeMove();
            mm.unitNW = (int)selectedUnit; // Convert enum to integer
            mm.playerName = Main.Instance.userInfo.UserName.ToString();
            mm.teamId = currentTeam;
            Client.Instance.SendToServer(mm);
            Debug.Log("Confirmed unit: " + (int)selectedUnit); // Convert selectedUnit to int

            selectWindow.SetActive(false);
            resultsWindow.SetActive(true);
        } else {
            Debug.LogError("No unit selected.");
            return;
        }
        
    }


    #region Manage Events
    private void RegisterEvents() {
        // Server events
        NetUtility.S_WELCOME += OnWelcomeServer;
        NetUtility.S_MAKE_MOVE += OnMakeMoveServer;
        NetUtility.S_QUIT_GAME += OnQuitGameServer;

        // Client events
        NetUtility.C_WELCOME += OnWelcomeClient;
        NetUtility.C_START_GAME += OnStartGameClient;
        NetUtility.C_MAKE_MOVE += OnMakeMoveClient;
        NetUtility.C_QUIT_GAME += OnQuitGameClient;
    }

    private void UnregisterEvents() {
        // Server events
        NetUtility.S_WELCOME -= OnWelcomeServer;
        NetUtility.S_MAKE_MOVE -= OnMakeMoveServer;
        NetUtility.S_QUIT_GAME -= OnQuitGameServer;

        // Client events
        NetUtility.C_WELCOME -= OnWelcomeClient;
        NetUtility.C_START_GAME -= OnStartGameClient;
        NetUtility.C_MAKE_MOVE -= OnMakeMoveClient;
        NetUtility.C_QUIT_GAME -= OnQuitGameClient;
    }

    private void OnStartGameClient(NetMessage msg) {
        // Show the start canvas (game window)
        Main.Instance.gameManager.gameWindow.SetActive(true);
        Main.Instance.gameManager.selectWindow.SetActive(true);
        Main.Instance.gameManager.resultsWindow.SetActive(false);
        Main.Instance.gameManager.waitingWindow.SetActive(false);
        Main.Instance.gameManager.lobbyWindow.SetActive(false);

        // Start the countdown timer
        countdownCoroutine = StartCoroutine(StartCountdown());
    }

    // Server
    private void OnWelcomeServer(NetMessage msg, NetworkConnection cnn) {
        // Client has connected, assign a team and return the message back to him
        NetWelcome nw = msg as NetWelcome;

        // Assign a team
        nw.AssignedTeam = ++playerCount;

        // Return back to the client
        Server.Instance.SendToClient(cnn, nw);

        // Check if server has 2 players, then start game
        if (playerCount == 1) {
            Server.Instance.Broadcast(new NetStartGame());
        }
    }

    // Client
    private void OnWelcomeClient(NetMessage msg) {
        // Receive the connection message
        NetWelcome nw = msg as NetWelcome;

        // Assign the team
        currentTeam = nw.AssignedTeam;

        Debug.Log($"My assigned team is {nw.AssignedTeam}");

        // Reset some stats
        playerOneScore = 0;
        playerTwoScore = 0;
        playerOneScoreText.text = "0";
        playerTwoScoreText.text = "0";
        statusText.text = "";
        statusText.color = new Color32(255, 255, 255, 255);

        DeselectAllUnits();
    }

    private void OnMakeMoveServer(NetMessage msg, NetworkConnection cnn) {
        // Receive the message, broadcast it back
        NetMakeMove mm = msg as NetMakeMove;

        // Here do validation checks

        // Receive and broadcast it back to other client
        //Server.Instance.Broadcast(msg);
        Server.Instance.Broadcast(mm);
    }

    private void OnMakeMoveClient(NetMessage msg) {
        NetMakeMove mm = msg as NetMakeMove;

        Debug.Log($"MM : {mm.teamId} : {mm.unitNW}");

        // Check if player one has made a move
        if (mm.teamId == 0 && mm.unitNW != 0) {
            playerOneHasSelected = true;
            username_player1 = mm.playerName.ToString();
            playerOneUnit = mm.unitNW;
            if (mm.teamId == currentTeam) {
                statusText.gameObject.SetActive(true);
                statusText.text = "Waiting for opponent to pick a unit...";
                Debug.Log("Waiting for opponent to pick a unit...");
            }
        }

        // Check if player two has made a move
        if (mm.teamId == 1 && mm.unitNW != 0) {
            playerTwoHasSelected = true;
            username_player2 = mm.playerName.ToString();
            playerTwoUnit = mm.unitNW;
            if (mm.teamId == currentTeam) {
                statusText.gameObject.SetActive(true);
                statusText.text = "Waiting for opponent to pick a unit...";
                Debug.Log("Waiting for opponent to pick a unit...");
            }
        }

        if (playerOneHasSelected == true && playerTwoHasSelected == true) {
            Debug.Log("Both players have chosen a unit!");
            statusText.gameObject.SetActive(true);
            statusText.text = "";
            playerOneHasSelected = false;
            playerTwoHasSelected = false;
            Debug.Log("P1: " + username_player1 + " P2: " + username_player2);
            battleCoroutine = StartCoroutine(Battle(msg));
        }

    }

    private void OnQuitGameServer(NetMessage msg, NetworkConnection cnn) {
        Server.Instance.Broadcast(msg);
    }

    private void OnQuitGameClient(NetMessage msg) {
        // Receive the connection message
        NetQuitGame qm = msg as NetQuitGame;

        Debug.Log("Player has left the game");
        Debug.Log(qm.hasQuit);

        // Activate UI (other player has left)
        if (qm.hasQuit != currentTeam) {

            Main.Instance.gameManager.gameWindow.SetActive(true);
            Main.Instance.gameManager.selectWindow.SetActive(false);
            Main.Instance.gameManager.resultsWindow.SetActive(false);
            Main.Instance.gameManager.quitWindow.SetActive(true);
        }

        // Send scores to database, and reset scores to 0
        playerOneScore = 0;
        playerTwoScore = 0;
        playerOneScoreText.text = "0";
        playerTwoScoreText.text = "0";
        playerCount = -1;
        currentTeam = -1;
    }

    private void ShutdownRelay() {
        Client.Instance.Shutdown();
        Server.Instance.Shutdown();
    }
    #endregion
}
