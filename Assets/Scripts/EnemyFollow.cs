using UnityEngine;

public class EnemyFollow : MonoBehaviour
{
    public Transform target; // el jugador
    public float speed = 2f;

    void Update()
    {
        if (target == null) return;

        // Mueve hacia el jugador en el eje X (y opcionalmente Y)
        Vector3 direction = target.position - transform.position;
        direction.y = 0; // eliminar esto si quieres que también siga verticalmente
        transform.position += direction.normalized * speed * Time.deltaTime;
    }
}
