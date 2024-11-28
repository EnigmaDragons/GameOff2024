
public enum NarrativeSection
{
    Intro = 0,
    ChasingSpy = 5,
    CaughtSpy = 10,
    CarriedBriefcase = 20,
    ChasingHandler = 25,
    CaughtHandler = 30,
}

public class BeginNarrativeSection
{
    public NarrativeSection Section { get; }

    public BeginNarrativeSection(NarrativeSection n) => Section = n;
}
