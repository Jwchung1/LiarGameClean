using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Launcher : MonoBehaviourPunCallbacks
{
    // 클라이언트 실행시 가장 먼저 실행되는 스크립트
    // 시작화면 세팅, 서버연결, 로비 입장까지 담당
    public Text ConnectionStatus;

    public InputField IdInput;

    public GameObject ConnectScreen;
    public GameObject LobbyScreen;

    private Dictionary<string, int> test = new Dictionary<string, int>();

    private void Awake()
    {
        Screen.SetResolution(1280, 720, false);
        ScreenManager.SM.SetInitialScreen();
    }
    private void Update()
    {
        ConnectionStatus.text = PhotonNetwork.NetworkClientState.ToString();

        if(Input.GetKeyDown(KeyCode.Return))
        {
            IdInput.Select();
            if (IdInput.text != "")
                Connect();
        }

    }
    public void Connect() => PhotonNetwork.ConnectUsingSettings();

    // LeaveRoom 할때도 불러짐
    public override void OnConnectedToMaster()
    {
        InitPlayer();
        PhotonNetwork.JoinLobby();
    }
    public override void OnJoinedLobby()
    {
        ScreenManager.SM.CloseConnectScreen();
        ScreenManager.SM.OpenLobbyScreen();
    }



    private void InitPlayer()
    {
        // 첫 접속시 접속한 플레이어 정보를 초기화
        if(ScreenManager.SM.GetCurrentScreen() == 0)
            PhotonNetwork.LocalPlayer.NickName = IdInput.text;

    }
}
