using UnityEngine;
using UnityEngine.Audio;

namespace UncertainLuei.CaudexLib.Components
{
    // Allows multiple things to use the Library's silence function without butting into one another
    public class PlayerSilenceManager : MonoBehaviour
    {
        private uint silences;
        internal static AudioMixer mixer;

        public void Silence(bool active)
        {
            if (active)
            {
                if (silences == 0)
                {
                    AudioListener.volume = 0f;
                    mixer.SetFloat("EchoWetMix", 1f);
                }
                silences++;
                return;
            }

            if (silences == 0) return;
            
            silences--;
            if (silences == 0)
            {
                AudioListener.volume = 1f;
                mixer.SetFloat("EchoWetMix", 0f);
            }
        }

        private void OnDestroy()
        {
            if (silences > 0)
            {
                silences = 1;
                Silence(false);
            }
        }
    }
}