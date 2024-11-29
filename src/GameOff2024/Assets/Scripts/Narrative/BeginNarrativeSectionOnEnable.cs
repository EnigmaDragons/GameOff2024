using UnityEngine;

public class BeginNarrativeSectionOnEnable : MonoBehaviour
{
    [SerializeField] private NarrativeSection section;

    private void OnEnable()
    {
        Trigger();
    }

    public void Trigger()
    {
        Message.Publish(new BeginNarrativeSection(section));
        Debug.Log($"Published Narrative Section Started for {section}");
    }
}
