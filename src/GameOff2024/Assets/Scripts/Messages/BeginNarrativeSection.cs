﻿
public enum NarrativeSection
{
    Intro = 0,
    IntroOpenEyes = 2,
    IntroPlayerFullControl = 3,
    IntroHalfwayThrough = 4,
    ChasingSpy = 5,
    CaughtSpy = 10,
    CarryingBriefcase = 15,
    CarriedBriefcase = 20,
    ChasingHandler = 25,
    CaughtHandler = 30,

    MainMenu = 100,
}

public class BeginNarrativeSection
{
    public NarrativeSection Section { get; }

    public BeginNarrativeSection(NarrativeSection n) => Section = n;
}
