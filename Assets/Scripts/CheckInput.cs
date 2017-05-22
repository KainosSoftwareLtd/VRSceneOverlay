using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class CheckInput : MonoBehaviour
{
    private SteamVR_Controller.Device device;
    private SteamVR_TrackedObject sto;
    public GameObject hat;
    void Update ()
    {
        sto = GetComponent<SteamVR_TrackedObject>();
        device = SteamVR_Controller.Input((int)sto.index);
        if (device.GetPressDown(EVRButtonId.k_EButton_SteamVR_Trigger))
            hat.SetActive(!hat.activeSelf);
    }
}
