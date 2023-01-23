using Photon.Pun;
using Unity.XR.CoreUtils;
using UnityEngine;
using TMPro;
using UnityEngine.XR;

public class PlayerSetup : MonoBehaviourPunCallbacks
{
    //[SerializeField] private Transform _head;
    [SerializeField] private Transform _avatar;
    [SerializeField] private Transform _leftHand;
    [SerializeField] private Transform _rightHand;
    [SerializeField] private TextMeshProUGUI _playerNameText;

    [SerializeField] private Animator _avatarWalkAnimator;
    [SerializeField] private Animator _leftHandAnimator;
    [SerializeField] private Animator _rightHandAnimator;

    //private Transform _headRig;
    private Transform _avatarRig;
    private Transform _leftHandRig;
    private Transform _rightHandRig;

    private XROrigin origin;

    private void Start()
    {
        origin = FindObjectOfType<XROrigin>();

        //_headRig = origin.transform.Find("Camera Offset/Main Camera");
        _avatarRig = origin.transform.Find("Camera Offset/Main Camera");
        _leftHandRig = origin.transform.Find("Camera Offset/LeftHand Controller");
        _rightHandRig = origin.transform.Find("Camera Offset/RightHand Controller");

        if (photonView.IsMine)
        {
            foreach (var item in GetComponentsInChildren<Renderer>())
            {
                item.enabled = false;
            }
        }
        else 
        {
            SetPlayerName();
        }
    }

    private void Update()
    {
        if (photonView.IsMine)
        {
            //MapPosition(_head, _headRig);
            MapAvatarPosition(_avatar, _avatarRig);
            MapHandPosition(_leftHand, _leftHandRig);
            MapHandPosition(_rightHand, _rightHandRig);

            UpdateAvatarWalkAnimation(InputDevices.GetDeviceAtXRNode(XRNode.LeftHand), _avatarWalkAnimator);
            UpdateHandAnimation(InputDevices.GetDeviceAtXRNode(XRNode.LeftHand), _leftHandAnimator);
            UpdateHandAnimation(InputDevices.GetDeviceAtXRNode(XRNode.RightHand), _rightHandAnimator);
        }
    }

    private void SetPlayerName()
    {
        if (_playerNameText != null)
        {
            _playerNameText.text = photonView.Owner.NickName;
        }
    }

    private void MapHandPosition(Transform target, Transform originTransform)
    {
        target.position = originTransform.position;
        target.rotation = originTransform.rotation;
    }

    private void MapAvatarPosition(Transform target, Transform originTransform)
    {
        target.position = new Vector3(originTransform.position.x, target.position.y, originTransform.position.z);
        target.eulerAngles = new Vector3(0, originTransform.eulerAngles.y, 0);
    }

    private void UpdateHandAnimation(InputDevice targetDevice, Animator handAnimator)
    {
        if (targetDevice.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue))
        {
            handAnimator.SetFloat("Trigger", triggerValue);
        }
        else
        {
            handAnimator.SetFloat("Trigger", 0);
        }

        if (targetDevice.TryGetFeatureValue(CommonUsages.grip, out float gripValue))
        {
            handAnimator.SetFloat("Grip", gripValue);
        }
        else
        {
            handAnimator.SetFloat("Grip", 0);
        }
    }

    private void UpdateAvatarWalkAnimation(InputDevice targetDevice, Animator handAnimator)
    {
        if (targetDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 inputAxisLeft))
        {
            handAnimator.SetFloat("Horizontal", inputAxisLeft.x);
            handAnimator.SetFloat("Vertical", inputAxisLeft.y);
        }
        else
        {
            handAnimator.SetFloat("Horizontal", 0);
            handAnimator.SetFloat("Vertical", 0);
        }
    }
}