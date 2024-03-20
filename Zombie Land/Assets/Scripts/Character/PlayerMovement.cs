using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float speed;

    [Space(5)] [SerializeField] private LayerMask aimLayerMask;

    [SerializeField] private Animator animator;

    private void Update()
    {
        AimTowardsMouse();

        var horizontal = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");

        var movement = new Vector3(horizontal, 0f, vertical);

        if (movement.magnitude > 0)
        {
            movement.Normalize();
            movement *= speed * Time.deltaTime;
            transform.Translate(movement, Space.World);
        }

        var velZ = Vector3.Dot(movement.normalized, transform.forward);
        var velX = Vector3.Dot(movement.normalized, transform.right);

        animator.SetFloat("VelocityZ", velZ, 0.1f, Time.deltaTime);
        animator.SetFloat("VelocityX", velX, 0.1f, Time.deltaTime);
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
            return;

        enabled = false;
        PlayerConnectedServerRpc();
    }

    [Rpc(SendTo.Server)]
    private void PlayerConnectedServerRpc()
    {
        var m = LevelManager.Default;
        var spawns = m.PlSpawnPoints;

        PlayerConnectedClientRpc(spawns[Random.Range(0, spawns.Length)].position);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PlayerConnectedClientRpc(Vector3 spawnPos)
    {
        if (IsOwner)
            return;

        gameObject.transform.position = spawnPos;
    }

    private void AimTowardsMouse()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, aimLayerMask))
        {
            var direction = hitInfo.point - transform.position;
            direction.y = 0f;
            direction.Normalize();
            transform.forward = direction;
        }
    }
}