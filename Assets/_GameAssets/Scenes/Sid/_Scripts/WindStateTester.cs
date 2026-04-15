using UnityEngine;
using FMODUnity;

public class WindStateTest : MonoBehaviour
{
    [SerializeField] private StudioEventEmitter emitter;
    [SerializeField] private string parameterName = "WindChaseState";

    private float timer = 0f;
    private int counter = 0;


    public enum EnumWindMixer { Indoor = 0, Outdoor = 1 }
    private void Awake()
    {
        if (emitter == null)
            emitter = GetComponent<StudioEventEmitter>();
    }



    private void Start()
    {
        if (emitter != null && !emitter.IsPlaying())
        {
            emitter.Play();
        }
    }

    [ContextMenu("Play Indoor")]
    public void PlayIndoor()
    {
        emitter.SetParameter(parameterName, (int) EnumWindMixer.Indoor);
    }

    [ContextMenu("Play Outdoor")]
    public void PlayOutdoor()
    {
        emitter.SetParameter(parameterName, (int)EnumWindMixer.Outdoor);
    }


    //private void Update()
    //{
    //    if (emitter == null)
    //        return;

    //    timer += Time.deltaTime;

    //    if (timer >= 1f)
    //    {
    //        timer = 0f;
    //        counter++;

    //        Debug.Log("Counter: " + counter);

    //        if (counter == 10)
    //        {
    //            emitter.SetParameter(parameterName, 1f); // Outdoor
    //            Debug.Log("WindChaseState = Outdoor");
    //        }

    //        if (counter == 20)
    //        {
    //            emitter.SetParameter(parameterName, 2f); // Chase
    //            Debug.Log("WindChaseState = Chase");
    //        }
    //    }
    //}
}