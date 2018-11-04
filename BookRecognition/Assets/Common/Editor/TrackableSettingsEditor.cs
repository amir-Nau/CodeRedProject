/*===============================================================================
Copyright (c) 2018 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other 
countries.
===============================================================================*/
using UnityEditor;
using UnityEngine;
using Vuforia;

[CustomEditor(typeof(TrackableSettings))]
[CanEditMultipleObjects]
public class TrackableSettingsEditor : Editor
{
    private const string FUSION_TOOLTIP =
        "Select the right Vuforia Fusion mode for your use case.\n Supports 2 modes:\n" +
        " - Optimize for Model Targets and Ground Plane (Default)\n" +
        " - Optimize for Image Targets and VuMarks (as well as CylinderTargets, MultiTargets and  Object Targets).";
    
    private SerializedProperty m_DeviceTrackerEnabled;

    private SerializedProperty m_FusionProviderType;

    #region UNITY_EDITOR_METHODS

    void OnEnable()
    {
        m_DeviceTrackerEnabled = serializedObject.FindProperty("m_DeviceTrackerEnabled");
        m_FusionProviderType = serializedObject.FindProperty("m_FusionProviderType");
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        serializedObject.Update();

        m_DeviceTrackerEnabled.boolValue = EditorGUILayout.Toggle("Device Tracker Enabled", m_DeviceTrackerEnabled.boolValue);
        
        // draw a dropdown to select between the two most common fusion provider options
        int selectedIndex = 0; // default == FusionProviderType.ALL
                    
        if (m_FusionProviderType.intValue == (int)FusionProviderType.OPTIMIZE_IMAGE_TARGETS_AND_VUMARKS)
            selectedIndex = 1;
        
        int newIndex = EditorGUILayout.Popup(new GUIContent("Fusion Mode", FUSION_TOOLTIP), selectedIndex, 
            new[] {"Optimize for Model Targets and Ground Plane", "Optimize for Image Targets and VuMarks"});

        if (newIndex == 1)
            m_FusionProviderType.intValue = (int)FusionProviderType.OPTIMIZE_IMAGE_TARGETS_AND_VUMARKS;
        else
            m_FusionProviderType.intValue = (int)FusionProviderType.OPTIMIZE_MODEL_TARGETS_AND_SMART_TERRAIN;
        
        serializedObject.ApplyModifiedProperties();
    }

    #endregion // UNITY_EDITOR_METHODS
}
