using System.Threading;
using UnityEngine;

public class HungerSoundTest : MonoBehaviour
{
    [SerializeField] private string soundEventName;
    [SerializeField] private bool useGlobal;

    private FMODUnity.StudioEventEmitter emitter;
    private CancellationTokenSource waitToken;

    private void Start()
    {
        this.emitter = GetComponent<FMODUnity.StudioEventEmitter>();
    }

    private void OnEnable()
    {
        HungerSystem.HungerStateChangedEvent += OnHungerStateChanged;
    }

    private void OnDisable()
    {
        HungerSystem.HungerStateChangedEvent -= OnHungerStateChanged;
        this.waitToken?.Cancel();
        this.waitToken?.Dispose();
    }

    private void OnHungerStateChanged(HungerSystem.EnumHungerState previous, HungerSystem.EnumHungerState current)
    {
        this.waitToken?.Cancel();
        this.waitToken?.Dispose();
        this.waitToken = new CancellationTokenSource();

        var value = (int)current;/*
        if (this.useGlobal)
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName(this.soundEventName, value);
        else
            emitter.SetParameter(this.soundEventName, value);*/

        float duration = GetHungerWaitDuration(current);
        _ = PlayHungerSounds(duration, this.waitToken.Token);
    }

    private float GetHungerWaitDuration(HungerSystem.EnumHungerState hungerState)
    {
        return hungerState switch
        {
            HungerSystem.EnumHungerState.Full => 10f,
            HungerSystem.EnumHungerState.SlightlyHungry => 7f,
            HungerSystem.EnumHungerState.Hungry => 5f,
            HungerSystem.EnumHungerState.Starving => 3f,
            _ => 10f
        };
    }
    
    private async Awaitable PlayHungerSounds(float duration, CancellationToken ct)
    {
        try
        {
            while (true)
            {
                await Awaitable.WaitForSecondsAsync(duration, ct);
                this.emitter.Play();
            }
        }
        catch (System.OperationCanceledException) { }
    }
}