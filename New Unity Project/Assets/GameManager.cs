using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameStateType
{
    Menu,
    Play,
}

public class GameManager : Singleton<GameManager>
{
    GameStateType gameState;

    public static GameStateType GameState
    {
        get => Instance.gameState;
        set
        {
            if (Instance == null)
                return;

            if (Instance.gameState == value)
                return;

            Debug.Log($"{Instance.gameState} -> {value}, TimeScale : {Time.timeScale}");
            Instance.gameState = value;

            switch (Instance.gameState)
            {
                case GameStateType.Menu:
                    Time.timeScale = 0;
                    break;
                case GameStateType.Play:
                    Time.timeScale = 1;
                    break;
            }
        }
    }

    void Start()
    {
        gameState = GameStateType.Play;
        Cursor.lockState = CursorLockMode.Locked;
    }
    void Update()
    {
        ToggleMousePointer();
    }

    void ToggleMousePointer()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked
                                ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }
}
