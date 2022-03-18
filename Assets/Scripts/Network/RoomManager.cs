using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

using Hashtable = ExitGames.Client.Photon.Hashtable;

public class RoomManager : MonoBehaviourPunCallbacks
{
    private bool isRequestingRestart = false;

    [SerializeField] private string gameSceneName;
    [SerializeField] private string mainMenuSceneName;
    [SerializeField] private string roomSceneName;

    public static void UpdateRoomProperty(string key, object value)
    {
        Hashtable roomProperty = new Hashtable();
        roomProperty[key] = value;
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperty);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"Player {otherPlayer.ActorNumber} has left the game");

        photonView.RPC("RPC_LoadRoomLevel", RpcTarget.All);
        base.OnPlayerLeftRoom(otherPlayer);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        PhotonNetwork.LoadLevel(mainMenuSceneName);
    }

    public void AttemptRestart()
    {
        if (isRequestingRestart)
        {
            RestartLevel();
        }
        else
        {
            RequestRestart();
        }
    }

    public void CancelRestartRequest()
    {
        photonView.RPC("RPC_CancelRestartRequest", RpcTarget.Others);
    }

    private void RequestRestart()
    {
        photonView.RPC("RPC_RequestRestart", RpcTarget.Others);
    }

    private void RestartLevel()
    {
        photonView.RPC("RPC_LoadGameLevel", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_RequestRestart()
    {
        Debug.Log("Partner is requesting restart");
        isRequestingRestart = true;
    }

    [PunRPC]
    private void RPC_CancelRestartRequest()
    {
        Debug.Log("Partner is no longer requesting restart");
        isRequestingRestart = false;
    }

    [PunRPC]
    public void RPC_LoadGameLevel()
    {
        PhotonNetwork.LoadLevel(gameSceneName);
    }

    [PunRPC]
    public void RPC_LoadRoomLevel()
    {
        PhotonNetwork.LoadLevel(roomSceneName);
    }
}
