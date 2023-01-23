using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private GameObject _playerPrefab;

    private void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            if (_playerPrefab != null)
            {
                PhotonNetwork.Instantiate(_playerPrefab.name, _playerPrefab.transform.position, _playerPrefab.transform.localRotation);
            }
            else
            {
                Debug.Log("Player prefab not assigned");
            }
        }
    }
}
