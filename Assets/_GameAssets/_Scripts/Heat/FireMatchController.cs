using UnityEngine;

public class FireMatchController : MonoBehaviour
{
    [SerializeField] private float despawnTimer;

    public void StartDespawnTimer()
    {
        _ = DespawnMatchAsync();
    }

    private async Awaitable DespawnMatchAsync()
    {
        Debug.Log("Start match despawn");
        await Awaitable.WaitForSecondsAsync(despawnTimer, destroyCancellationToken);
        Destroy(this.transform.parent.gameObject);
    }
}
