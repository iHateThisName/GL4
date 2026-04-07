using UnityEngine;

/// <summary>
/// Runtime reference to the Radio component.
/// Radio sets Value in Awake, consumers (RadioSensor, DisruptRadioFrequencyState) read it.
/// </summary>
[CreateAssetMenu(menuName = "Runtime/Radio Reference")]
public class SO_RadioRef : SO_RuntimeRef<Radio> { }
