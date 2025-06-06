using BepInEx;
using UnityEngine;
using static Spectape.Info;

namespace Spectape
{
    [BepInPlugin(Guid, Name, Version)]
    public class Main : BaseUnityPlugin
    {
        private bool allowed;
        GameObject? spectape;
        private bool parentSet;
        private float fov;

        private void Awake()
        {
            GorillaTagger.OnPlayerSpawned(Init);
        }

        void Init()
        {
            NetworkSystem.Instance.OnMultiplayerStarted += JoinedRoom;
            NetworkSystem.Instance.OnReturnedToSinglePlayer += OnLeaveRoom;
            fov = Camera.main.fieldOfView; // idk if this works or does anything but ok
        }

        void FixedUpdate()
        {
            if (!allowed) return;
            foreach (VRRig rig in GorillaParent.instance.vrrigs)
            {
                if (rig == null) continue;
                if (rig.isLocal) continue;
                if (Vector3.Distance(rig.headMesh.transform.position,
                        GorillaTagger.Instance.rightHandTransform.position) < 0.2f)
                {
                    if (ControllerInputPoller.instance.rightGrab)
                    {
                        if (!parentSet)
                        {
                            spectape?.transform.SetParent(rig.headMesh.transform, false);
                            parentSet = true;
                        }
                    }
                    else
                    {
                        if (parentSet)
                        {
                            spectape?.transform.SetParent(GorillaTagger.Instance.headCollider.transform, false);
                            parentSet = false;
                        }
                    }
                }
                else
                {
                    if (!ControllerInputPoller.instance.rightGrab)
                    {
                        if (parentSet)
                        {
                            spectape?.transform.SetParent(GorillaTagger.Instance.headCollider.transform, false);
                            parentSet = false;
                        }
                    }
                }
            }
        }

        void JoinedRoom()
        {
            allowed = NetworkSystem.Instance.GameModeString.Contains("MODDED");
            if (allowed)
            {
                spectape = CameraObj();
                parentSet = false;
                spectape.transform.SetParent(GorillaTagger.Instance.headCollider.transform, false);
            }
        }

        void OnLeaveRoom()
        {
            allowed = false;
            Destroy(spectape);
            spectape = null;
            parentSet = false;
        }

        GameObject CameraObj()
        {
            GameObject cameraObj = new GameObject("SpectapeCam");
            Camera cam = cameraObj.AddComponent<Camera>();
            cam.fieldOfView = fov;
            cam.farClipPlane = 5000f;
            cam.nearClipPlane = 0.01f;
            return cameraObj;
        }
    }
}