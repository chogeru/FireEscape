using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 電気スイッチ。登録したLightをまとめてON/OFFする。
/// </summary>
public class LightSwitchInteractable : InteractableBase
{
    [SerializeField] private List<Light> lights = new();
    [SerializeField] private bool startOn = true;

    private bool isOn;

    private void Reset()
    {
        oneShot = false;
        gazeDuration = 0.4f;
    }

    private void Start()
    {
        isOn = startOn;
        Apply();
    }

    public override void Interact(GameObject interactor)
    {
        isOn = !isOn;
        Apply();
        base.Interact(interactor);
    }

    private void Apply()
    {
        foreach (var light in lights)
        {
            if (light != null)
                light.enabled = isOn;
        }
    }
}
