/*===============================================================================
Copyright (c) 2015-2018 PTC Inc. All Rights Reserved.

Copyright (c) 2015 Qualcomm Connected Experiences, Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
===============================================================================*/
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Vuforia;

public class TrackableSettings : MonoBehaviour
{
    #region PUBLIC_MEMBERS
    
    [HideInInspector]
    public bool m_DeviceTrackerEnabled = false;

    [HideInInspector]
    public FusionProviderType m_FusionProviderType = FusionProviderType.OPTIMIZE_MODEL_TARGETS_AND_SMART_TERRAIN;
    
    #endregion //PUBLIC_MEMBERS

    #region PRIVATE_MEMBERS
    PositionalDeviceTracker m_PositionalDeviceTracker;
    #endregion // PRIVATE_MEMBERS

    
    #region UNITY_MONOBEHAVIOUR_METHODS
    
    private void Awake()
    {
        VuforiaARController.Instance.RegisterBeforeVuforiaTrackersInitializedCallback(OnBeforeVuforiaTrackerInitialized);
        VuforiaARController.Instance.RegisterVuforiaInitializedCallback(OnVuforiaInitialized);
    }

    private void Start()
    {
        VuforiaARController.Instance.RegisterVuforiaStartedCallback(OnVuforiaStarted);
    }

    private void OnDestroy()
    {
        VuforiaARController.Instance.UnregisterBeforeVuforiaTrackersInitializedCallback(OnBeforeVuforiaTrackerInitialized);
        VuforiaARController.Instance.UnregisterVuforiaStartedCallback(OnVuforiaStarted);
        VuforiaARController.Instance.UnregisterVuforiaInitializedCallback(OnVuforiaInitialized);
    }
    
    #endregion // UNITY_MONOBEHAVIOUR_METHODS

    
    #region PRIVATE_METHODS

    private void OnBeforeVuforiaTrackerInitialized()
    {
        // set the selected fusion provider mask in the DeviceTrackerARController before it's being used.
        DeviceTrackerARController.Instance.FusionProvider = m_FusionProviderType;
    }

    private void OnVuforiaInitialized()
    {

        m_PositionalDeviceTracker = TrackerManager.Instance.GetTracker<PositionalDeviceTracker>();

        // if we don't have yet a positional device tracker, initialize one
        if (m_PositionalDeviceTracker == null)
        {
            m_PositionalDeviceTracker = TrackerManager.Instance.InitTracker<PositionalDeviceTracker>();

            if (m_PositionalDeviceTracker != null)
            {
                Debug.Log("Successfully initialized the positional device tracker");
            }
            else
            {
                Debug.LogError("Failed to initialize the positional device tracker");
            }        
        }
    }

    private void OnVuforiaStarted()
    {
        ToggleDeviceTracking(m_DeviceTrackerEnabled);
    }
    
    #endregion // PRIVATE_METHODS

    
    #region PUBLIC_METHODS
    
    public bool IsDeviceTrackingEnabled()
    {
        return m_DeviceTrackerEnabled;
    }

    public virtual void ToggleDeviceTracking(bool enableDeviceTracking)
    {
        if (m_PositionalDeviceTracker != null)
        {
            if (enableDeviceTracking)
            {
                // if the positional device tracker is not yet started, start it
                if (!m_PositionalDeviceTracker.IsActive)
                {
                    if (m_PositionalDeviceTracker.Start())
                    {
                        Debug.Log("Successfully started device tracker");
                    }
                    else
                    {
                        Debug.LogError("Failed to start device tracker");
                    }
                }
            }
            else if (m_PositionalDeviceTracker.IsActive)
            {
                m_PositionalDeviceTracker.Stop();

                Debug.Log("Successfully stopped device tracker");
            }
        }
        else
        {
            Debug.LogError("Failed to toggle device tracker state, make sure device tracker is initialized");
        }

        m_DeviceTrackerEnabled = m_PositionalDeviceTracker.IsActive;
    }

    public string GetActiveDatasetName()
    {
        ObjectTracker tracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
        List<DataSet> activeDataSets = tracker.GetActiveDataSets().ToList();
        if (activeDataSets.Count > 0)
        {
            string datasetPath = activeDataSets.ElementAt(0).Path;
            string datasetName = datasetPath.Substring(datasetPath.LastIndexOf("/") + 1);
            return datasetName.TrimEnd(".xml".ToCharArray());
        }
        else
        {
            return string.Empty;
        }
    }

    public void ActivateDataSet(string datasetName)
    {
        // ObjectTracker tracks ImageTargets contained in a DataSet and provides methods for creating and (de)activating datasets.
        ObjectTracker objectTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
        IEnumerable<DataSet> datasets = objectTracker.GetDataSets();

        IEnumerable<DataSet> activeDataSets = objectTracker.GetActiveDataSets();
        List<DataSet> activeDataSetsToBeRemoved = activeDataSets.ToList();

        // 1. Loop through all the active datasets and deactivate them.
        foreach (DataSet ads in activeDataSetsToBeRemoved)
        {
            objectTracker.DeactivateDataSet(ads);
        }

        // Swapping of the datasets should NOT be done while the ObjectTracker is running.
        // 2. So, Stop the tracker first.
        objectTracker.Stop();

        // 3. Then, look up the new dataset and if one exists, activate it.
        foreach (DataSet ds in datasets)
        {
            if (ds.Path.Contains(datasetName))
            {
                objectTracker.ActivateDataSet(ds);
            }
        }

        // 4. Finally, restart the object tracker and reset the device tracker.
        objectTracker.Start();

        if (m_PositionalDeviceTracker != null)
        {
            m_PositionalDeviceTracker.Reset();
        }
        else
        {
            Debug.LogError("Failed to reset device tracker");
        }
    }
    #endregion //PUBLIC_METHODS
}
