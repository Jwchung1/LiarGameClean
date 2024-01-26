using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviourPunCallbacks
{
    #region Inspector
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

    [Header("DescribeUI")]
    public InputField DescriptionInput;

    [Header("KillSaveBtn")]
    public Button KillBtn;
    public Button SaveBtn;

    [Header("WrapGame")]
    public InputField GuessInput;
    public Button LeaveBtn;
    #endregion

    #region 게임 설정 변수
    int TARGET_SCORE = 7;
    #endregion

    #region 게임 상태 변수
    private List<GamePlayer> gamePlayers;
    private string[] colors = { "red", "yellow", "green", "blue", "cyan", "purple", "magenta", "white" };

    private int myPlayerID;

    private int startTurn;
    private int curTurn;
    private bool isDescribing;

    private string liarName;
    private string answerSubject;
    private string answerWord;

    private Dictionary<string, int> VoteBox = new Dictionary<string, int>();
    private int votedNum;
    private string votedLiar;

    private int[] KillSaveVoteBox = new int[2];
    private bool isKill;

    private bool isGuessing;

    string winners = "";

    enum gameState { WAIT, SET_PLAYER , SET_GAME, DESCRIBE, VOTE, ARGUE, REVOTE,VOTE_COUNT, GUESS, WRAP_GAME, GAME_OVER }
    gameState curState = gameState.WAIT;
    private bool isStateOver = false;

    enum WinStatus { RightLiarWrongGuess, RightLiarRightGuess, WrongLiar};
    WinStatus winStatus;
    #endregion

    #region 게임 입퇴장
    public void MasterStartGame() // Master만 누를수있는 버튼 콜백 함수라 어차피 마스터만 사용함 => 마스터에서만 코루틴 돌아감 클라들은 단계별로 UI만 갱신됨
    {
        curState = gameState.SET_PLAYER;
        StartCoroutine(GameStateRoutine());
    }
    private bool CheckGameOver()
    {
        
        bool isGameOver = false;
        foreach(GamePlayer p in gamePlayers)
        {
            if (p.GetScore() >= TARGET_SCORE)
            {
                winners += p.GetName() + ", ";
                isGameOver = true;
            }
        }
        return isGameOver;
    }
    private void GameOver()
    {
        Announce("GAME OVER\n" + winners + " 님이 우승하였습니다!");
        PV.RPC("ClearGameRoom", RpcTarget.All);
        PV.RPC("ActivateLeaveBtn", RpcTarget.All);
    }
    [PunRPC] private void ClearGameRoom()
    {
        StopAllCoroutines();
        SubjectTxt.text = "";
        WordTxt.text = "";
        TimeTxt.text = "";
        AnnouncementTxt.text = "";
    }
    [PunRPC] private void ActivateLeaveBtn()
    {
        LeaveBtn.gameObject.SetActive(true);
    }
    public void OnClickLeaveBtn()
    {
        LeaveBtn.gameObject.SetActive(false);
        
    }
    #endregion

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && ScreenManager.SM.GetCurrentScreen() == 3)
        {
            if (isDescribing)
            {
                if (DescriptionInput.text != "")
                    SendDescription();
                DescriptionInput.Select();
            }
            else if(isGuessing)
            {
                if (GuessInput.text != "")
                    SendGuess();
                GuessInput.Select();
            }
            else
            {
                if (ChatInput.text != "")
                    Send();
                ChatInput.Select();
            }
        }
    }

    #region Game State Functions
    #region Change State
    private void ChangeState()
    {
        switch (curState) 
        {
            case gameState.WAIT:
                curState = gameState.WAIT; break;
            case gameState.SET_PLAYER:
                curState = gameState.SET_GAME; break;
            case gameState.SET_GAME:
                curState = gameState.DESCRIBE; break;
            case gameState.DESCRIBE:
                // 다시 첫턴으로 돌아오면 다음 단계로
                if(curTurn == startTurn)
                {
                    curState = gameState.VOTE; break;
                }
                // 아니면 다시 설명단계
                else
                {
                    curState = gameState.DESCRIBE; break;
                }
            case gameState.VOTE:
                curState = gameState.ARGUE; break;
            case gameState.ARGUE:
                if (isKill)
                {
                    curState = gameState.VOTE_COUNT; break;
                }
                else
                {
                    curState = gameState.REVOTE; break;
                }
            case gameState.REVOTE:
                curState = gameState.VOTE_COUNT; break;
            case gameState.VOTE_COUNT:
                if(votedLiar.Equals(liarName))
                {
                    curState = gameState.GUESS; break;
                }
                else
                {
                    curState = gameState.WRAP_GAME; break;
                }
            case gameState.GUESS:
                curState = gameState.WRAP_GAME; break;
            case gameState.WRAP_GAME:
                if(CheckGameOver())
                {
                    curState = gameState.GAME_OVER; break;
                }
                else
                {
                    curState = gameState.SET_GAME; break;
                }
                
        }
        StartCoroutine(GameStateRoutine());
    }
    private void ExecuteDefault()
    {
        switch (curState)
        {
            case gameState.SET_PLAYER:
                isStateOver = true; break;
            case gameState.SET_GAME:
                isStateOver = true; break;
            case gameState.DESCRIBE:
                PV.RPC("DescribeDone", RpcTarget.All, true);
                break;
            case gameState.VOTE:
                VoteDone();
                break;
            case gameState.ARGUE:
                ArgueDone();
                break;
            case gameState.REVOTE:
                VoteDone();
                break;
            case gameState.VOTE_COUNT:
                isStateOver = true; break;
            case gameState.GUESS:
                GuessDone("");
                break;
            case gameState.WRAP_GAME:
                isStateOver = true; break;
        }

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
            case gameState.SET_GAME:
                turnTime = 5; SetGame(); break;
            case gameState.DESCRIBE:
                turnTime = 30; Describe(); break;
            case gameState.VOTE:
                turnTime = 30; Vote(); break;
            case gameState.ARGUE:
                turnTime = 30; Argue(); break;
            case gameState.REVOTE:
                turnTime = 30; Revote(); break;
            case gameState.VOTE_COUNT:
                turnTime = 5; VoteCount(); break;
            case gameState.GUESS:
                turnTime = 20; Guess(); break;
            case gameState.WRAP_GAME:
                turnTime = 10; WrapGame(); break;
            case gameState.GAME_OVER:
                GameOver(); break;
        }

        while (true)
        {
            if (isStateOver)
            {
                ChangeState();
                break;
            }
            else if (turnTime <= 1)
            {
                ExecuteDefault();
            }
            turnTime--;
            PV.RPC("SyncTimerUI", RpcTarget.All, turnTime);
            yield return new WaitForSecondsRealtime(1.0f);
        }

    }

    [PunRPC]
    private void SyncTimerUI(int time)
    {
        TimeTxt.text = time.ToString();
    }
    #endregion

    #region Wait
    private void Wait()
    {
        return;
    }
    #endregion

    #region Set Player
    private void SetPlayer()
    {
        Announce("플레이어들을 위한 의자와 음료를 준비중입니다...");
        PV.RPC("SyncPlayers", RpcTarget.All);
    }
    [PunRPC]
    private void SyncPlayers()
    {
        // 참여한 플레이어 정보 전역변수 갱신
        gamePlayers = new List<GamePlayer>();
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            GamePlayer newGP = new GamePlayer(PhotonNetwork.PlayerList[i].NickName, colors[i]);
            if (newGP.GetName().Equals(PhotonNetwork.NickName))
                myPlayerID = i;
            gamePlayers.Add(newGP);
        }
        // UI 표시
        for (int i = 0; i < gamePlayers.Count; i++)
        {
            PlayerPanel[i].transform.GetChild(0).GetComponent<Text>().text = "<color=" + gamePlayers[i].GetColor() + ">" + gamePlayers[i].GetName() + "</color>";
            PlayerPanel[i].transform.GetChild(1).GetComponent<Text>().text = gamePlayers[i].GetScore() + "/"+TARGET_SCORE;
        }
    }
    #endregion

    #region Set Game
    private void SetGame()
    {
        Announce("라이어와 제시어를 정하는 중입니다...");
        // 마스터가 선턴 정함
        startTurn = Random.Range(0, gamePlayers.Count);
        curTurn = startTurn;

        // 마스터가 게임 정답 정함
        int liarID = Random.Range(0, gamePlayers.Count);
        liarName = gamePlayers[liarID].GetName();
        answerSubject = DB.GetRandomSubject();
        answerWord = DB.GetRandomWord(answerSubject);

        // 승리 정보 초기화
        winStatus = WinStatus.WrongLiar;

        // 다른 클라들한테 전달
        PV.RPC("SyncGame", RpcTarget.All, liarName, answerSubject, answerWord, startTurn);
    }
    [PunRPC]
    private void SyncGame(string liarName_, string answerSubject_, string answerWord_, int startTurn_)
    {
        // 게임 핵심 정보 전역변수 갱신
        startTurn = startTurn_;
        curTurn = startTurn_;
        liarName = liarName_;
        answerSubject = answerSubject_;
        answerWord = answerWord_;
        // UI 표시
        SubjectTxt.text = "주제: " + answerSubject;
        WordTxt.text = (gamePlayers[myPlayerID].GetName().Equals(liarName)) ? "당신은 Liar입니다." : "제시어: " + answerWord;
    }

    #endregion

    #region Describe
    private void Describe()
    {
        Announce(gamePlayers[curTurn].GetName() + " 님은 제시어를 설명해주세요.");
        PV.RPC("DescribeStart", RpcTarget.All, curTurn);
    }
    [PunRPC]
    private void DescribeStart(int curTurn_)
    {
        // 누구 턴인지 UI에 표시
        PlayerPanel[curTurn_].transform.GetChild(2).GetComponent<Text>().text = "O";
        // 내가 설명할 차례면 설명패널 활성화
        isDescribing = false;
        if (curTurn_ == myPlayerID)
        {
            isDescribing = true;
            DescriptionInput.gameObject.SetActive(true);
        }
    }
    [PunRPC]
    private void DescribeDone(bool isTimeOver)
    {
        if (isTimeOver)
        {
            string msg = "<color=orange>" + "시간초과" + "</color>";
            ChatRPC(msg);
        }
        // 턴끝났으면 UI에 O표시 삭제
        PlayerPanel[curTurn].transform.GetChild(2).GetComponent<Text>().text = "";
        // 설명패널 비활성화
        DescriptionInput.gameObject.SetActive(false);
        // 턴 넘기기
        curTurn = (curTurn + 1) % gamePlayers.Count;
        isDescribing = false;
        isStateOver = true;
    }
    public void SendDescription()
    {
        string msg = "<color=" + gamePlayers[myPlayerID].GetColor() + ">" + gamePlayers[myPlayerID].GetName() + "</color>: " + "<color=orange>" + DescriptionInput.text + "</color>";
        PV.RPC("ChatRPC", RpcTarget.All, msg);
        DescriptionInput.text = "";
        // 설명 끝났어
        PV.RPC("DescribeDone", RpcTarget.All, false);
    }
    #endregion

    #region Vote
    private void Vote()
    {
        InitVoteBox();
        Announce("라이어라고 생각되는 플레이어에게 투표해주세요.");
        PV.RPC("ActivateVoteBtn", RpcTarget.All);

        StartCoroutine(CheckVoteComplete());
    }
    IEnumerator CheckVoteComplete()
    {
        while(isStateOver == false)
        {
            // 모두가 투표했으면 투표결과 내고 state종료
            if(votedNum == gamePlayers.Count)
            {
                VoteDone();
                break;
            }
            yield return new WaitForSecondsRealtime(0.5f);
        }
    }
    private void VoteDone()
    {
        GetVoteResult();
        PV.RPC("DeactivateVoteBtn", RpcTarget.All);
        isStateOver = true;
    }
    private void GetVoteResult()
    {
        int maxVoteNum = 0;
        foreach(var kvp in VoteBox)
        {
            if(kvp.Value > maxVoteNum)
            {
                maxVoteNum = kvp.Value;
                votedLiar = kvp.Key;
            }
        }
    }
    private void InitVoteBox()
    {
        VoteBox.Clear();
        for(int i = 0; i < gamePlayers.Count; i++)
        {
            VoteBox.Add(gamePlayers[i].GetName(), 0);
        }
        votedNum = 0;
        votedLiar = "";
    }
    [PunRPC] private void ActivateVoteBtn()
    {
        for (int i = 0; i < gamePlayers.Count; i++)
        {
            PlayerPanel[i].transform.GetChild(3).GetComponent<Button>().interactable = true;
        }
    }
    [PunRPC] private void DeactivateVoteBtn()
    {
        for (int i = 0; i < gamePlayers.Count; i++)
        {
            PlayerPanel[i].transform.GetChild(3).GetComponent<Button>().interactable = false;
        }
    }

    public void OnClickVoteBtn(int PID)
    {
        DeactivateVoteBtn();
        PV.RPC("GetVotePaper", RpcTarget.MasterClient, gamePlayers[PID].GetName());
    }
    [PunRPC] private void GetVotePaper(string name)
    {
        VoteBox[name]++;
        votedNum++;
    }

    #endregion

    #region Argue
    private void Argue()
    {
        InitKillSaveVoteBox();
        Announce(votedLiar+" 님이 "+ VoteBox[votedLiar] +"표로 라이어로 지목되었습니다. 최후변론을 해주세요.");
        PV.RPC("ActivateKillSaveBtn", RpcTarget.All);

        StartCoroutine(CheckKillSaveVoteComplete());
    }
    IEnumerator CheckKillSaveVoteComplete()
    {
        while (isStateOver == false)
        {
            // 모두가 투표했으면 투표결과 내고 state종료
            if (votedNum == gamePlayers.Count)
            {
                ArgueDone();
                break;
            }
            yield return new WaitForSecondsRealtime(0.5f);
        }
    }
    private void ArgueDone()
    {
        GetKillSaveVoteResult();
        PV.RPC("DeactivateKillSaveBtn", RpcTarget.All);
        isStateOver = true;
    }
    private void GetKillSaveVoteResult()
    {
        isKill = (KillSaveVoteBox[0] >= KillSaveVoteBox[1]) ? true : false;
    }
    private void InitKillSaveVoteBox()
    {
        KillSaveVoteBox[0] = 0;
        KillSaveVoteBox[1] = 0;
        votedNum = 0;
        isKill = false;
    }
    [PunRPC] private void ActivateKillSaveBtn()
    {
        KillBtn.gameObject.SetActive(true);
        SaveBtn.gameObject.SetActive(true);
    }
    [PunRPC] private void DeactivateKillSaveBtn()
    {
        KillBtn.gameObject.SetActive(false);
        SaveBtn.gameObject.SetActive(false);
    }
    public void OnClickKillSaveBtn(int n)
    {
        DeactivateKillSaveBtn();
        PV.RPC("GetKillSaveVotePaper", RpcTarget.MasterClient, n);
    }
    [PunRPC] private void GetKillSaveVotePaper(int n)
    {
        KillSaveVoteBox[n]++;
        votedNum++;
    }
    #endregion

    #region Revote
    private void Revote()
    {
        InitVoteBox();
        Announce("라이어라고 생각되는 플레이어에게 다시 투표해주세요.");
        PV.RPC("ActivateVoteBtn", RpcTarget.All);

        StartCoroutine(CheckVoteComplete());
    }
    #endregion

    #region VoteCount
    private void VoteCount()
    {
        Announce(votedLiar + " 님이 " + VoteBox[votedLiar] + "표로 라이어로 최종 지목되었습니다.");
    }
    #endregion

    #region Guess
    private void Guess()
    {
        Announce(votedLiar + " 님은 라이어가 맞았습니다. 제시어를 맞춰보세요.\n힌트: "+GenerateHint());
        PV.RPC("ActivateGuess", RpcTarget.All);
    }
    private string GenerateHint()
    {
        string hint = "";
        for(int i = 0; i < answerWord.Length - 1; i++)
        {
            hint += "_.";
        }
        hint += "_";
        return hint;
    }
    [PunRPC]
    private void ActivateGuess()
    {
        if (gamePlayers[myPlayerID].GetName().Equals(liarName))
        {
            isGuessing = true;
            GuessInput.gameObject.SetActive(true);
        }
    }
    private void SendGuess()
    {
        string msg = "<color=" + gamePlayers[myPlayerID].GetColor() + ">" + gamePlayers[myPlayerID].GetName() + "</color>: " + "<color=orange>" + GuessInput.text + "</color>";
        PV.RPC("ChatRPC", RpcTarget.All, msg);
        PV.RPC("GuessDone", RpcTarget.All, GuessInput.text);
        GuessInput.text = "";
    }
    [PunRPC] private void GuessDone(string guess)
    {
        GuessInput.gameObject.SetActive(false);
        isGuessing = false;
        
        if(guess.Equals(answerWord))
        {
            winStatus = WinStatus.RightLiarRightGuess;
        }
        else
        {
            winStatus = WinStatus.RightLiarWrongGuess;
        }
        isStateOver = true;
    }
    #endregion

    #region WrapGame
    private void WrapGame()
    {
        bool isLiarWin = false;
        switch (winStatus)
        {
            case WinStatus.WrongLiar:
                Announce(votedLiar + " 님은 라이어가 아니었습니다.\n" + "진짜 라이어는 " + liarName + " 님이었습니다.\n라이어 승리!");
                isLiarWin = true;
                break;
            case WinStatus.RightLiarWrongGuess:
                Announce(votedLiar + " 님이 제시어를 맞추지 못했습니다.\n시민 승리!");
                isLiarWin = false;
                break;
            case WinStatus.RightLiarRightGuess:
                Announce(votedLiar + " 님이 제시어를 맞췄습니다.\n라이어 승리!");
                isLiarWin = true;
                break;
        }
        PV.RPC("GiveScore", RpcTarget.All, isLiarWin);
    }
    [PunRPC] private void GiveScore(bool isLiarWin)
    {
        // 라이어 승
        if (isLiarWin)
        {
            foreach(GamePlayer p in gamePlayers)
            {
                if (p.GetName().Equals(liarName))
                    p.AddScore(2);
            }
        }
        // 시민 승
        else
        {
            foreach (GamePlayer p in gamePlayers)
            {
                if (!p.GetName().Equals(liarName))
                    p.AddScore(1);
            }
        }
        // UI 갱신
        for (int i = 0; i < gamePlayers.Count; i++)
        {
            PlayerPanel[i].transform.GetChild(1).GetComponent<Text>().text = gamePlayers[i].GetScore() + "/"+TARGET_SCORE;
        }
    }
    #endregion

    #endregion


    #region 공지
    private void Announce(string msg)
    {
        PV.RPC("AnnounceRPC", RpcTarget.All, msg);
    }
    [PunRPC] private void AnnounceRPC(string msg)
    {
        AnnouncementTxt.text = msg;
    }
    #endregion

    #region 채팅
    public void Send()
    {
        string msg = "<color="+ gamePlayers[myPlayerID].GetColor()+">" + gamePlayers[myPlayerID].GetName() +"</color>: " + ChatInput.text;
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
