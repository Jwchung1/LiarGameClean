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

    [Header("DescribeUI")]
    public InputField DescriptionInput;

    #region ���� ���� ����
    private List<GamePlayer> gamePlayers;
    private string[] colors = { "red", "yellow", "green", "blue", "cyan", "purple", "magenta", "white" };

    private int myPlayerID;

    private int startTurn;
    private int curTurn;

    private string liarName;
    private string answerSubject;
    private string answerWord;

    enum gameState { WAIT, SET_PLAYER , SET_GAME, DESCRIBE, VOTE, ARGUE, REVOTE, WRAP_GAME }
    gameState curState = gameState.WAIT;
    private bool isStateOver = false;
    #endregion


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && ScreenManager.SM.GetCurrentScreen() == 3)
        {
            if (curTurn == myPlayerID)
            {
                if (DescriptionInput.text != "")
                    SendDescription();
                DescriptionInput.Select();
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
                // �ٽ� ù������ ���ƿ��� ���� �ܰ��
                if(curTurn == startTurn)
                {
                    curState = gameState.VOTE; break;
                }
                // �ƴϸ� �ٽ� ����ܰ�
                else
                {
                    curState = gameState.DESCRIBE; break;
                }
        }
        StartCoroutine(GameStateRoutine());
    }
    private void Wait()
    {
        Debug.Log("Wait");
    }

    private void SetPlayer()
    {
        Announce("�÷��̾���� ���� ���ڿ� ���Ḧ �غ����Դϴ�...");
        PV.RPC("SyncPlayers", RpcTarget.All);
    }

    private void SetGame()
    {
        Announce("���̾�� ���þ ���ϴ� ���Դϴ�...");
        // �����Ͱ� ���� ����
        startTurn = Random.Range(0, gamePlayers.Count);
        curTurn = startTurn;

        // �����Ͱ� ���� ���� ����
        int liarID = Random.Range(0, gamePlayers.Count);
        liarName = gamePlayers[liarID].GetName();
        answerSubject = DB.GetRandomSubject();
        answerWord = DB.GetRandomWord(answerSubject);

        // �ٸ� Ŭ������� ����
        PV.RPC("SyncGame", RpcTarget.All, liarName, answerSubject, answerWord, startTurn);
    }

    private void Describe()
    {
        Announce(gamePlayers[curTurn].GetName() + " ���� ���þ �������ּ���.");
        PV.RPC("DescribeStart", RpcTarget.All, curTurn);
    }

    private void Vote()
    {
        Announce("���̾��� �����Ǵ� �÷��̾�� ��ǥ���ּ���.");
    }








    private void ExecuteDefault()
    {
        switch (curState)
        {
            case gameState.DESCRIBE:
                PV.RPC("DescribeDefault", RpcTarget.All);
                break;
        }

    }
    #endregion

    public void MasterStartGame() // Master�� �������ִ� ��ư �ݹ� �Լ��� ������ �����͸� ����� => �����Ϳ����� �ڷ�ƾ ���ư� Ŭ����� �ܰ躰�� UI�� ���ŵ�
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
            case gameState.SET_GAME:
                turnTime = 5; SetGame(); break;
            case gameState.DESCRIBE:
                turnTime = 30; Describe(); break;
            case gameState.VOTE:
                turnTime = 30; Vote(); break;
        }

        while (true)
        {
            if (isStateOver)
            {
                ChangeState();
                break;
            }
            else if(turnTime <= 0)
            {
                ExecuteDefault();
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
        // ������ �÷��̾� ���� �������� ����
        gamePlayers = new List<GamePlayer>();
        for(int i=0; i<PhotonNetwork.PlayerList.Length; i++)
        {
            GamePlayer newGP = new GamePlayer(PhotonNetwork.PlayerList[i].NickName, colors[i]);
            if (newGP.GetName().Equals(PhotonNetwork.NickName))
                myPlayerID = i;
            gamePlayers.Add(newGP);
        }
        // UI ǥ��
        for (int i = 0; i < gamePlayers.Count; i++)
        {
            PlayerPanel[i].transform.GetChild(0).GetComponent<Text>().text = "<color=" + gamePlayers[i].GetColor() + ">" + gamePlayers[i].GetName() + "</color>";
            PlayerPanel[i].transform.GetChild(1).GetComponent<Text>().text = gamePlayers[i].GetScore() + "/5";
        }
    }
    [PunRPC] private void SyncGame(string liarName_, string answerSubject_, string answerWord_, int startTurn_)
    {
        // ���� �ٽ� ���� �������� ����
        startTurn = startTurn_;
        curTurn = startTurn_;
        liarName = liarName_;
        answerSubject = answerSubject_;
        answerWord = answerWord_;
        // UI ǥ��
        SubjectTxt.text = "����: " + answerSubject;
        WordTxt.text = (gamePlayers[myPlayerID].GetName().Equals(liarName)) ? "����� Liar�Դϴ�." : "���þ�: " + answerWord;
    }

    [PunRPC] private void DescribeStart(int curTurn_)
    {
        curTurn = curTurn_;
        // ���� ������ ���ʸ� �����г� Ȱ��ȭ
        if(curTurn == myPlayerID) 
        {
            DescriptionInput.gameObject.SetActive(true);
        }
    }
    [PunRPC] private void DescribeDefault()
    {
        // �� ���̾��µ� �ð��� �� ������
        if(curTurn == myPlayerID)
        {
            // ������ ������ ������ ����
            SendDescription();
        }
    }
    [PunRPC] private void DescribeDone()
    {
        // �����г� ��Ȱ��ȭ
        DescriptionInput.gameObject.SetActive(false);
        // �� �ѱ��
        curTurn = (curTurn + 1) % gamePlayers.Count;
        isStateOver = true;
    }
    public void SendDescription()
    {
        string msg = "<color="+ gamePlayers[myPlayerID].GetColor()+">" + gamePlayers[myPlayerID].GetName() +"</color>: " + "<color=yellow>"+ DescriptionInput.text + "</color>";
        PV.RPC("ChatRPC", RpcTarget.All, msg);
        DescriptionInput.text = "";
        // ���� ������
        PV.RPC("DescribeDone", RpcTarget.All);
    }
    #region ����
    private void Announce(string msg)
    {
        PV.RPC("AnnounceRPC", RpcTarget.All, msg);
    }
    [PunRPC] private void AnnounceRPC(string msg)
    {
        AnnouncementTxt.text = msg;
    }
    #endregion

    #region ä��
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
