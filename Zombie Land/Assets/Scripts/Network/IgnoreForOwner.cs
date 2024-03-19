using Unity.Netcode;

public class IgnoreForOwner : NetworkBehaviour // TODO: Костыль, ежжи. Организовать структуру игрока
{
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) gameObject.SetActive(false);
        base.OnNetworkSpawn();
    }
}