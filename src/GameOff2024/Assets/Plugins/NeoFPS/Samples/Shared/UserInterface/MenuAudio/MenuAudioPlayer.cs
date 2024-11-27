using FMOD.Studio;
using FMODUnity;
using System;
using UnityEngine;

namespace NeoFPS.Samples
{
    [HelpURL("http://docs.neofps.com/manual/samples-ui.html")]
    [RequireComponent (typeof (RectTransform))]
	[RequireComponent (typeof (AudioSource))]
	public class MenuAudioPlayer : MonoBehaviour
	{
		public void PlayClip (EventReference clip)
		{
            EventInstance clipInstance = RuntimeManager.CreateInstance(clip);
            if(clipInstance.isValid())
            {
                clipInstance.start();
                clipInstance.release();
            }           
        }
	}
}

