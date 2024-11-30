using System.Collections.Generic;
using FMODUnity;
using UnityEngine;
using Random = UnityEngine.Random;

public class DX_Chase_Lines : OnMessage<BeginNarrativeSection>
{
    [SerializeField] private EventReference[] lines;
    [SerializeField] private float minSecondsBetweenLines = 15f;
    [SerializeField] private float maxSecondsBetweenLines = 60f;
    [SerializeField] private NarrativeSection chaseSection;
    
    private bool isInChaseSection = false;
    private Queue<EventReference> lineQueue = new ();
    private float cooldownTimer = 0f;


    private void Awake()
    {
        Debug.Log($"DX Lines for {chaseSection} - Awake");
    }

    protected override void Execute(BeginNarrativeSection msg)
    {
        Debug.Log($"Received message with section: {msg.Section}");
        if (msg.Section == chaseSection)
        {
            Debug.Log("Entering chase section.");
            isInChaseSection = true;
            lineQueue.Clear();
            List<EventReference> shuffledLines = new List<EventReference>(lines);
            for (int i = 0; i < shuffledLines.Count; i++)
            {
                int randomIndex = Random.Range(i, shuffledLines.Count);
                (shuffledLines[i], shuffledLines[randomIndex]) = (shuffledLines[randomIndex], shuffledLines[i]);
            }
            foreach (var line in shuffledLines)
            {
                lineQueue.Enqueue(line);
            }
            Debug.Log($"Line queue populated with {lineQueue.Count} lines.");
            SetNewCooldown();
        }
        else
        {
            Debug.Log($"Exiting chase section - {chaseSection}");
            isInChaseSection = false;
            lineQueue.Clear();
        }
    }

    private void Update()
    {
        if (isInChaseSection && lineQueue.Count > 0)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                PlayNextLine();
                SetNewCooldown();
            }
        }
    }

    private void PlayNextLine()
    {
        if (lineQueue.Count > 0)
        {
            EventReference nextLine = lineQueue.Dequeue();
            Debug.Log($"Playing line: {nextLine}");
            RuntimeManager.PlayOneShot(nextLine, transform.position);
        }
    }

    private void SetNewCooldown()
    {
        cooldownTimer = Random.Range(minSecondsBetweenLines, maxSecondsBetweenLines);
        Debug.Log($"New cooldown set: {cooldownTimer} seconds.");
    }
}
