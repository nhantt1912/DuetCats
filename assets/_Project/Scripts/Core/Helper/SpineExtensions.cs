using System; 
using UnityEngine;
using Spine.Unity;

public static class SpineExtensions
{
    /// <summary>
    /// Plays an animation immediately and runs a callback when it finishes.
    /// </summary>
    public static void PlayAnimation(this SkeletonAnimation skeleton, string animName, bool loop = false, int trackIndex = 0,float mixDuration = 0.2f, Action onComplete = null)
    {
        if (skeleton == null || string.IsNullOrEmpty(animName)) return;
        
        // Play the animation and get its TrackEntry
        Spine.TrackEntry trackEntry = skeleton.AnimationState.SetAnimation(trackIndex, animName, loop);   
        trackEntry.MixDuration = mixDuration;

        // Attach the callback to the Complete event if one was provided
        if (onComplete != null)
        {
            trackEntry.Complete += delegate { onComplete.Invoke(); };
        }
    }
   
}