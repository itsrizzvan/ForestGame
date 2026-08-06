using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnemyAnimationController : MonoBehaviour
{
    private Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
        anim.speed = Random.Range(0.95f, 1.1f);
    }

    public void SetRunning(bool isRunning)
    {
        anim.SetBool("IsRunning", isRunning);
        if (isRunning) anim.SetInteger("RunIndex", Random.Range(1, 3));
    }

    public void SetDetected(bool isDetected)
    {
        anim.SetBool("IsDetected", isDetected);
        if (isDetected) anim.SetInteger("DetectIndex", Random.Range(1, 3));
    }

    public void PlayIdle()
    {
        anim.SetBool("IsRunning", false);
        anim.SetBool("IsDetected", false);
        anim.SetInteger("IdleIndex", Random.Range(1, 3));
    }

    public void TriggerAttack()
    {
        anim.SetBool("IsRunning", false);
        anim.SetTrigger("Attack" + Random.Range(1, 3));
    }
}