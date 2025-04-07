using System.Diagnostics.Eventing.Reader;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Drone_Forcefield : UdonSharpBehaviour
{
    private VRCPlayerApi localPlayer;
    private VRCDroneApi localDrone;
    private Transform center;
    private bool isInsideBox = false;

    // Teleport Settings
    [Tooltip("If true, the drone will be teleported to the teleport location instead of being pushed.")]
    public bool shouldTeleportInsteadOfPush = false;
    public Transform teleportDestination;
    [Tooltip("If true, the drone will keep its rotation when being teleported.")]
    public bool keepDroneRotation = true;
    [Tooltip("If true, the drone's velocity will be reset to zero when being teleported.")]
    public bool resetVelocityOnTeleport = true;

    [Tooltip("If true, the drone will be teleported when it enters the trigger.")]
    public bool teleportOnEnter = true;
    [Tooltip("If true, the drone will be teleported when it exits the trigger.")]
    public bool teleportOnExit = false;
    [Tooltip("If true, the drone will be teleported while it stays inside the trigger.")]
    public bool teleportOnStay = false;


    // Push Settings
    [Tooltip("The strength of the push force applied to the drone.")]
    public float dronePushStrength = 10f;
    [Tooltip("If true, the drone will be pushed away from the specified point. If false, the drone will be pushed in a consstant direction.")]
    public bool shouldPushFromPoint = true; 
    [Tooltip("The point from which the drone will be pushed away from. If unset, defaults to the origin of this object.")]
    public Transform pushFromTransform;
    public Vector3 pushDirection = Vector3.forward;
    public Quaternion pushRotation = Quaternion.LookRotation(Vector3.forward);
    [Tooltip("If true, the push direction will be relative to the object instead of the world.")]
    public bool pushDirectionInLocalSpace = false;
    [Tooltip("If true, instead of pushing the drone it will just set its velocity to the push direction multiplied by the push strength.")]
    public bool overrideVelocity = false;
    [Tooltip("Push the drone when it enters the trigger.")]
    public bool pushOnEnter = false;
    [Tooltip("Push the drone when it exits the trigger.")]
    public bool pushOnExit = false;
    [Tooltip("Push the drone while it stays inside the trigger.")]
    public bool pushOnStay = true;


    // Other Settings
    [Tooltip("If true, the drone will not be affected if the player controlling it is inside of this object's trigger.")]
    public bool ignoreIfPlayerInsideTrigger = false;
    [Tooltip("If true, the drone will not be affected no matter what. This can be set from other scripts, for example to disable the forcefield for users with certain permissions, users on certain teams, etc.")]
    public bool ignorePlayer = false;

    [Tooltip("Add UdonBehaviours here where you want a custom event to be sent when a drone enters the trigger. Triggered events are: OnDroneForcefieldEnter")]
    public GameObject[] onEnterEventReceivers;
    [Tooltip("Add UdonBehaviours here where you want a custom event to be sent when a drone exits the trigger. Triggered events are: OnDroneForcefieldExit")]
    public GameObject[] onExitEventReceivers;

    private UdonBehaviour[] onEnterEventReceivers_UdonBehaviours;
    private UdonBehaviour[] onExitEventReceivers_UdonBehaviours;

    // private float lastPushTime = 0f;


    void Start()
    {
        Debug.Log("[DFF] Drone Forcefield initialized.");
        localPlayer = Networking.LocalPlayer;
        if (localPlayer == null) return;
        localDrone = localPlayer.GetDrone();
        if (localDrone == null) 
        {
            Debug.LogWarning("[DFF] Start: No local drone found.");
        }
        center = transform;

        if (onEnterEventReceivers == null || onEnterEventReceivers.Length == 0)
        {
            Debug.LogWarning("[DFF] Start: No onEnterEventReceivers set.");
        }
        else
        {
            Debug.Log("[DFF] Start: Some onEnterEventReceivers found.");
            onEnterEventReceivers_UdonBehaviours = new UdonBehaviour[onEnterEventReceivers.Length];
            for (int i = 0; i < onEnterEventReceivers.Length; i++)
            {
                onEnterEventReceivers_UdonBehaviours[i] = onEnterEventReceivers[i].GetComponent<UdonBehaviour>();
                if (onEnterEventReceivers_UdonBehaviours[i] == null)
                {
                    Debug.LogWarning("[DFF] Start: No UdonBehaviour found on " + onEnterEventReceivers[i].name);
                }
            }
        }
        if (onExitEventReceivers == null || onExitEventReceivers.Length == 0)
        {
            Debug.LogWarning("[DFF] Start: No onExitEventReceivers set.");
        }
        else
        {
            Debug.Log("[DFF] Start: Some onExitEventReceivers found.");
            onExitEventReceivers_UdonBehaviours = new UdonBehaviour[onExitEventReceivers.Length];
            for (int i = 0; i < onExitEventReceivers.Length; i++)
            {
                onExitEventReceivers_UdonBehaviours[i] = onExitEventReceivers[i].GetComponent<UdonBehaviour>();
                if (onExitEventReceivers_UdonBehaviours[i] == null)
                {
                    Debug.LogWarning("[DFF] Start: No UdonBehaviour found on " + onExitEventReceivers[i].name);
                }
            }
        }
    }


    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (player == null || player != localPlayer) return;
        isInsideBox = true;
    }


    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        if (player == null || player != localPlayer) return;
        isInsideBox = false;
    }


    // For ClientSim
    public void Test_DroneEnter_EventSender()
    {
        if (onEnterEventReceivers == null) return;
        if (onEnterEventReceivers.Length == 0) return;
        foreach (UdonBehaviour udonBehaviour in onEnterEventReceivers_UdonBehaviours)
        {
            if (udonBehaviour == null) continue;
            Debug.Log("[DFF] Sending OnDroneForcefieldEnter to " + udonBehaviour.name);
            udonBehaviour.SendCustomEvent("OnDroneForcefieldEnter");
        }
    }


    public override void OnDroneTriggerEnter(VRCDroneApi drone)
    {
        if (localDrone == null) return;
        if (drone != localDrone) return;
        Debug.Log("[DFF] Drone entered forcefield " + gameObject.name);
        if (ignoreIfPlayerInsideTrigger && isInsideBox) return;
        if (ignorePlayer) return;

        if (pushOnEnter && !shouldTeleportInsteadOfPush)
        {
            PushDrone(drone);
        }
        else if (teleportOnEnter && shouldTeleportInsteadOfPush)
        {
            TeleportDrone(drone);
        }

        if (onEnterEventReceivers == null) return;
        if (onEnterEventReceivers.Length == 0) return;
        foreach (GameObject obj in onEnterEventReceivers)
        {
            UdonBehaviour udonBehaviour = obj.GetComponent<UdonBehaviour>();
            if (udonBehaviour == null) continue;
            Debug.Log("[DFF] Sending OnDroneForcefieldEnter to " + udonBehaviour.name);
            udonBehaviour.SendCustomEvent("OnDroneForcefieldEnter");
        }
    }


    public override void OnDroneTriggerExit(VRCDroneApi drone)
    {
        if (localDrone == null) return;
        if (drone != localDrone) return;
        Debug.Log("[DFF] Drone exited forcefield " + gameObject.name);
        if (ignoreIfPlayerInsideTrigger && isInsideBox) return;
        if (ignorePlayer) return;

        if (pushOnExit && !shouldTeleportInsteadOfPush)
        {
            PushDrone(drone);
        }
        else if (teleportOnExit && shouldTeleportInsteadOfPush)
        {
            TeleportDrone(drone);
        }

        if (onExitEventReceivers == null) return;
        if (onExitEventReceivers.Length == 0) return;
        foreach (GameObject obj in onExitEventReceivers)
        {
            UdonBehaviour udonBehaviour = obj.GetComponent<UdonBehaviour>();
            if (udonBehaviour == null) continue;
            Debug.Log("[DFF] Sending OnDroneForcefieldExit to " + udonBehaviour.name);
            udonBehaviour.SendCustomEvent("OnDroneForcefieldExit");
        }
    }


    public override void OnDroneTriggerStay(VRCDroneApi drone)
    {
        if (pushOnStay && !shouldTeleportInsteadOfPush)
        {
            PushDrone(drone);
        } 
        else if (teleportOnStay && shouldTeleportInsteadOfPush)
        {
            TeleportDrone(drone);
        }
    }


    private void TeleportDrone(VRCDroneApi drone)
    {
        if (teleportDestination == null) return;
        if (resetVelocityOnTeleport)
        {
            drone.SetVelocity(Vector3.zero);
        }
        Quaternion droneRotation;
        if (keepDroneRotation && drone.TryGetRotation(out droneRotation))
        {
            drone.TeleportTo(teleportDestination.position, droneRotation);
            return;
        }
        else if (keepDroneRotation)
        {
            Debug.LogWarning("[DFF] TP: Could not get drone rotation.");
        }
        drone.TeleportTo(teleportDestination.position, teleportDestination.rotation);
    }


    private void PushDrone(VRCDroneApi drone) {
        if (localDrone == null || drone != localDrone) return;
        if (isInsideBox && ignoreIfPlayerInsideTrigger) return;
        if (ignorePlayer) return;

        Vector3 dronePosition;
        if (drone.TryGetPosition(out dronePosition))
        {
            Vector3 pushDirection;
            if (shouldPushFromPoint)
            {
                pushDirection = pushFromTransform != null ? pushFromTransform.position - dronePosition : center.position - dronePosition;
                pushDirection = -pushDirection.normalized;
            }
            else
            {
                pushDirection = this.pushDirection;
                if (pushDirectionInLocalSpace)
                {
                    pushDirection = transform.TransformDirection(pushDirection);
                }
            }
            
            Vector3 droneVelocity;
            if (drone.TryGetVelocity(out droneVelocity))
            {
                Vector3 finalPushStrength = pushDirection * dronePushStrength;
                if (!overrideVelocity)
                {
                    finalPushStrength = droneVelocity + finalPushStrength * 10f * Time.fixedDeltaTime;
                }
                drone.SetVelocity( finalPushStrength );
            }
            else
            {
                Debug.LogWarning("[DFF] Push: Could not get drone velocity.");
            }
        }
        else
        {
            Debug.LogWarning("[DFF] Push: Could not get drone position.");
        }
    }


