﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ReliableOnTriggerExit : MonoBehaviour
{
    public delegate void _OnTriggerExit(Collider c);

    Collider thisCollider;
    bool ignoreNotifyTriggerExit = false;

    // Target callback
    Dictionary<GameObject, _OnTriggerExit> waitingForOnTriggerExit = new Dictionary<GameObject, _OnTriggerExit>();

    public static void NotifyTriggerEnter(Collider c, GameObject caller, _OnTriggerExit onTriggerExit)
    {
        ReliableOnTriggerExit thisComponent = null;
        ReliableOnTriggerExit[] ftncs = c.gameObject.GetComponents<ReliableOnTriggerExit>();
        foreach (ReliableOnTriggerExit ftnc in ftncs)
        {
            if (ftnc.thisCollider == c)
            {
                thisComponent = ftnc;
                break;
            }
        }
        if (thisComponent == null)
        {
            thisComponent = c.gameObject.AddComponent<ReliableOnTriggerExit>();
            thisComponent.thisCollider = c;
        }
        // Unity bug? (!!!!): Removing a Rigidbody while the collider is in contact will call OnTriggerEnter twice, so I need to check to make sure it isn't in the list twice
        // In addition, force a call to NotifyTriggerExit so the number of calls to OnTriggerEnter and OnTriggerExit match up
        if (thisComponent.waitingForOnTriggerExit.ContainsKey(caller) == false)
        {
            thisComponent.waitingForOnTriggerExit.Add(caller, onTriggerExit);
            thisComponent.enabled = true;
        }
        else
        {
            thisComponent.ignoreNotifyTriggerExit = true;
            thisComponent.waitingForOnTriggerExit[caller].Invoke(c);
            thisComponent.ignoreNotifyTriggerExit = false;
        }
    }

    public static void NotifyTriggerExit(Collider c, GameObject caller)
    {
        if (c == null)
            return;

        ReliableOnTriggerExit thisComponent = null;
        ReliableOnTriggerExit[] ftncs = c.gameObject.GetComponents<ReliableOnTriggerExit>();
        foreach (ReliableOnTriggerExit ftnc in ftncs)
        {
            if (ftnc.thisCollider == c)
            {
                thisComponent = ftnc;
                break;
            }
        }
        if (thisComponent != null && thisComponent.ignoreNotifyTriggerExit == false)
        {
            thisComponent.waitingForOnTriggerExit.Remove(caller);
            if (thisComponent.waitingForOnTriggerExit.Count == 0)
            {
                thisComponent.enabled = false;
            }
        }
    }
    private void OnDisable()
    {
        if (gameObject.activeSelf == false)
            CallCallbacks();
    }
    private void Update()
    {
        if (thisCollider == null)
        {
            // Will GetOnTriggerExit with null, but is better than no call at all
            CallCallbacks();

            Component.Destroy(this);
        }
        else if (thisCollider.enabled == false)
        {
            CallCallbacks();
        }
    }
    void CallCallbacks()
    {
        ignoreNotifyTriggerExit = true;
        foreach (var v in waitingForOnTriggerExit)
        {
            if (v.Key == null)
            {
                continue;
            }

            v.Value.Invoke(thisCollider);
        }
        ignoreNotifyTriggerExit = false;
        waitingForOnTriggerExit.Clear();
        enabled = false;
    }
}