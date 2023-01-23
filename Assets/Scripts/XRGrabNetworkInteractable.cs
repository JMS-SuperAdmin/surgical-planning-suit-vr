using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Photon.Pun;

public class XRGrabNetworkInteractable : XRGrabInteractable
{
    private PhotonView _photonView;
    private Rigidbody _rigidbody;
    private float _defaultAngularDrag;

    void Start()
    {
        _photonView = GetComponent<PhotonView>();
        _rigidbody = GetComponent<Rigidbody>();
        _defaultAngularDrag = _rigidbody.angularDrag;
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        _photonView.RequestOwnership();
        _photonView.RPC("GameObjectOnGripEnter", RpcTarget.OthersBuffered);
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        _photonView.RPC("GameObjectOnGripExit", RpcTarget.OthersBuffered);
    }

    [PunRPC]
    private void GameObjectOnGripEnter()
    {
        _rigidbody.useGravity = false;
        _rigidbody.isKinematic = true;
        _rigidbody.angularDrag = 0f;
    }

    [PunRPC]
    private void GameObjectOnGripExit()
    {
        _rigidbody.isKinematic = false;
        _rigidbody.useGravity = true;
        _rigidbody.angularDrag = _defaultAngularDrag;
    }
}
