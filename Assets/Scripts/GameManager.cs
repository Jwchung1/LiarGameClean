using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviourPunCallbacks
{
    public PhotonView PV;
    public Database DB;

    [Header("Chat")]
    public InputField ChatInput;
    public GameObject ChatPrefab;
    public GameObject ChatWindow;

    [Header("PlayerInfo")]
    public GameObject[] PlayerPanel;

    [Header("GameInfo")]
    public Text SubjectTxt;
    public Text WordTxt;
    public Text AnnouncementTxt;
    public Text TimeTxt;

    #region 게임 상태 변수
    private List<GamePlayer> gamePlayers;
    private string[] colors = { "red", "yellow", "green", "blue", "cyan", "purple", "magenta", "white" };

    private string myName;
    private string liarName;
    private string answerSubject;
    private string answerWord;

    enum gameState { WAIT, SET_PLAYER , SELECT_LIAR, DESCRIBE, VOTE, ARGUE, REVOTE, WRAP_GAME }
    gameState curState = gameState.WAIT;
    private bool isStateOver = false;
    #endregion


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && ScreenManager.SM.GetCurrentScreen() == 3)
        {
            if (ChatInput.text != "")
                Send();
            ChatInput.Select();
        }
    }

    #region Game State Functions
    private void ChangeState()
    {
        switch (curState) 
        {
            case gameState.WAIT:
                curState = gameState.WAIT; break;
            case gameState.SET_PLAYER:
                curState = gameState.SELECT_LIAR; break;
            case gameState.SELECT_LIAR:
                curState = gameState.DESCRIBE; break;
        }
        StartCoroutine(GameStateRoutine());
    }
    private void Wait()
    {
        Debug.Log("Wait");
    }

    private void SetPlayer()
    {
        Debug.Log("SetPlayer");
        PV.RPC("SyncPlayers", RpcTarget.All);
    }

    private void SelectLiar()
    {
        Debug.Log("SelectLiar");
        // 마스터가 게임 정답 정함
        int liarID = Random.Range(0, gamePlayers.Count);
        liarName = gamePlayers[liarID].GetName();
        answerSubject = DB.GetRandomSubject();
        answerWord = DB.GetRandomWord(answerSubject);
        // 다른 클라들한테 전달
        PV.RPC("SyncGameAnswer", RpcTarget.All, liarName, answerSubject, answerWord);
    }

    private void Describe()
    {
        Debug.Log("Describe");
    }
    #endregion

    public void MasterStartGame() // Master만 누를수있는 버튼 콜백 함수라 어차피 마스터만 사용함 => 마스터에서만 코루틴 돌아감 클라들은 단계별로 UI만 갱신됨
    {
        curState = gameState.SET_PLAYER;
        StartCoroutine(GameStateRoutine());
    }

    IEnumerator GameStateRoutine()
    {
        int turnTime = 0;
        isStateOver = false;
        switch (curState)
        {
            case gameState.WAIT:
                Wait(); break;
            case gameState.SET_PLAYER:
                turnTime = 3; SetPlayer(); break;
            case gameState.SELECT_LIAR:
                turnTime = 5; SelectLiar(); break;
            case gameState.DESCRIBE:
                turnTime = 30; Describe(); break;
        }

        while (true)
        {
            if (isStateOver || turnTime < 0)
            {
                ChangeState();
                break;
            }
            turnTime--;
            PV.RPC("SyncTimerUI", RpcTarget.All, turnTime);
            yield return new WaitForSecondsRealtime(1.0f);
        }

    }


    [PunRPC] private void SyncTimerUI(int time)
    {
        TimeTxt.text = time.ToString();
    }
    [PunRPC] private void SyncPlayers()
    {
        // 참여한 플레이어 정보 전역변수 갱신
        myName = PhotonNetwork.LocalPlayer.NickName;
        gamePlayers = new List<GamePlayer>();
        for(int i=0; i<PhotonNetwork.PlayerList.Length; i++)
        {
            GamePlayer newGP = new GamePlayer(PhotonNetwork.PlayerList[i].NickName, colors[i]);
            gamePlayers.Add(newGP);
        }
        // UI 표시
        for (int i = 0; i < gamePlayers.Count; i++)
        {
            PlayerPanel[i].transform.GetChild(0).GetComponent<Text>().text = "<color=" + gamePlayers[i].GetColor() + ">" + gamePlayers[i].GetName() + "</color>";
            PlayerPanel[i].transform.GetChild(1).GetComponent<Text>().text = gamePlayers[i].GetScore() + "/5";
        }
    }
    [PunRPC] private void SyncGameAnswer(string liarName_, string answerSubject_, string answerWord_)
    {
        // 게임 핵심 정보 전역변수 갱신
        liarName = liarName_;
        answerSubject = answerSubject_;
        answerWord = answerWord_;
        // UI 표시
        SubjectTxt.text = "주제: " + answerSubject;
        WordTxt.text = (myName.Equals(liarName)) ? "당신은 Liar입니다." : "제시어: " + answerWord;
    }
    #region 채팅
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
