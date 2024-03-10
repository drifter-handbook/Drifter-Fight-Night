using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum SyncedAudioType {
    SFX, MUSIC
}

public class AudioSystemManager : MonoBehaviour
{

    public static AudioSystemManager Instance => GameObject.FindGameObjectWithTag("AudioSystemManager").GetComponent<AudioSystemManager>();

    [SerializeField] private AudioSource source;

    [SerializeField] private AudioLibrary audioLibrary;

    [SerializeField] private AudioMixerGroup VoiceMixer;
    [SerializeField] private AudioMixerGroup SFXMixer;
    [SerializeField] private AudioMixerGroup UIMixer;
    [SerializeField] private AudioMixerGroup MusicMixer;

    // Start is called before the first frame update
    void Start()
    {
        audioLibrary.BuildLibrary();
    }
    
    public void CreateAudioUI(string name) {
        source.outputAudioMixerGroup = UIMixer;
        source.PlayOneShot(audioLibrary.FetchClip(name));
    }

    public void CreateAudioVoice(string name) {
        source.outputAudioMixerGroup = VoiceMixer;
        source.PlayOneShot(audioLibrary.FetchClip(name));
    }

    public void CreateAudioSFX(string name) {
        source.outputAudioMixerGroup = SFXMixer;
        source.PlayOneShot(audioLibrary.FetchClip(name));
    }

    public void CreateAudioMusic(string name) {
        source.outputAudioMixerGroup = MusicMixer;
        source.clip = audioLibrary.FetchClip(name);
        source.Play();
    }

    private void CreateAudioSFX(short id) {
        source.outputAudioMixerGroup = SFXMixer;
        source.PlayOneShot(audioLibrary.FetchClip(id));
    }

    private void CreateAudioMusic(short id) {
        source.outputAudioMixerGroup = MusicMixer;
        source.clip = audioLibrary.FetchClip(id);
        source.Play();
    }

    public void CreateSyncedSFX(string name) {
        CreateAudioSFX(name);
    }

    public void CreateSyncedMusic(string name) {
        CreateAudioMusic(name);
    }
}

