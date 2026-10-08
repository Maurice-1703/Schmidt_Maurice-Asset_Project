using UnityEngine;

public class ModeActivator : MonoBehaviour
{
    [SerializeField] GameObject activeMode; 
    [SerializeField] GameObject inactiveMode;
    [SerializeField] bool startOn = false;

    void Start() => SetPowered(startOn);

    // Called by the toggle: true = on, false = off
    public void SetPowered(bool isOn)
    {
        activeMode.SetActive(isOn);
        inactiveMode.SetActive(!isOn); // inverted on purpose
    }
}