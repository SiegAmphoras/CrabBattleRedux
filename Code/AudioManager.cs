using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class AudioDefinition
{
    public string SoundName;
    public AudioClip[] audioClips;

    public bool Loop = false;

    public float Volume = 1;
    public float Pitch = 1;

    public float SoundRangeMin = 10;
    public float SoundRangeMax = 50;

    [Range(0, 2)]
    public float PitchVariation = 0;

    public AudioClip GetAudioClip()
    {
        return audioClips[Random.Range(0, audioClips.Length)];
    }

    public float GetPitch()
    {
        return Pitch + Random.Range(-PitchVariation, PitchVariation);
    }
}

public class AudioManager : MonoBehaviour
{
    public Data_AudioDefinitions AudioDefinitions;
    List<AudioDefinition> defs;

    public static AudioManager Instance;

    static List<AudioSource> sources;

    static float effectsLevel;

    void Start()
    {
        Instance = this;

        defs = new List<AudioDefinition>();
        defs.AddRange(AudioDefinitions.definitions);

        sources = new List<AudioSource>();

        effectsLevel = float.Parse(GameSubsystems.Instance.GetConfigValue<string>("Audio", "EffectsLevel"));
    }

    public static void SetFXVolume(float volume)
    {
        if (Instance == null)
            return;

        effectsLevel = volume;
    }

    public AudioDefinition FindAudioDef(string name)
    {
        foreach (AudioDefinition a in defs)
        {
            if (a.SoundName == name)
                return a;
        }

        return null;
    }

    public static AudioSource PlaySoundAt(Vector3 position, string soundName)
    {
        AudioDefinition d = AudioManager.Instance.defs.Find(i => i.SoundName == soundName);

        if (d == null)
        {
            Debug.Log("Attempted to play undefined sound '" + soundName + "'!");
            return null;
        }

        return PlaySoundAt(position, d);
    }

    public static AudioSource PlaySoundAt(Vector3 position, AudioDefinition audioDef)
    {
        GameObject aObj = new GameObject("DynamicSound");
        aObj.transform.position = position;

        AudioSource aSrc = aObj.AddComponent<AudioSource>();

        AudioClip clip = audioDef.GetAudioClip();

        aSrc.clip = clip;
        aSrc.volume = audioDef.Volume * effectsLevel;
        aSrc.pitch = audioDef.GetPitch();
        aSrc.minDistance = audioDef.SoundRangeMin;
        aSrc.maxDistance = audioDef.SoundRangeMax;

        aSrc.Play();

        if (!audioDef.Loop)
            Destroy(aObj, clip.length);

        sources.Add(aSrc);
        return aSrc;
    }

    public static AudioSource PlaySoundParented(Transform transform, string soundName)
    {
        AudioDefinition d = AudioManager.Instance.defs.Find(i => i.SoundName == soundName);

        if (d == null)
        {
            Debug.Log("Attempted to play undefined sound '" + soundName + "'!");
            return null;
        }

        return PlaySoundParented(transform, d);
    }

    public static AudioSource PlaySoundParented(Transform transform, AudioDefinition audioDef)
    {
        GameObject aObj = new GameObject("DynamicSound");
        aObj.transform.parent = transform;
        aObj.transform.localPosition = Vector3.zero;
        aObj.transform.localRotation = Quaternion.identity;

        AudioSource aSrc = aObj.AddComponent<AudioSource>();

        AudioClip clip = audioDef.GetAudioClip();

        aSrc.clip = clip;
        aSrc.volume = audioDef.Volume * effectsLevel;
        aSrc.pitch = audioDef.GetPitch();
        aSrc.minDistance = audioDef.SoundRangeMin;
        aSrc.maxDistance = audioDef.SoundRangeMax;

        aSrc.Play();

        if(!audioDef.Loop)
            Destroy(aObj, clip.length);

        sources.Add(aSrc);
        return aSrc;
    }
}
