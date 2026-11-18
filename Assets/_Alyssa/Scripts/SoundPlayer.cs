using UnityEngine;

namespace Alyssa
{
    public class SoundPlayer : MonoBehaviour
    {
        public AudioClip[] clips;
        [Range(0f, 1f)] public float volume = 1f;
        public Vector2 pitchRange = new Vector2(0.95f, 1.05f);
        public bool scaleWithSpeed;
        public Vector2 speedRange = new Vector2(1f, 5f);
        public Vector2 volumeRange = new Vector2(0.5f, 1f);

        public void Play()
        {
            Debug.Log("SoundPlayer.Play on " + name, this);
            if (clips == null || clips.Length == 0) return;

            AudioClip clip = clips[Random.Range(0, clips.Length)];
            float vol = volume;

            if (scaleWithSpeed)
            {
                float t = Mathf.InverseLerp(speedRange.x, speedRange.y, Cuttable.LastHitSpeed);
                vol *= Mathf.Lerp(volumeRange.x, volumeRange.y, t);
            }

            PlayAt(clip, transform.position, vol, Random.Range(pitchRange.x, pitchRange.y));
        }

        public static void PlayAt(AudioClip clip, Vector3 pos, float vol, float pitch)
        {
            GameObject go = new GameObject("OneShotAudio");
            go.transform.position = pos;
            AudioSource src = go.AddComponent<AudioSource>();
            src.clip = clip;
            src.volume = vol;
            src.pitch = pitch;
            src.spatialBlend = 1f;
            src.Play();
            Destroy(go, clip.length / pitch + 0.1f);
        }
    }
}