using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Modules.Utility.Singleton;
using Mirror;

namespace Contra
{
    public class SoundManager : MonoSingleton<SoundManager>
    {
        /// <summary>
        /// 游戏中所有声音分为4个类型，对应4个AudioSource
        /// </summary>
        public enum SoundType
        {
            BGM,    //背景音乐
            Shoot,  //开枪的声音
            Effect, //特效声音（爆炸等）
            Other   //其它声音
        }

        private static Dictionary<string, AudioClip> _Sounds = new Dictionary<string, AudioClip>();

        private AudioSource _AudBGM;

        private AudioSource _AudShoot;

        private AudioSource _AudEffect;

        private AudioSource _AudOther;

        protected override void OnAwake()
        {
            DontDestroyOnLoad(gameObject);

            AudioSource[] auds = GetComponents<AudioSource>();
            _AudBGM = auds[0];
            _AudShoot = auds[1];
            _AudEffect = auds[2];
            _AudOther = auds[3];
        }

        public void LoadSounds()
        {
            Addressables.LoadAssetsAsync<AudioClip>("Sound", (x) => _Sounds.Add(x.name, x)).WaitForCompletion();
        }

        public void Play(SoundType type, string name, bool loop)
        {
            AudioSource aud = _GetSource(type);
            aud.clip = _Sounds[name];
            aud.loop = loop;
            aud.Play();
        }

        public bool IsSourcePlaying(SoundType type)
        {
            return _GetSource(type).isPlaying;
        }

        private AudioSource _GetSource(SoundType type)
        {
            switch (type)
            {
                case SoundType.BGM:
                    return _AudBGM;
                case SoundType.Shoot:
                    return _AudShoot;
                case SoundType.Effect:
                    return _AudEffect;
                case SoundType.Other:
                    return _AudOther;
                default: return null;
            }
        }
    }
}
