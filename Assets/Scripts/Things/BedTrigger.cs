using UnityEngine;

public class BedTrigger : MonoBehaviour
{
    public static bool IsPlayerInBedArea { get; private set; }

    // 床的躺卧位置
    public Transform bedSleepPoint; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            print("111111");
            IsPlayerInBedArea = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            print("222222");
            IsPlayerInBedArea = false;
        }
    }

    // Gizmos可视化躺床位置
    private void OnDrawGizmosSelected()
    {
        if (bedSleepPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(bedSleepPoint.position, 0.5f);
            Gizmos.DrawLine(bedSleepPoint.position, bedSleepPoint.position + bedSleepPoint.forward * 1f);
        }
    }
}