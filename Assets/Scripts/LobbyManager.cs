using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public Text WelcomeTxt;

    public InputField RoomNameInput;

    public GameObject ConnectScreen;
    public GameObject LobbyScreen;

    public Button[] RoomCells;
    public Button PrevBtn;
    public Button NextBtn;

    private void Update()
    {
        WelcomeTxt.text = "ȯ���մϴ� " + PhotonNetwork.LocalPlayer.NickName +"��.";
        if (Input.GetKeyDown(KeyCode.Return))
        {
            RoomNameInput.Select();
            if (RoomNameInput.text != "")
                CreateRoom();
        }
    }

    public void Disconnect() => PhotonNetwork.Disconnect();
    public override void OnDisconnected(DisconnectCause cause)
    {
        ScreenManager.SM.CloseLobbyScreen();
        ScreenManager.SM.OpenConnectScreen();
    }

    public void CreateRoom() => PhotonNetwork.CreateRoom(RoomNameInput.text == "" ? "Room" + Random.Range(1000, 10000) : RoomNameInput.text, new RoomOptions { MaxPlayers = 8 });
    

    public void JoinRandomRoom() => PhotonNetwork.JoinRandomRoom();
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        CreateRoom();
    }

    #region �� ����Ʈ ����

    List<RoomInfo> myRoomList = new List<RoomInfo>();
    int currentPage = 1, maxPage, pageIndex;

    // �κ� �ִµ��� �� ������Ʈ�� �ҷ����� �Լ�
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        int roomCount = roomList.Count;
        for (int i=0; i<roomCount; i++)
        {
            // ���� ������ �����ϴ� ���ε�
            if (!roomList[i].RemovedFromList)
            {
                // ���� �� Ŭ���̾�Ʈ �� ��Ͽ� ������ �߰����ֱ�
                if (!myRoomList.Contains(roomList[i]))
                {
                    myRoomList.Add(roomList[i]);
                }
                // �̹� �� Ŭ���̾�Ʈ �� ��Ͽ� ������ ���� �� ���� ������Ʈ(�̸��̳� �ο���)
                else
                {
                    myRoomList[myRoomList.IndexOf(roomList[i])] = roomList[i];
                }
            }
        }
        // UI ������Ʈ
        MyRoomListUpdate();
    }
    private void MyRoomListUpdate()
    {
        // �ִ� ������ ��. ���� ���� ��ư ���� ���� �������� ������ ���� ��, �ƴϸ� ��+1
        maxPage = (myRoomList.Count % RoomCells.Length == 0) ? myRoomList.Count / RoomCells.Length : myRoomList.Count / RoomCells.Length + 1;
        // ����, ���� ��ư ��Ȱ��ȭ ù�������� ���� ��Ȱ��, ������ �������� ���� ��Ȱ��
        PrevBtn.interactable = (currentPage <= 1) ? false : true;
        NextBtn.interactable = (currentPage >= maxPage) ? false : true;
        // �������� �´� �� ��ư Ȱ��ȭ
        pageIndex = (currentPage - 1) * RoomCells.Length;
        for(int i = 0; i < RoomCells.Length; i++)
        {
            RoomCells[i].interactable = (myRoomList.Count > pageIndex + i) ? true : false;
            RoomCells[i].transform.GetChild(0).GetComponent<Text>().text = (myRoomList.Count > pageIndex + i) ? myRoomList[pageIndex + i].Name : "";
            RoomCells[i].transform.GetChild(1).GetComponent<Text>().text = (myRoomList.Count > pageIndex + i) ? myRoomList[pageIndex + i].PlayerCount + "/" + myRoomList[pageIndex + i].MaxPlayers : "";
        }
    }
    public void OnClickRoomCell(int n)
    {
        PhotonNetwork.JoinRoom(myRoomList[pageIndex + n].Name);
        MyRoomListUpdate();
    }
    public void OnClickPageChange(int n)
    {
        if (n == 0) currentPage--;
        else currentPage++;
        MyRoomListUpdate();
    }
    #endregion
}