#if UNITY_EDITOR && !COMPILER_UDONSHARP
    private void OnDrawGizmosSelected() {
        // initialize the gizmo matrix to the object's transform
        // Matrix4x4 originalGizmoMatrix = Gizmos.matrix;
        // Gizmos.matrix = transform.localToWorldMatrix;
        //     Gizmos.color = Color.yellow;

        // Collider collider = GetComponent<Collider>();
        // BoxCollider boxCollider = collider as BoxCollider;
        // if (boxCollider != null)
        // {
        //     Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
        // }
        // SphereCollider sphereCollider = collider as SphereCollider;
        // if (sphereCollider != null)
        // {
        //     Gizmos.DrawWireSphere(sphereCollider.center, sphereCollider.radius);
        // }
        // CapsuleCollider capsuleCollider = collider as CapsuleCollider;
        // if (capsuleCollider != null)
        // {
        //     GizmosUtil.DrawWireCapsule(capsuleCollider.center, capsuleCollider.height, capsuleCollider.radius);
        // }
        // Gizmos.matrix = originalGizmoMatrix;

        // draw push-from point
        Vector3 pushFromPoint = pushFromTransform != null ? pushFromTransform.position : transform.position;
        Gizmos.color = Color.grey;
        GizmosUtil.DrawWireSphere(pushFromPoint, 0.1f);
        if (!shouldPushFromPoint && !shouldTeleportInsteadOfPush)
        {
            Vector3 pushDirection = pushDirectionInLocalSpace ? transform.TransformDirection(this.pushDirection) : this.pushDirection;
            Gizmos.color = Color.red;
            GizmosUtil.DrawArrow(transform.position, transform.position + Vector3.Normalize(pushDirection) * 2f, 0.5f);
            Gizmos.color = new Color(1f, 0.5f, 0f, 1f);
            Gizmos.DrawLine(transform.position, transform.position + pushDirection * dronePushStrength );
        }

        // draw teleport destination
        if (teleportDestination != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(teleportDestination.position, 0.1f);
            Gizmos.DrawLine(transform.position, teleportDestination.position);
            Gizmos.color = Color.blue;
            GizmosUtil.DrawArrow(teleportDestination.position, teleportDestination.position + teleportDestination.forward * 0.5f, 0.2f);
        }
    }
#endif
}
