using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatManager : MonoBehaviour
{
    public PhotonView PV;

    private static ChatManager instance = null;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public static ChatManager CM 
    {  get { return instance; } }

    public void Send()
    {

    }
}
