using UnityEngine;
using UnityEditor;
using VRC.SDKBase.Editor.BuildPipeline;
using System.Linq;

[CustomEditor(typeof(Drone_Forcefield))]
[CanEditMultipleObjects]
public class Drone_ForcefieldEditor : Editor
{
    private GUIStyle TitleHeaderStyle = new GUIStyle();
    private GUIStyle gizmoErrorStyle = new GUIStyle();

    private bool shouldShowDefaultInspector = false;

    private void OnEnable()
    {
        TitleHeaderStyle = new GUIStyle();
        TitleHeaderStyle.fontStyle = FontStyle.Bold;
        TitleHeaderStyle.fontSize = 16;
        TitleHeaderStyle.alignment = TextAnchor.MiddleCenter;
        TitleHeaderStyle.normal.textColor = Color.white;

        gizmoErrorStyle.normal.textColor = Color.red;
        gizmoErrorStyle.alignment = TextAnchor.MiddleCenter;
        gizmoErrorStyle.fontSize = 12;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        Drone_Forcefield behaviour = (Drone_Forcefield)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Drone Force Field Settings", TitleHeaderStyle);
        EditorGUILayout.Space(10);
        EditorGUILayout.HelpBox("Drones will be pushed or teleported while inside this object's trigger.\nMouse over fields for more info.", MessageType.Info);
        EditorGUILayout.Space(10);


        Collider collider = behaviour.GetComponent<Collider>();
        if (collider == null) {
            EditorGUILayout.HelpBox("No Collider found on the object. Please add a collider marked as \"Trigger\" to the object for the script to work.", MessageType.Error);
            if (GUILayout.Button("Add BoxCollider"))
            {
                collider = behaviour.gameObject.AddComponent<BoxCollider>();
                collider.isTrigger = true;
            }
            EditorGUILayout.Space(20);
        }
        else if (!collider.isTrigger) {
            EditorGUILayout.HelpBox("Collider is not marked as a trigger. Please mark the collider as a trigger for the script to work.", MessageType.Error);
            if (GUILayout.Button("Mark as Trigger"))
            {
                if (collider is MeshCollider)
                {
                    ((MeshCollider)collider).convex = true;
                }
                collider.isTrigger = true;
            }
            EditorGUILayout.Space(20);
        }


        // Get SerializedProperties for teleport settings
        SerializedProperty shouldTeleportInsteadOfPushProp = serializedObject.FindProperty("shouldTeleportInsteadOfPush");
        SerializedProperty teleportDestinationProp = serializedObject.FindProperty("teleportDestination");
        SerializedProperty keepDroneRotationProp = serializedObject.FindProperty("keepDroneRotation");
        SerializedProperty resetVelocityOnTeleportProp = serializedObject.FindProperty("resetVelocityOnTeleport");
        SerializedProperty teleportOnEnterProp = serializedObject.FindProperty("teleportOnEnter");
        SerializedProperty teleportOnExitProp = serializedObject.FindProperty("teleportOnExit");
        SerializedProperty teleportOnStayProp = serializedObject.FindProperty("teleportOnStay");

        // Get SerializedProperties for push settings
        SerializedProperty dronePushStrengthProp = serializedObject.FindProperty("dronePushStrength");
        SerializedProperty shouldPushFromPointProp = serializedObject.FindProperty("shouldPushFromPoint");
        SerializedProperty pushFromTransformProp = serializedObject.FindProperty("pushFromTransform");
        SerializedProperty pushDirectionProp = serializedObject.FindProperty("pushDirection");
        SerializedProperty pushRotationProp = serializedObject.FindProperty("pushRotation");
        SerializedProperty pushDirectionInLocalSpaceProp = serializedObject.FindProperty("pushDirectionInLocalSpace");
        SerializedProperty overrideVelocityProp = serializedObject.FindProperty("overrideVelocity");
        SerializedProperty pushOnEnterProp = serializedObject.FindProperty("pushOnEnter");
        SerializedProperty pushOnExitProp = serializedObject.FindProperty("pushOnExit");
        SerializedProperty pushOnStayProp = serializedObject.FindProperty("pushOnStay");

        // Get SerializedProperty for misc settings
        SerializedProperty ignoreIfPlayerInsideTriggerProp = serializedObject.FindProperty("ignoreIfPlayerInsideTrigger");
        SerializedProperty ignorePlayerProp = serializedObject.FindProperty("ignorePlayer");
        SerializedProperty onEnterEventReceiversProp = serializedObject.FindProperty("onEnterEventReceivers");
        SerializedProperty onExitEventReceiversProp = serializedObject.FindProperty("onExitEventReceivers");
 

        EditorGUILayout.PropertyField(shouldTeleportInsteadOfPushProp, new GUIContent("Teleport Instead Of Push", "If true, the drone will be teleported to the teleport location instead of being pushed."));
        EditorGUILayout.Space();


        if (serializedObject.targetObjects.Any(obj => ((Drone_Forcefield)obj).shouldTeleportInsteadOfPush))
        {
            EditorGUILayout.LabelField("Drone Teleport Settings", EditorStyles.boldLabel);

            if (teleportDestinationProp.objectReferenceValue == null && targets.Length == 1)
            {
            EditorGUILayout.HelpBox("No teleport destination set.", MessageType.Error);
            }
            EditorGUILayout.PropertyField(teleportDestinationProp, new GUIContent("Teleport Destination", "The point to which the drone will be teleported."));
            EditorGUILayout.PropertyField(keepDroneRotationProp, new GUIContent("Keep Drone Rotation", "If true, the drone will keep its rotation when being teleported."));
            EditorGUILayout.PropertyField(resetVelocityOnTeleportProp, new GUIContent("Reset Velocity On Teleport", "If true, the drone's velocity will be reset to zero when being teleported."));
            EditorGUILayout.Space();

            // Teleport on enter/exit/stay settings
            EditorGUILayout.LabelField("Teleport the drone upon:", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            float defaultLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 40;
            
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = teleportOnEnterProp.hasMultipleDifferentValues;
            bool newTeleportOnEnter = EditorGUILayout.Toggle(new GUIContent("Enter", "If true, the drone will be teleported when it enters the trigger."), teleportOnEnterProp.boolValue, GUILayout.Width(100));
            if (EditorGUI.EndChangeCheck())
                teleportOnEnterProp.boolValue = newTeleportOnEnter;
            
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = teleportOnExitProp.hasMultipleDifferentValues;
            bool newTeleportOnExit = EditorGUILayout.Toggle(new GUIContent("Exit", "If true, the drone will be teleported when it exits the trigger."), teleportOnExitProp.boolValue, GUILayout.Width(100));
            if (EditorGUI.EndChangeCheck())
                teleportOnExitProp.boolValue = newTeleportOnExit;
            
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = teleportOnStayProp.hasMultipleDifferentValues;
            bool newTeleportOnStay = EditorGUILayout.Toggle(new GUIContent("Stay", "If true, the drone will be teleported while it stays inside the trigger."), teleportOnStayProp.boolValue, GUILayout.Width(100));
            if (EditorGUI.EndChangeCheck())
                teleportOnStayProp.boolValue = newTeleportOnStay;
            
            EditorGUI.showMixedValue = false;
            EditorGUIUtility.labelWidth = defaultLabelWidth;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(15);
        }


        if (serializedObject.targetObjects.Any(obj => !((Drone_Forcefield)obj).shouldTeleportInsteadOfPush))
        {
            EditorGUILayout.LabelField("Drone Push Settings", EditorStyles.boldLabel);

            float pushStrength = dronePushStrengthProp.floatValue;
            float sliderValue = EditorGUILayout.Slider("Push Strength", pushStrength, -100f, 100f);
            if (sliderValue != pushStrength)
            {
                pushStrength = sliderValue;
            }
            // EditorGUILayout.BeginHorizontal();
            // EditorGUILayout.LabelField("Push Strength", "Slider override, in case you need more force", EditorStyles.miniLabel);
            // pushStrength = EditorGUILayout.FloatField(pushStrength, GUILayout.MaxWidth(50));
            // EditorGUILayout.EndHorizontal();    
            dronePushStrengthProp.floatValue = pushStrength;
            // EditorGUILayout.PropertyField(dronePushStrengthProp, new GUIContent("Push Strength", "The strength of the push force applied to the drone. Negative values indicate a pull strength instead."));
            
            EditorGUILayout.PropertyField(shouldPushFromPointProp, new GUIContent("Push Away From a Point?", "If true, the drone will be pushed away from the specified point. If false, the drone will be pushed in a constant direction."));
            
            if (shouldPushFromPointProp.boolValue)
            {
                EditorGUILayout.PropertyField(pushFromTransformProp, new GUIContent("Push From This Point:", "The point from which the drone will be pushed away from. If unset, defaults to the origin of this object."));
            }

            else
            {
                // Display the push rotation as euler angles
                Quaternion currentRotation = pushRotationProp.quaternionValue;
                Vector3 currentEuler = currentRotation.eulerAngles;
                Vector3 editedEuler = EditorGUILayout.Vector3Field(new GUIContent("Push in This Direction", "The direction in which the drone will be pushed away from the specified point."), currentEuler);
                if (editedEuler != currentEuler)
                {
                    Quaternion newRotation = Quaternion.Euler(editedEuler);
                    pushRotationProp.quaternionValue = newRotation;
                    pushDirectionProp.vector3Value = newRotation * Vector3.forward;
                }
                Vector3 pd = pushDirectionProp.vector3Value;
                if (pd.sqrMagnitude > 1.05f || pd.sqrMagnitude < 0.95f)
                {
                    EditorGUILayout.HelpBox("Push direction is not normalized.", MessageType.Warning);
                    if (GUILayout.Button("Normalize"))
                    {
                        pushDirectionProp.vector3Value = pd.normalized;
                    }
                }
                
                EditorGUILayout.PropertyField(pushDirectionInLocalSpaceProp, new GUIContent("Push Relative to Object's Orientation", "If true, the push direction will be relative to the object instead of the world."));
            }
            EditorGUILayout.PropertyField(overrideVelocityProp, new GUIContent("Override Drone Velocity", "If true, the drone's velocity will be overridden instead of being pushed."));

            EditorGUILayout.Space();

            // Push on enter/exit/stay settings
            EditorGUILayout.LabelField("Push the drone upon:", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            float defaultLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 40;
            
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = pushOnEnterProp.hasMultipleDifferentValues;
            bool newPushOnEnter = EditorGUILayout.Toggle(new GUIContent("Enter", "If true, the drone will be pushed when it enters the trigger."), pushOnEnterProp.boolValue, GUILayout.Width(100));
            if (EditorGUI.EndChangeCheck())
                pushOnEnterProp.boolValue = newPushOnEnter;
            
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = pushOnExitProp.hasMultipleDifferentValues;
            bool newPushOnExit = EditorGUILayout.Toggle(new GUIContent("Exit", "If true, the drone will be pushed when it exits the trigger."), pushOnExitProp.boolValue, GUILayout.Width(100));
            if (EditorGUI.EndChangeCheck())
                pushOnExitProp.boolValue = newPushOnExit;
            
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = pushOnStayProp.hasMultipleDifferentValues;
            bool newPushOnStay = EditorGUILayout.Toggle(new GUIContent("Stay", "If true, the drone will be pushed while it stays inside the trigger."), pushOnStayProp.boolValue, GUILayout.Width(100));
            if (EditorGUI.EndChangeCheck())
                pushOnStayProp.boolValue = newPushOnStay;
            
            EditorGUI.showMixedValue = false;
            EditorGUIUtility.labelWidth = defaultLabelWidth;
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.Space(15);
        }


        EditorGUILayout.LabelField("Misc Settings", EditorStyles.boldLabel);
        
        EditorGUILayout.PropertyField(ignoreIfPlayerInsideTriggerProp, new GUIContent("Ignore If Player is Inside Trigger", "If true, the drone will not be affected if the player controlling it is inside of this object's trigger."));
        
        EditorGUILayout.PropertyField(ignorePlayerProp, new GUIContent("Always Ignore Local Player", "If true, the drone will not be affected no matter what. This can be set from other scripts, for example to disable the forcefield for users with certain permissions, users on certain teams, etc."));
        
        EditorGUILayout.PropertyField(onEnterEventReceiversProp, new GUIContent("Drone Enter Event Receivers", "Add UdonBehaviours here where you want a custom event to be sent when a drone enters the trigger. Will send a custom event called OnDroneForcefieldEnter."));
       
        EditorGUILayout.PropertyField(onExitEventReceiversProp, new GUIContent("Drone Exit Event Receivers", "Add UdonBehaviours here where you want a custom event to be sent when a drone exits the trigger. Will send a custom event called OnDroneForcefieldExit."));
        
        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Space(20);


        shouldShowDefaultInspector = EditorGUILayout.BeginFoldoutHeaderGroup(shouldShowDefaultInspector, "(Debug) Show Default Inspector");
        if (shouldShowDefaultInspector) 
        {
            base.OnInspectorGUI();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();


        if (GUI.changed)
        {
            serializedObject.ApplyModifiedProperties();
            SceneView.RepaintAll();
        }
    }

    private void OnSceneGUI() {
        Drone_Forcefield behaviour = (Drone_Forcefield)target;

        Transform transform = behaviour.transform;
        Vector3 handlePosition = transform.position;
        Quaternion handleRotation = behaviour.pushRotation;
        

        if (behaviour.shouldTeleportInsteadOfPush && behaviour.teleportDestination == null) 
        {
            Handles.Label(transform.position, "Teleport Destination is not set!", gizmoErrorStyle);
        }

        Collider[] colliders = behaviour.GetComponents<Collider>();
        if (colliders == null || colliders.Length == 0) 
        {
            Handles.Label(transform.position + Vector3.up, "No Collider found on the object!", gizmoErrorStyle);
        }

        if (!behaviour.shouldPushFromPoint && behaviour.pushDirection == Vector3.zero) 
        {
            Debug.LogWarning("[DFF] Push direction is zero. Resetting.");
            Undo.RecordObject(behaviour, "Fix Drone Push Direction");
            behaviour.pushDirection = Vector3.forward;
            behaviour.pushRotation = Quaternion.LookRotation(Vector3.forward);
            EditorUtility.SetDirty(behaviour);
        }

        if (!behaviour.shouldPushFromPoint && !behaviour.shouldTeleportInsteadOfPush 
            && Tools.current != Tool.Rotate && targets.Length == 1)
        {
            if (behaviour.pushDirectionInLocalSpace)
            {
                // Convert the world rotation into the object's local space
                Quaternion localRotation = Quaternion.Inverse(transform.rotation) * behaviour.pushRotation;
                Quaternion newLocalRotation = Handles.RotationHandle(localRotation, handlePosition);
                if (localRotation != newLocalRotation)
                {
                    Undo.RecordObject(behaviour, "Change Drone Push Direction");
                    // Convert the modified local rotation back to world space
                    Quaternion newWorldRotation = transform.rotation * newLocalRotation;
                    behaviour.pushDirection = newWorldRotation * Vector3.forward;
                    behaviour.pushRotation = newWorldRotation;
                    EditorUtility.SetDirty(behaviour);
                }
            }
            else
            {
                Quaternion newRotation = Handles.RotationHandle(handleRotation, handlePosition);
                if (handleRotation != newRotation)
                {
                    Undo.RecordObject(behaviour, "Change Drone Push Direction");
                    behaviour.pushDirection = newRotation * Vector3.forward;
                    behaviour.pushRotation = newRotation;
                    EditorUtility.SetDirty(behaviour);
                }
            }
        }
    }

    public static bool DoPreProcess()
    {
        Debug.Log("[DFF] Preprocessing Drone_Forcefield objects.");
        Drone_Forcefield[] ffs = FindObjectsOfType<Drone_Forcefield>();
        foreach (Drone_Forcefield ff in ffs)
        {
            Collider[] colliders = ff.GetComponents<Collider>();
            if (colliders == null || colliders.Length == 0)
            {
                Debug.LogError("[DFF] No Colliders found on " + ff.name + ". Disabling forcefield.");
                ff.enabled = false;
                EditorUtility.SetDirty(ff);
            }

            if (ff.shouldTeleportInsteadOfPush && ff.teleportDestination == null)
            {
                Debug.LogError("[DFF] Teleport destination not set on " + ff.name + ". Disabling teleport.");
                ff.shouldTeleportInsteadOfPush = false;
                EditorUtility.SetDirty(ff);
            }

            if (ff.pushDirection == Vector3.zero)
            {
                Debug.LogWarning("[DFF] Push direction is zero on " + ff.name + ". Resetting.");
                ff.pushDirection = Vector3.forward;
                EditorUtility.SetDirty(ff);
            }

            Vector3 oldPd = ff.pushDirection;
            ff.pushDirection.Normalize();
            if (oldPd != ff.pushDirection) // If the push direction was not normalized (and got changed)
            {
                Debug.LogWarning("[DFF] Push direction was not normalized on " + ff.name + ". Normalizing.");
                EditorUtility.SetDirty(ff);
            }
        }
        Debug.Log("[DFF] Preprocessing completed successfully.");
        return true;
    }
}

public class Drone_ForcefieldEditorCallbacks : IVRCSDKBuildRequestedCallback
{
    public int callbackOrder => 0;

    public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
    {
        if (requestedBuildType == VRCSDKRequestedBuildType.Avatar)
        {
            return true;
        }
        return Drone_ForcefieldEditor.DoPreProcess();
    }
}