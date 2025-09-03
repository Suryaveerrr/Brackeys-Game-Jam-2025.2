using System;
using UnityEngine;
using UnityEngine.Audio;

public class Audio : MonoBehaviour
{
    public Sound[] sounds;
    
    void Start()
    {
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
        }
    }
    
    

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning($"Sound with name '{name}' not found!");
            return;
        }

        if (s.clip == null)
        {
            Debug.LogError($"AudioClip for sound '{name}' is not assigned!");
            return;
        }

        Debug.Log($"Playing sound: {name}");
        s.source.Play();
    }
}
