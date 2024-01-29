using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public PhotonView PV;

    [Header("Chat")]
    public InputField ChatInput;
    public GameObject ChatPrefab;
    public GameObject ChatWindow;

    [Header("Member")]
    public GameObject MemberPrefab;
    public GameObject MemberWindow;

    [Header("Button")]
    public Button StartGameBtn;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return) && ScreenManager.SM.GetCurrentScreen() == 2)
        {
            if(ChatInput.text != "")
                Send();
            ChatInput.Select();
        }
    }

    public void LeaveRoom() => PhotonNetwork.LeaveRoom();
    public override void OnLeftRoom()
    {
        ClearChat();
        ScreenManager.SM.CloseGameScreen();
        ScreenManager.SM.CloseRoomScreen();
        ScreenManager.SM.OpenLobbyScreen();
    }
    public override void OnJoinedRoom()
    {
        ScreenManager.SM.CloseLobbyScreen();
        ScreenManager.SM.OpenRoomScreen();
        InitRoom();
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        InitRoom();
        ChatRPC("<color=yellow>" + newPlayer.NickName + "¥‘¿Ã ¿‘¿Â«œºÃΩ¿¥œ¥Ÿ</color>");
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        InitRoom();
        ChatRPC("<color=yellow>" + otherPlayer.NickName + "¥‘¿Ã ≈¿Â«œºÃΩ¿¥œ¥Ÿ</color>");
    }
    private void InitRoom()
    {
        ShowMemberList();
        SetMasterAuthority();
    }
    private void ShowMemberList()
    {
        // ∏‚πˆ ∏ÆΩ∫∆Æ √ ±‚»≠
        Transform[] childList = MemberWindow.GetComponentsInChildren<Transform>();
        if (childList != null)
        {
            for (int i = 1; i < childList.Length; i++)
            {
                if (childList[i] != transform)
                    Destroy(childList[i].gameObject);
            }
        }
        // ∏‚πˆ ∂ÁøÏ±‚
        int numberOfPlayers = PhotonNetwork.PlayerList.Length;
        for (int i = 0; i < numberOfPlayers; i++)
        {
            GameObject member = Instantiate(MemberPrefab, MemberWindow.transform);
            member.GetComponentInChildren<Text>().text = PhotonNetwork.PlayerList[i].NickName;
        }
    }
    private void ClearChat()
    {
        Transform[] childList = ChatWindow.GetComponentsInChildren<Transform>();
        if (childList != null)
        {
            for (int i = 1; i < childList.Length; i++)
            {
                if (childList[i] != transform)
                    Destroy(childList[i].gameObject);
            }
        }
    }
    private void SetMasterAuthority()
    {
        if(PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            StartGameBtn.interactable = true;
        }
        else
        {
            StartGameBtn.interactable= false;
        }
    }

    public void OnClickStartGame()
    {
        PV.RPC("ChangeToGameScreenRPC", RpcTarget.All);
    }
    [PunRPC]
    private void ChangeToGameScreenRPC()
    {
        ScreenManager.SM.CloseRoomScreen();
        ScreenManager.SM.OpenGameScreen();
    }
    #region √§∆√
    public void Send()
    {
        string msg = PhotonNetwork.NickName + " : " + ChatInput.text;
        PV.RPC("ChatRPC", RpcTarget.All, msg);
        ChatInput.text = "";
    }
    [PunRPC]
    void ChatRPC(string msg)
    {
        GameObject chat = Instantiate(ChatPrefab, ChatWindow.transform);
        chat.GetComponentInChildren<Text>().text = msg;
    }
    #endregion
}
