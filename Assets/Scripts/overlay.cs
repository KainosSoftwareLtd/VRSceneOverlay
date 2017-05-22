using System;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Random = System.Random;

public class overlay : MonoBehaviour
{
    public Texture OverlayTexture;
    private Vector4 UvOffset = new Vector4(0, 0, 1, 1);
    private Vector3 AnchorOffset = new Vector3(0, 0, 0.5f);
    public float Alpha = 1;
    public static Random Rand = new Random();
    public static string Key { get { return "unity:" + Application.companyName + "." + Application.productName + "." + Rand.Next(); } }
    public ulong Handle { get { return _handle; } }
    protected ulong _handle = OpenVR.k_ulOverlayHandleInvalid;

    public void Update()
    {
        if (_anchorOffset != AnchorOffset)
        {
            gameObject.transform.localPosition = AnchorOffset;
            OverlayReference.transform.localRotation = Quaternion.identity;
            UpdateOverlay();
        }
        else
            UpdateTexture();
        UpdatePoses();
    }

    public void OnEnable()
    {
        var svr = SteamVR.instance;
        var overlay = OpenVR.Overlay;
        if (overlay == null) return;
        var error = overlay.CreateOverlay(Key + gameObject.GetInstanceID(), gameObject.name, ref _handle);
        var rt = RotationTracker;
        if (error == EVROverlayError.None) return;
        Debug.Log(error.ToString());
        enabled = false;
    }

    public void OnDisable()
    {
        if (_handle == OpenVR.k_ulOverlayHandleInvalid) return;
        var overlay = OpenVR.Overlay;
        if (overlay != null) overlay.DestroyOverlay(_handle);
        _handle = OpenVR.k_ulOverlayHandleInvalid;
    }

    private void UpdateOverlay()
    {
        var overlay = OpenVR.Overlay;
        if (overlay == null) return;
        if (OverlayTexture != null)
        {
            var error = overlay.ShowOverlay(_handle);
            if (error == EVROverlayError.InvalidHandle || error == EVROverlayError.UnknownOverlay)
            {
                if (overlay.FindOverlay(Key + gameObject.GetInstanceID(), ref _handle) != EVROverlayError.None) return;
            }
            var tex = new Texture_t
            {
                handle = OverlayTexture.GetNativeTexturePtr(),
                eType = SteamVR.instance.graphicsAPI,
                eColorSpace = EColorSpace.Auto
            };
            overlay.SetOverlayColor(_handle, 1f, 1f, 1f);
            overlay.SetOverlayTexture(_handle, ref tex);
            overlay.SetOverlayAlpha(_handle, Alpha);
            var textureBounds = new VRTextureBounds_t
            {
                uMin = (0 + UvOffset.x) * UvOffset.z,
                vMin = (1 + UvOffset.y) * UvOffset.w,
                uMax = (1 + UvOffset.x) * UvOffset.z,
                vMax = (0 + UvOffset.y) * UvOffset.w
            };
            overlay.SetOverlayTextureBounds(_handle, ref textureBounds);
            var vrcam = SteamVR_Render.Top();
            var offset = new SteamVR_Utils.RigidTransform(OverlayReference.transform, transform);
            offset.pos.x /= OverlayReference.transform.localScale.x;
            offset.pos.y /= OverlayReference.transform.localScale.y;
            offset.pos.z /= OverlayReference.transform.localScale.z;
            var t = offset.ToHmdMatrix34();
            overlay.SetOverlayTransformTrackedDeviceRelative(_handle, 0, ref t);
        }
        else
        {
            overlay.HideOverlay(_handle);
        }
    }

    private void UpdateTexture(bool refresh = false)
    {
        if (refresh && OverlayTexture == null) return;
        var overlay = OpenVR.Overlay;
        if (overlay == null) return;
        var tex = new Texture_t
        {
            handle = OverlayTexture.GetNativeTexturePtr(),
            eType = SteamVR.instance.graphicsAPI,
            eColorSpace = EColorSpace.Auto
        };
        overlay.SetOverlayTexture(_handle, ref tex);
    }

    private readonly TrackedDevicePose_t[] _poses = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
    private readonly TrackedDevicePose_t[] _gamePoses = new TrackedDevicePose_t[0];

    private void UpdatePoses()
    {
        var compositor = OpenVR.Compositor;
        if (compositor == null) return;
        compositor.GetLastPoses(_poses, _gamePoses);
        SteamVR_Utils.Event.Send("new_poses", _poses);
        SteamVR_Utils.Event.Send("new_poses_applied");
    }

    public GameObject RotationTracker
    {
        get
        {
            if (_rotationTracker != null) return _rotationTracker;
            _rotationTracker = new GameObject("Overlay Rotation Tracker" + gameObject.name);
            _rotationTracker.transform.parent = transform.parent;
            _rotationTracker.SetActive(true);
            return _rotationTracker;
        }
    }
    private GameObject _rotationTracker;
    public GameObject OverlayReference
    {
        get
        {
            return _overlayReference ?? (_overlayReference = new GameObject("Overlay Reference" + GetType()));
        }
    }
    private GameObject _overlayReference;
    protected Vector3 _anchorOffset = Vector3.zero;
}