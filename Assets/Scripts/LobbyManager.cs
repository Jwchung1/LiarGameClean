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
        WelcomeTxt.text = "환영합니다 " + PhotonNetwork.LocalPlayer.NickName +"님.";
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

    #region 방 리스트 띄우기

    List<RoomInfo> myRoomList = new List<RoomInfo>();
    int currentPage = 1, maxPage, pageIndex;

    // 로비에 있는동안 매 업데이트에 불러지는 함수
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        int roomCount = roomList.Count;
        for (int i=0; i<roomCount; i++)
        {
            // 현재 서버상에 존재하는 방인데
            if (!roomList[i].RemovedFromList)
            {
                // 아직 내 클라이언트 방 목록에 없으면 추가해주기
                if (!myRoomList.Contains(roomList[i]))
                {
                    myRoomList.Add(roomList[i]);
                }
                // 이미 내 클라이언트 방 목록에 있으면 기존 방 정보 업데이트(이름이나 인원수)
                else
                {
                    myRoomList[myRoomList.IndexOf(roomList[i])] = roomList[i];
                }
            }
        }
        // UI 업데이트
        MyRoomListUpdate();
    }
    private void MyRoomListUpdate()
    {
        // 최대 페이지 수. 방의 수가 버튼 수로 나눠 떨어지면 페이지 수는 몫, 아니면 몫+1
        maxPage = (myRoomList.Count % RoomCells.Length == 0) ? myRoomList.Count / RoomCells.Length : myRoomList.Count / RoomCells.Length + 1;
        // 이전, 다음 버튼 비활성화 첫페이지면 이전 비활성, 마지막 페이지면 다음 비활성
        PrevBtn.interactable = (currentPage <= 1) ? false : true;
        NextBtn.interactable = (currentPage >= maxPage) ? false : true;
        // 페이지에 맞는 방 버튼 활성화
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
