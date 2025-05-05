using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SoundEffectManager : NetworkBehaviour
{
    public static SoundEffectManager Instance;

    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
    }

    public List<Sound> sounds;
    private Dictionary<string, AudioClip> soundDict = new Dictionary<string, AudioClip>();

    private AudioSource audioSource;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float globalVolume = 1f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = GetComponent<AudioSource>();

            foreach (var sound in sounds)
            {
                soundDict[sound.name] = sound.clip;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // เล่นเสียง Local (UI หรือ Client-side เท่านั้น)
    public void PlayLocal(string soundName, float volume = 1f)
    {
        if (soundDict.TryGetValue(soundName, out var clip))
        {
            audioSource.PlayOneShot(clip, volume * globalVolume);
        }
        else
        {
            Debug.LogWarning($"Sound {soundName} not found!");
        }
    }

    // เรียกใช้เสียงให้ทุก Client เล่น (สำหรับ Multiplayer)
    public void PlayGlobal(string soundName, float volume = 1f)
    {
        if (IsServer)
        {
            PlaySoundClientRpc(soundName, volume);
        }
        else
        {
            PlaySoundServerRpc(soundName, volume);
        }
    }
    

    [ServerRpc(RequireOwnership = false)]
    void PlaySoundServerRpc(string soundName, float volume)
    {
        PlaySoundClientRpc(soundName, volume);
    }

    [ClientRpc]
    void PlaySoundClientRpc(string soundName, float volume)
    {
        PlayLocal(soundName, volume);
    }

    [ServerRpc(RequireOwnership = false)]
    void PlaySoundAtPositionServerRpc(string soundName, Vector3 position, float volume)
    {
        PlaySoundAtPositionClientRpc(soundName, position, volume);
    }

    [ClientRpc]
    void PlaySoundAtPositionClientRpc(string soundName, Vector3 position, float volume)
    {
        if (soundDict.TryGetValue(soundName, out var clip))
        {
            AudioSource.PlayClipAtPoint(clip, position, volume * globalVolume);
        }
        else
        {
            Debug.LogWarning($"Sound {soundName} not found!");
        }
    }
    
    public void PlayGlobal3DAtPosition(string soundName, Vector3 position, float volume = 1f, float minDistance = 1f, float maxDistance = 10f)
    {
        if (IsServer)
        {
            Play3DSoundAtPositionClientRpc(soundName, position, volume, minDistance, maxDistance);
        }
        else
        {
            Play3DSoundAtPositionServerRpc(soundName, position, volume, minDistance, maxDistance);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void Play3DSoundAtPositionServerRpc(string soundName, Vector3 position, float volume, float minDistance, float maxDistance)
    {
        Play3DSoundAtPositionClientRpc(soundName, position, volume, minDistance, maxDistance);
    }
    
    [ClientRpc]
    void Play3DSoundAtPositionClientRpc(string soundName, Vector3 position, float volume, float minDistance, float maxDistance)
    {
        if (soundDict.TryGetValue(soundName, out var clip))
        {
            float listenerZ = Camera.main != null ? Camera.main.transform.position.z : 0f;
            Vector3 soundPos = new Vector3(position.x, position.y, listenerZ);

            GameObject tempGO = new GameObject("Temp2DAudio");
            tempGO.transform.position = soundPos;

            AudioSource aSource = tempGO.AddComponent<AudioSource>();
            aSource.clip = clip;
            aSource.volume = volume * globalVolume;
            aSource.spatialBlend = 1.0f;
            aSource.minDistance = minDistance;
            aSource.maxDistance = maxDistance;
            aSource.rolloffMode = AudioRolloffMode.Linear;

            aSource.Play();
            Destroy(tempGO, clip.length);
        }
        else
        {
            Debug.LogWarning($"Sound {soundName} not found!");
        }
    }


}
