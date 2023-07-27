using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUI :MonoBehaviour {

    public GameObject gameOverScreen;
    public GameObject gameWinScreen;
    bool end;

    void Start () {
        Guard.OnPlayerSpotted += ShowLostScreem;
        PlayerMovement.OnPlayerWin += ShowWinScreen;
    }

    // Update is called once per frame
    void Update () {
        if (end) {
            if (Input.GetKeyDown(KeyCode.Space)) {
                SceneManager.LoadScene(0);
            }
        }

    }

    void ShowLostScreem () {
        OnGameOver(gameOverScreen);
    }

    void ShowWinScreen () {
        OnGameOver(gameWinScreen);
    }

    private void OnGameOver (GameObject endScreen) {
        endScreen.SetActive(true);
        end = true;
        Guard.OnPlayerSpotted -= ShowLostScreem;
        PlayerMovement.OnPlayerWin -= ShowWinScreen;
    }
}
