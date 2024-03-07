using Unity.Netcode;

public class IgnoreForOwner : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) gameObject.SetActive(false);
        base.OnNetworkSpawn();
    }
}