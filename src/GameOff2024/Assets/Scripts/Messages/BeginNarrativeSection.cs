
public enum NarrativeSection
{
    Intro = 0,
    CaughtSpy = 10,
    CarriedBriefcase = 20,
    CaughtHandler = 30
}

public class BeginNarrativeSection
{
    public NarrativeSection Section { get; }

    public BeginNarrativeSection(NarrativeSection n) => Section = n;
}
