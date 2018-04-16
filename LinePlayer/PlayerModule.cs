using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSCore;
using CSCore.Codecs;
using CSCore.CoreAudioAPI;
using CSCore.SoundOut;

namespace LinePlayer
{
    class PlayerModule : IDisposable
    {
        private ISoundOut _soundOut;
        private IWaveSource _waveSource;
        private double volumeHandle = 100;  // control volume when _soundOut == null

        public event EventHandler<PlaybackStoppedEventArgs> PlaybackStopped;

        public PlaybackState PlaybackState
        {
            get
            {
                if (_soundOut != null)
                    return _soundOut.PlaybackState;
                return PlaybackState.Stopped;
            }
        }

        public TimeSpan Position
        {
            get
            {
                if (_waveSource != null)
                    return _waveSource.GetPosition();
                return TimeSpan.Zero;
            }
            set
            {
                if (_waveSource != null)
                    _waveSource.SetPosition(value);
            }
        }

        public TimeSpan Length
        {
            get
            {
                if (_waveSource != null)
                    return _waveSource.GetLength();
                return TimeSpan.Zero;
            }
        }

        public double Volume
        {
            get
            {
                if (_soundOut == null)
                {
                    return volumeHandle;
                }
                else
                {
                    return Math.Min(100, Math.Max((_soundOut.Volume * 100), 0));
                }
            }
            set
            {
                if (_soundOut == null)
                {
                    volumeHandle = value;
                }
                else
                {
                    volumeHandle = value;
                    _soundOut.Volume = Math.Min(1.0f, Math.Max((float)value / 100f, 0f));
                }
            }
        }

        public void Open(string filename)
        {
            CleanupPlayback();

            _waveSource =
                CodecFactory.Instance.GetCodec(filename)
                    .ToSampleSource()
                    .ToStereo()
                    .ToWaveSource();
            _soundOut = new WasapiOut() { Latency = 100 };
            _soundOut.Initialize(_waveSource);
            _soundOut.Volume = Math.Min(1.0f, Math.Max((float)volumeHandle / 100f, 0f));
            if (PlaybackStopped != null) _soundOut.Stopped += PlaybackStopped;
        }

        public void Play()
        {
            if (_soundOut != null)
                _soundOut.Play();
        }

        public void Pause()
        {
            if (_soundOut != null)
                _soundOut.Pause();
        }

        public void Stop()
        {
            if (_soundOut != null)
                _soundOut.Stop();
            if (_waveSource != null)
                _waveSource.SetPosition(TimeSpan.Zero);
        }

        private void CleanupPlayback()
        {
            if (_soundOut != null)
            {
                _soundOut.Dispose();
                _soundOut = null;
            }
            if (_waveSource != null)
            {
                _waveSource.Dispose();
                _waveSource = null;
            }
        }

        public void Dispose()
        {
            CleanupPlayback();
        }

        /*
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            CleanupPlayback();
        }
        */
    }
}
