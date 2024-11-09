using UnityEngine;

public class Breadcrumb : MonoBehaviour
{
    public bool isDone = false;
    public float progressValue = 0f;
    
    public void SetProgressValue(float val)
    {
        progressValue = val;
    }

    public void MarkRejected()
    {
        isDone = true;
    }

    public void MarkDone()
    {
        isDone = true;
    }
}
