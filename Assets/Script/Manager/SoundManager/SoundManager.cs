using UnityEngine;
using System.Collections;
using System.Collections.Generic;


using Common;

namespace JuneSample
{
    public enum FXSound : int
    {
        None,
        Button,
        Max,
    }

    public class SoundManager
    {
        static SoundManager _instance;
        public static SoundManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new SoundManager();
                return _instance;
            }
        }

        public const string SOUND_RESOUCE_PATH = "Sound";

        public const string SOUND_BUNDLE_PATH = "Sound";
        public const string BGM_PATH = "/BGM/BGM.ogg";

        #region Sound Path
        public static Dictionary<int, string> FXSoundPath = new Dictionary<int, string>
        {
            {(int)FXSound.Button, SOUND_RESOUCE_PATH + "ButtonPath"},
         
        };
        #endregion

        GameObject _globalAudioObj;
        Transform _globalAudioTransform;
        AudioSource _globalAudioSource;

        AudioSource _globalBGMAudioSource;
        AudioSource _dancingHallBGMAudioSource;

        List<AudioSource> _2dAudioList = new List<AudioSource>();

        float _bgmVolume;
        float _sfxVolume;

        public AudioSource GlobalBGM { get { return _globalBGMAudioSource; } }
        public AudioSource DancingHallBGM { get { return _dancingHallBGMAudioSource; } }

        private ObjectPool<AudioSource> _audio2DPool;
        public ObjectPool<AudioSource> AudioPool2D { get { return _audio2DPool; } }
        private Dictionary<int, AudioClip> _clips = new Dictionary<int, AudioClip>();

        public float BGMVolume
        {
            get
            {
                return _bgmVolume;
            }
        }

        public void Initialize()
        {
            _globalAudioObj = new GameObject("GlobalAudio");
            _globalAudioTransform = _globalAudioObj.transform;
            _globalAudioTransform.parent = GameManager.Instance.transform;

            _globalAudioSource = _globalAudioObj.AddComponent<AudioSource>();
            _globalAudioSource.spatialBlend = 0f;

            _globalBGMAudioSource = _globalAudioObj.AddComponent<AudioSource>();
            _globalBGMAudioSource.spatialBlend = 0f;
            _dancingHallBGMAudioSource = _globalAudioObj.AddComponent<AudioSource>();
            _dancingHallBGMAudioSource.spatialBlend = 0f;

            #region Sound Pool
            _audio2DPool = new ObjectPool<AudioSource>()
            {
                InitializeCreateCount = 7,
                ExpandCreateCount = 2,
                OnObjectDestroy = (AudioSource v) =>
                {
                    _2dAudioList.Remove(v);
                    GameObject.Destroy(v);
                },
                OnTake = (AudioSource v) =>
                {
                    v.playOnAwake = false;
                    v.volume = 1f;
                },
                OnRelease = (AudioSource v) =>
                {
                    if (v)
                    {
                        if (v.isPlaying) v.Stop();

                        v.clip = null;
                        v.outputAudioMixerGroup = null;
                    }
                },
                OnCreate = () =>
                {
                    var data = _globalAudioObj.AddComponent<AudioSource>();
                    _2dAudioList.Add(data);
                    return data;
                }
            };
            #endregion
        }

        public void ClearBGMSound()
        {
            ClearAudioSource(_globalBGMAudioSource);
            ClearAudioSource(_dancingHallBGMAudioSource);
        }

        public void SoundMute(bool mute)
        {
            _globalAudioSource.mute = mute;
            _globalBGMAudioSource.mute = mute;
            _dancingHallBGMAudioSource.mute = mute;

            for (int i = 0; i < _2dAudioList.Count; i++)
                _2dAudioList[i].mute = mute;
        }

        void ClearAudioSource(AudioSource audioSource)
        {
            if (audioSource != null)
            {
                audioSource.loop = false;
                if (audioSource.isPlaying)
                    audioSource.Stop();

                if (audioSource.clip)
                    audioSource.clip = null;
            }
        }

        public void LoadDefaultSound()
        {
            for (FXSound i = FXSound.None + 1; i < FXSound.Max; i++)
            {
                LoadFxSound(i);
            }
        }

        public void ChangeBGMVolume(float masterValue, float value)
        {
            _bgmVolume = value * masterValue;
            GlobalBGM.volume = _bgmVolume;
            DancingHallBGM.volume = _bgmVolume;
        }

        public void ChangeSFXVolume(float masterValue, float value)
        {
            _sfxVolume = value * masterValue;
        }

        public void ChangeMasterVolume(float bgmValue, float sfxValue, float value)
        {
            ChangeBGMVolume(value, bgmValue);
            ChangeSFXVolume(value, sfxValue);
        }

        public AudioClip GetAudioClip(FXSound sound)
        {
            int key = (int)sound;
            AudioClip result;
            if (!_clips.TryGetValue(key, out result))
            {
                LoadFxSound(sound);
                if (_clips.TryGetValue(key, out result))
                {
                    return result;
                }
                return null;
            }
            else
            {
                return result;
            }
        }

        public void LoadFxSound(FXSound sound)
        {
            int key = (int)sound;
            if (!_clips.ContainsKey(key) && FXSoundPath.ContainsKey(key))
            {
                // 번들 시스템에서 제외
                //var data = ResourceLoad.Load<AudioClip>(SOUND_RESOUCE_PATH, FXSoundPath[key]);
                var data = ResourceLoad.Load<AudioClip>(FXSoundPath[key]);
                if (data != null)
                    _clips.Add(key, data);
            }
        }

        public void UnLoadFxSound(FXSound sound)
        {
            int key = (int)sound;
            if (_clips.ContainsKey(key))
                _clips.Remove(key);
        }

        public void ClaerAudioClip()
        {
            _clips.Clear();
        }

        #region Sound Play 관련
        public void Play(FXSound entry, float volume = 1f)
        {
            if (entry == FXSound.None) return;
            Play(GetAudioClip(entry), _sfxVolume * volume);
        }

        // ignoreOptionValume : 옵션 볼륨을 무시하는 사운드를 위한 값
        public void Play(AudioClip clip, float volume = 1f, bool ignoreOptionValume = false)
        {
            _globalAudioSource.PlayOneShot(clip, ignoreOptionValume == false ? _sfxVolume * volume : 1f);
        }

        public void Stop()
        {
            if (_globalAudioSource.isPlaying)
                _globalAudioSource.Stop();
        }

        public void PlayPos(FXSound entry, Vector3 position, float volume = 1f)
        {
            if (entry == FXSound.None) return;
            PlayPos(GetAudioClip(entry), position, _sfxVolume * volume);
        }

        public void PlayPos(AudioClip clip, Vector3 position, float volume = 1f)
        {
            AudioSource.PlayClipAtPoint(clip, position, _sfxVolume * volume);
        }

        public ObjectPoolWrapper<AudioSource> TakePlay(FXSound entry, float volume = 1f, float speed = 1f, float delay = 0f, bool repeat = false)
        {
            if (entry == FXSound.None) return null;
            return TakePlay(GetAudioClip(entry), _sfxVolume * volume, speed, delay, repeat);
        }

        public ObjectPoolWrapper<AudioSource> TakePlay(AudioClip clip, float volume = 1f, float speed = 1f, float delay = 0f, bool repeat = false, bool ignoreOptionValume = false)
        {
            var waudio = _audio2DPool.Take();
            var audio = waudio.Value;

            audio.clip = clip;
            audio.loop = repeat;
            audio.volume = ignoreOptionValume == false ? _sfxVolume * volume : 1f;
            audio.pitch = speed;

            if (delay > 0)
            {
                audio.PlayDelayed(delay);
            }
            else
            {
                audio.Play();
            }
            return waudio;
        }
        #endregion
    }
}