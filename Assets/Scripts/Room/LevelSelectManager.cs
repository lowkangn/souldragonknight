using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

using Hashtable = ExitGames.Client.Photon.Hashtable;

public class LevelSelectManager : MonoBehaviourPunCallbacks
{
    public static readonly string ROOM_PROPERTIES_LEVELS_CLEARED = "levelsCleared";
    public static readonly string PLAYER_PROPERTIES_LEVEL_SELECTED = "levelSelected";

    [SerializeField] private Button startButton;
    [SerializeField] private string gameSceneName;
    [SerializeField] private string roleSelectSceneName;
    [SerializeField] private LevelButton[] levelSelectButtons;

    private void Start()
    {
        int levelsCleared = (int)PhotonNetwork.CurrentRoom.CustomProperties[ROOM_PROPERTIES_LEVELS_CLEARED];

        for (int i = 0; i <= levelsCleared; i++)
        {
            levelSelectButtons[i].SetInteractable(true);
        }

        foreach (Player player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            object playerTypeObj = player.CustomProperties[PLAYER_PROPERTIES_LEVEL_SELECTED];
            if (playerTypeObj != null)
            {
                int levelNumber = (int)playerTypeObj;
                bool isLocalPlayer = (player == PhotonNetwork.LocalPlayer);
                levelSelectButtons[levelNumber - 1].UpdateIndicators(levelNumber, isLocalPlayer);
            }
        }
    }
    public static void SelectLevel(int levelNumber)
    {
        Hashtable playerProperties = new Hashtable();
        playerProperties[PLAYER_PROPERTIES_LEVEL_SELECTED] = levelNumber;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
    }

    public static void SetLevelsCleared(int levelsCleared)
    {
        RoomManager.UpdateRoomProperty(ROOM_PROPERTIES_LEVELS_CLEARED, levelsCleared);
    }
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);

        int levelNumber = (int)targetPlayer.CustomProperties[PLAYER_PROPERTIES_LEVEL_SELECTED];
        bool isLocalPlayer = (targetPlayer == PhotonNetwork.LocalPlayer);
        Debug.Log($"Player {targetPlayer.ActorNumber} chose level {levelNumber}");

        startButton.interactable = CanStart();
        foreach (LevelButton button in levelSelectButtons)
        {
            button.UpdateIndicators(levelNumber, isLocalPlayer);
        }
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);

        PhotonNetwork.LoadLevel(roleSelectSceneName);
    }

    private bool CanStart()
    {
        HashSet<int> selectedLevels = new HashSet<int>();

        foreach (Player player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            object playerTypeObj = player.CustomProperties[PLAYER_PROPERTIES_LEVEL_SELECTED];
            if (playerTypeObj == null)
            {
                return false;
            }

            selectedLevels.Add((int)playerTypeObj);
        }

        return selectedLevels.Count == 1 && PhotonNetwork.CurrentRoom.PlayerCount == 2;
    }

    private void ResetLevelChoice()
    {
        Hashtable playerProperties = new Hashtable();
        playerProperties[PLAYER_PROPERTIES_LEVEL_SELECTED] = null;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
    }

    public void StartGame()
    {
        // TODO: Change when more levels are added.
        Debug.Log("Starting game...");
        photonView.RPC("RPC_LoadGameLevel", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_LoadGameLevel()
    {
        PhotonNetwork.LoadLevel(gameSceneName);
        ResetLevelChoice();
    }
}
