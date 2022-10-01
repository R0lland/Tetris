using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Script to manage HUD elements
public class HUDManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _menu;
    [SerializeField]
    private GameObject _gameOver;
    [SerializeField]
    private GameObject _controls;
    [SerializeField]
    private Text _gameOverScore;
    [SerializeField]
    private Text _scoreTxt;

    public void StartGame() {
        _menu.SetActive(false);
        SetScore(0);
    }

    public void ReturnMenu() {
        _menu.SetActive(true);
        _gameOver.SetActive(false);
    }

    public void SetScore(int score) {
        _scoreTxt.text = score.ToString();
    }

    public void DisableGameOver() {
        _gameOver.SetActive(false);
    }

    public void GameOver(int score) {
        _gameOver.SetActive(true);
        _gameOverScore.text = "Score: " + score.ToString();
    }

    public void OpenControls(bool open) {
        _controls.SetActive(open);
        GameManager.Instance.PauseGame(!open);
    }

}
