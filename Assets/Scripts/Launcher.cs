using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Launcher : MonoBehaviourPunCallbacks
{
    // Ŭ���̾�Ʈ ����� ���� ���� ����Ǵ� ��ũ��Ʈ
    // ����ȭ�� ����, ��������, �κ� ������� ���
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

    // LeaveRoom �Ҷ��� �ҷ���
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
        // ù ���ӽ� ������ �÷��̾� ������ �ʱ�ȭ
        if(ScreenManager.SM.GetCurrentScreen() == 0)
            PhotonNetwork.LocalPlayer.NickName = IdInput.text;

    }
}
