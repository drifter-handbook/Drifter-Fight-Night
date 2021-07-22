using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum SyncedAudioType {
    SFX, MUSIC
}

public class AudioSystemManager : MonoBehaviour, INetworkMessageReceiver
{

    public static AudioSystemManager Instance => GameObject.FindGameObjectWithTag("AudioSystemManager").GetComponent<AudioSystemManager>();

    NetworkSync sync;

    [SerializeField] private AudioSource source;

    [SerializeField] private AudioLibrary audioLibrary;

    [SerializeField] private AudioMixerGroup VoiceMixer;
    [SerializeField] private AudioMixerGroup SFXMixer;
    [SerializeField] private AudioMixerGroup UIMixer;
    [SerializeField] private AudioMixerGroup MusicMixer;

    // Start is called before the first frame update
    void Start()
    {
        sync = GetComponent<NetworkSync>();
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
        if (GameController.Instance.IsHost)
        {
            CreateAudioSFX(name);
            sync.SendNetworkMessage(new AudioEffectPacket
            {
                audioType = (int)SyncedAudioType.SFX,
                id = audioLibrary.FetchID(name)
            }, LiteNetLib.DeliveryMethod.Unreliable);
        }
    }

    public void CreateSyncedMusic(string name) {
        if (GameController.Instance.IsHost)
        {
            CreateAudioMusic(name);
            sync.SendNetworkMessage(new AudioEffectPacket
            {
                audioType = (int)SyncedAudioType.MUSIC,
                id = audioLibrary.FetchID(name)
            }, LiteNetLib.DeliveryMethod.Unreliable);
        }
    }

    public void ReceiveNetworkMessage(NetworkMessage message)
    {
        if (!GameController.Instance.IsHost)
        {
            AudioEffectPacket effect = NetworkUtils.GetNetworkData<AudioEffectPacket>(message.contents);
            if (effect != null)
            {
                switch ((SyncedAudioType)effect.audioType)
                {
                    case SyncedAudioType.MUSIC:
                        CreateAudioMusic(effect.id);
                        break;
                    case SyncedAudioType.SFX:
                        CreateAudioSFX(effect.id);
                        break;
                }
            }
        }
    }

}

public class AudioEffectPacket : INetworkData
{
    public string Type { get; set; }
    public int audioType;
    public short id;
}
