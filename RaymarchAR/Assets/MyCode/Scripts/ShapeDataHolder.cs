using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class ShapeDataHolder : MonoBehaviour
{

    List<RayShape> shapes;

    void Start()
    {
        shapes = new List<RayShape>();
    }

    public void GetAllTrackables()
    {
        IEnumerable<TrackableBehaviour> trackables = TrackerManager.Instance.GetStateManager().GetTrackableBehaviours();
        foreach(TrackableBehaviour behaviour in trackables)
        {
            shapes.Add(behaviour.GetComponentInChildren<RayShape>());
        }
    }
}
