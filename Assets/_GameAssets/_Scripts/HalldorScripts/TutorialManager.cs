using System.Threading;
using UnityEngine;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    //Bools to see your progress through the night
    public bool hasLitFire = false;
    public bool hasEatenFood = false;
    public bool hasFixedRadio = false;

    //A refrence to the night settings
    [SerializeField] private SO_NightSettings nightSettings;

    //A refrence to the temperture manager
    [SerializeField] private PlayerTemperatureSimulator tempertureManager;

    //A refrence to the hunger manager
    [SerializeField] private HungerSystem hungerManager;

    //A refrence to the radio
    [SerializeField] private Radio radio;

    //A refrence to the tutorial UI text
    [SerializeField] private TMP_Text tutorialText;

    private CancellationTokenSource livingroomToken;
    private TriggerArea triggerArea;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (nightSettings != null)
        {
            if (nightSettings.DebugStartNight > 1)
            {
                Debug.Log("Tutorial deleted");
                tutorialText.text = "";
                Destroy(this.gameObject);
            }
            else
            {
                tutorialText.text = "Go into the livingroom";
                this.tempertureManager.SetIsSimulatorEnabled(false);
                this.hungerManager.Pause();
                GameManager.Instance.PauseNightTimer();
                GameManager.Instance.SetNightTimerRemainingSeconds(15f);
                Debug.Log("Tutorial started");

                this.triggerArea = this.GetComponentInChildren<TriggerArea>();
                if (this.triggerArea != null)
                    this.triggerArea.OnTriggerEntered += EnteredLivingRoom;
            }
        }
    }

    private void EnteredLivingRoom(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        Debug.Log("Tutorial Entered Living room");
        this.radio.SendBroadcast(Radio.RadioBroadcasts.IntroductionTutorial);
        StartBroadcastWait(0f, StartTutorial); // 57 seconds
    }

    private void OnEnable()
    {
        HungerSystem.HungerStateChangedEvent += OnHungerStateChanged;
        this.radio.OnChannelChanged += OnRadioChannelChanged;
        this.radio.OnBroadcastChanged += OnRadioBroadcastChanged;
    }

    private void OnDisable()
    {
        HungerSystem.HungerStateChangedEvent -= OnHungerStateChanged;
        this.radio.OnChannelChanged -= OnRadioChannelChanged;
        this.radio.OnBroadcastChanged -= OnRadioBroadcastChanged;
    }

    private void OnRadioChannelChanged(int channelIndex, bool isSafeChannel)
    {
        if (!isSafeChannel)
        {
            this.livingroomToken?.Cancel();
            return;
        }
        if (this.hasFixedRadio) return;
        this.radio.SendBroadcast(Radio.RadioBroadcasts.RadioTutorialTip);
        StartBroadcastWait(0f /* TODO: RadioTutorialTip duration */, () =>
        {
            hasFixedRadio = true;
            this.tutorialText.text = "Survive the night";
            GameManager.Instance.ResumeNightTimer();
        });
    }

    private void OnRadioBroadcastChanged(Radio.RadioBroadcasts broadcast)
    {
        if (broadcast == Radio.RadioBroadcasts.IntroductionTutorial) return;
        this.livingroomToken?.Cancel();
    }

    private void OnHungerStateChanged(HungerSystem.EnumHungerState oldState, HungerSystem.EnumHungerState newState)
    {
        if (this.hasEatenFood) return;
        if (newState != HungerSystem.EnumHungerState.Full) return;
        this.hasEatenFood = true;
        this.radio.SendBroadcast(Radio.RadioBroadcasts.FoodTutorialTip);
        Debug.Log("eaten food");
        StartBroadcastWait(0f /* TODO: FoodTutorialTip duration */, () =>
        {
            this.tutorialText.text = "Light the fireplace";
        });
    }

    [ContextMenu("Turn on fire")]
    public void TurnOnFire()
    {
        if (hasLitFire) return;
        this.hasLitFire = true;
        this.radio.SendBroadcast(Radio.RadioBroadcasts.FireTutorialTip);
        StartBroadcastWait(10f /* TODO: FireTutorialTip duration */, () =>
        {
            this.radio.SetChannel(8);
            this.tutorialText.text = "Put the radio frequency back to Channel 30";
        });
    }
    
    private void StartBroadcastWait(float duration, System.Action onFinished)
    {
        this.livingroomToken?.Cancel();
        this.livingroomToken?.Dispose();
        this.livingroomToken = new CancellationTokenSource();
        _ = WaitForBroadcastToFinish(duration, this.livingroomToken.Token, onFinished);
    }

    private async Awaitable WaitForBroadcastToFinish(float duration, CancellationToken ct, System.Action onFinished)
    {
        try { await Awaitable.WaitForSecondsAsync(duration, ct); }
        catch (System.OperationCanceledException) { }
        finally { onFinished?.Invoke(); }
    }

    private void StartTutorial()
    {
        if (this.triggerArea != null)
            this.triggerArea.OnTriggerEntered -= EnteredLivingRoom;
        tutorialText.text = "Eat a can of food";
        hungerManager.ModifyHunger(-20);
    }
}
