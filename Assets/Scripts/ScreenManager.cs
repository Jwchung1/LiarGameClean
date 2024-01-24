using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenManager : MonoBehaviour
{
    private int currentScreen = 0;

    [Header("Connection Screen")]
    public GameObject ConnectScreen;
    public InputField IdInput;

    [Header("Lobby Screen")]
    public GameObject LobbyScreen;
    public InputField RoomNameInput;

    [Header("Room Screen")]
    public GameObject RoomScreen;
    public InputField ChatInput;

    [Header("Game Screen")]
    public GameObject GameScreen;

    // 싱글톤 패턴으로 모든 클래스에서 호출가능
    private static ScreenManager instance = null;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    public static ScreenManager SM
    { get { return instance; } }
    private void SetCurrentScreen(int n)
    {
        currentScreen = n;
    }
    public int GetCurrentScreen()
    {
        return currentScreen;
    }

    public void SetInitialScreen()
    {
        SetCurrentScreen(0);
        ConnectScreen.SetActive(true);
        LobbyScreen.SetActive(false);
        RoomScreen.SetActive(false);
        GameScreen.SetActive(false);
    }
    public void OpenConnectScreen()
    {
        SetCurrentScreen(0);
        IdInput.text = "";
        ConnectScreen.SetActive(true);
    }
    public void CloseConnectScreen()
    {
        IdInput.text = "";
        ConnectScreen.SetActive(false);
    }
    public void OpenLobbyScreen()
    {
        SetCurrentScreen(1);
        RoomNameInput.text = "";
        LobbyScreen.SetActive(true);
    }
    public void CloseLobbyScreen()
    {
        RoomNameInput.text = "";
        LobbyScreen.SetActive(false);
    }
    public void OpenRoomScreen()
    {
        SetCurrentScreen(2);
        ChatInput.text = "";
        RoomScreen.SetActive(true);
    }
    public void CloseRoomScreen()
    {
        ChatInput.text = "";
        RoomScreen.SetActive(false);
    }
    public void OpenGameScreen()
    {
        SetCurrentScreen(3);
        ChatInput.text = "";
        GameScreen.SetActive(true);
    }
    public void CloseGameScreen()
    {
        ChatInput.text = "";
        GameScreen.SetActive(false);
    }
}
