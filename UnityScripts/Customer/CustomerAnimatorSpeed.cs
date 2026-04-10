using UnityEngine;

public class CustomerAnimatorSpeed : MonoBehaviour
{
    public Animator animator;
    public float normalSpeed = 0.5f;
    public float urgentSpeed = 2.5f;

    public void SetUrgent(bool urgent)
    {
        if (animator == null)
        {
            Debug.LogWarning("CustomerAnimatorSpeed: Animator is not assigned!");
            return;
        }

        animator.speed = urgent ? urgentSpeed : normalSpeed;
        Debug.Log("Customer animation urgent = " + urgent + ", speed = " + animator.speed);
    }
}