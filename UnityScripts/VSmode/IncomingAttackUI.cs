using UnityEngine;

public class IncomingAttackUI : MonoBehaviour
{
    public Transform iconsContainer;
    public GameObject iconPrefab;

    public void SetCount(int count)
    {
        if (iconsContainer == null || iconPrefab == null) return;

        for (int i = iconsContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(iconsContainer.GetChild(i).gameObject);
        }

        for (int i = 0; i < count; i++)
        {
            Instantiate(iconPrefab, iconsContainer);
        }
    }
}