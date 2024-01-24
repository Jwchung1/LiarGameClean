using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public PhotonView PV;

    public Text TimeTxt;

    public bool isTimeOver;

    private static Timer instance = null;
    private void Awake()
    {
        if(instance == null)
            instance = this;
    }

    public static Timer T {  get { return instance; } }

    public void SetTimer(int time)
    {
        isTimeOver = false;
        StartCoroutine(TimerRoutine(time));
    }
    IEnumerator TimerRoutine(int time)
    {
        while(time > 0)
        {
            time--;
            PV.RPC("SyncTimerUI", RpcTarget.All, time);
            yield return new WaitForSecondsRealtime(1.0f);
        }
        // 타이머 끝
        isTimeOver=true;
    }
    [PunRPC] private void SyncTimerUI(int time)
    {
        TimeTxt.text = time.ToString();
    }
}
