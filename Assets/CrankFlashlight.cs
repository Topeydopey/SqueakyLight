using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script is for a flashlight that charges in "chunks" when you hold a key.
// If you stop holding, the flashlight won't fully charge that chunk. 
// Also, the flashlight slowly loses brightness (decays) if you're not charging.
// We use a long wind-up audio clip that "scrubs" according to how far we've charged,
// plus a short clank sound at the end of each chunk.

[RequireComponent(typeof(Light))]
public class CrankFlashlightReactiveAudio : MonoBehaviour
{
    [Header("Key Setup")]
    public KeyCode chargeKey = KeyCode.E;
    // Press/hold this key to charge the flashlight.

    [Header("Light Settings")]
    public float maxIntensity = 8f;   // Highest brightness the flashlight can reach.
    public float chargeChunk = 2f;    // How much brightness is gained per "chunk."
    public float chargeRate = 1f;     // How fast we move toward each chunk's target intensity.
    public float decayRate = 0.5f;    // How fast the flashlight dims when not charging.

    [Header("Audio")]
    public AudioSource windUpAudio;   // For the continuous wind-up sound.
    public AudioSource clankAudio;    // For the short clank sound at the end of a chunk.
    public AudioClip clankClip;       // Assign the audio file for the clank here.

    private Light flashlight;         // We'll store the Light component so we can change intensity.
    private float currentIntensity = 0f; // Track how bright the flashlight is at any time.

    // We'll charge from chunkStartIntensity up to targetIntensity in each chunk.
    private float chunkStartIntensity = 0f;
    private float targetIntensity = 0f;

    // isCharging means the key is held down. chunkFinished means we hit our chunk goal
    // and need to release the key before starting another chunk.
    private bool isCharging = false;
    private bool chunkFinished = false;

    void Start()
    {
        // Grab the Light component so we can set flashlight.intensity later.
        flashlight = GetComponent<Light>();
        flashlight.intensity = currentIntensity;

        // Make sure these AudioSources won't start automatically or loop unless we want them to.
        if (windUpAudio != null)
        {
            windUpAudio.playOnAwake = false;
            windUpAudio.loop = false;
        }
        if (clankAudio != null)
        {
            clankAudio.playOnAwake = false;
            clankAudio.loop = false;
        }
    }

    void Update()
    {
        // Split logic so it's easier to read: one part handles charging, another handles decay.
        HandleCharging();
        HandleDecaying();
    }

    private void HandleCharging()
    {
        // 1) If I press the key, and I'm not charging already, 
        //    and I haven't finished a chunk that I haven't released yet, start charging a new chunk.
        if (Input.GetKeyDown(chargeKey) && !isCharging && !chunkFinished)
        {
            isCharging = true;
            chunkStartIntensity = currentIntensity;
            targetIntensity = Mathf.Min(currentIntensity + chargeChunk, maxIntensity);

            // If we have a wind-up sound, stop and reset it to 0 before playing.
            if (windUpAudio != null)
            {
                windUpAudio.Stop();
                windUpAudio.time = 0f;
                windUpAudio.Play();
            }
        }

        // 2) If we are in the process of charging:
        if (isCharging)
        {
            // Check if the key is still held. If the player let go early, stop charging.
            if (!Input.GetKey(chargeKey))
            {
                StopChargingEarly();
                return;
            }

            // Otherwise, move the flashlight's intensity closer to our target chunk over time.
            currentIntensity = Mathf.MoveTowards(
                currentIntensity,
                targetIntensity,
                chargeRate * Time.deltaTime
            );
            flashlight.intensity = currentIntensity;

            // We'll also update the position in the wind-up audio clip so it sounds like we're "advancing" in real time.
            UpdateWindUpAudio();

            // If we've reached the chunk's target (no more to charge in this chunk):
            if (Mathf.Approximately(currentIntensity, targetIntensity))
            {
                chunkFinished = true;
                isCharging = false;

                // Play a clank sound to let us know we've maxed out this chunk.
                PlayClankSound();
            }
        }

        // 3) Once a chunk is finished, the user has to release the key to reset 'chunkFinished' 
        //    before we can crank up another chunk.
        if (Input.GetKeyUp(chargeKey) && chunkFinished)
        {
            chunkFinished = false;
        }
    }

    private void HandleDecaying()
    {
        // If we're not charging at all, the flashlight slowly dims over time until it goes dark (0 intensity).
        if (!isCharging && currentIntensity > 0f)
        {
            currentIntensity = Mathf.MoveTowards(
                currentIntensity,
                0f,
                decayRate * Time.deltaTime
            );
            flashlight.intensity = currentIntensity;
        }
    }

    private void StopChargingEarly()
    {
        // If we let go of the key mid-charge, we don't get a full chunk, so we stop right away.
        isCharging = false;

        // Also stop the wind-up sound (in case it's still playing).
        if (windUpAudio != null && windUpAudio.isPlaying)
        {
            windUpAudio.Stop();
        }
    }

    private void UpdateWindUpAudio()
    {
        // This is where we "scrub" the wind-up clip time to match how far along we are in the current chunk.
        if (windUpAudio == null || windUpAudio.clip == null) return;

        float chunkSize = targetIntensity - chunkStartIntensity;
        float chunkProgress = 0f;

        if (!Mathf.Approximately(chunkSize, 0f))
        {
            chunkProgress = (currentIntensity - chunkStartIntensity) / chunkSize;
        }

        // Clamp the progress between 0 and 1, just in case.
        chunkProgress = Mathf.Clamp01(chunkProgress);

        // Now figure out how far into the audio clip we want to be.
        // Subtract a bit so we don't accidentally go beyond the clip length.
        float newTime = chunkProgress * (windUpAudio.clip.length - 0.001f);

        // If there's a big difference from the current audio time, set it. 
        // This avoids micro-scrubbing that can make the audio stutter.
        if (Mathf.Abs(windUpAudio.time - newTime) > 0.05f)
        {
            newTime = Mathf.Clamp(newTime, 0f, windUpAudio.clip.length - 0.001f);
            windUpAudio.time = newTime;
        }
    }

    private void PlayClankSound()
    {
        // This plays the short clank clip to signal "chunk fully charged!"
        if (clankAudio != null && clankClip != null)
        {
            // Stopping just in case there's a leftover clank playing (rare, but safe).
            clankAudio.Stop();
            clankAudio.clip = clankClip;
            clankAudio.Play();
        }
    }
}
