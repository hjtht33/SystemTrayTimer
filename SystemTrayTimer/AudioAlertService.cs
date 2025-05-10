using NAudio.Utils;
using NAudio.Wave;
using System;
using System.IO;
using System.Windows.Forms;


namespace SystemTrayTimer
{
    public class AudioAlertService : IDisposable
    {
        private WaveOutEvent _waveOut;
        private AudioFileReader _audioFile;
        private Timer _fadeTimer;
        private float _initialVolume = 2.0f;
        private bool _disposed;
        private int _effectiveDuration;

        public string CustomSoundPath { get; set; }
        public bool UseSystemSound { get; set; } = true;
        public bool EnableFade { get; set; } = true;
        public int MaxDuration { get; set; } = 8000;

        public event Action<string> AlertTriggered;

        public void PlayAlert()
        {
            Stop();

            if (UseSystemSound)
            {
                // 触发时间到的通知事件
                AlertTriggered?.Invoke("预定时间已到");
            }
            else if (!string.IsNullOrEmpty(CustomSoundPath) && File.Exists(CustomSoundPath))
            {
                PlayCustomSound();
            }
            
        }

        private void PlayCustomSound()
        {
            try
            {
                _audioFile = new AudioFileReader(CustomSoundPath);
                _waveOut = new WaveOutEvent();
                _waveOut.Init(_audioFile);

                // 动态计算实际持续时间
                _effectiveDuration = (int)Math.Min(_audioFile.TotalTime.TotalMilliseconds, MaxDuration);

                if (EnableFade)
                {
                    SetupFadeEffect();
                }
                else
                {
                    _audioFile.Volume = 1.0f;
                }

                _waveOut.Play();
                

                // 单一定时器控制
                StartPlaybackMonitor();
            }
            catch (Exception ex)
            {
                AlertTriggered?.Invoke($"播放失败: {ex.Message}");
            }
        }

        private void SetupFadeEffect()
        {
            _initialVolume = 1.0f;
            _audioFile.Volume = 0;
            _fadeTimer = new Timer { Interval = 50 };
            _fadeTimer.Tick += FadeTickHandler;
            _fadeTimer.Start();
        }

        private void FadeTickHandler(object sender, EventArgs e)
        {
            if (_audioFile == null || _waveOut == null) return;

            var elapsed = _waveOut.GetPositionTimeSpan().TotalMilliseconds;

            // 提前终止条件
            if (elapsed >= _effectiveDuration || _waveOut.PlaybackState == PlaybackState.Stopped)
            {
                Stop();
                return;
            }

            // 淡入阶段（前1秒）
            if (elapsed < 1000)
            {
                _audioFile.Volume = Math.Min(1.0f, (float)(elapsed / 1000));
            }
            // 淡出阶段（最后1秒）
            else if (elapsed > _effectiveDuration - 1000)
            {
                _audioFile.Volume = Math.Max(0, (float)((_effectiveDuration - elapsed) / 1000));
            }
            // 稳定阶段
            else
            {
                _audioFile.Volume = _initialVolume;
            }
        }

        private void StartPlaybackMonitor()
        {
            // 单一定时器负责监控
            var monitorTimer = new Timer { Interval = 100 };
            DateTime startTime = DateTime.Now;

            monitorTimer.Tick += (s, e) =>
            {
                // 双重停止条件：自然结束或超时
                var elapsed = (DateTime.Now - startTime).TotalMilliseconds;

                if (_waveOut?.PlaybackState == PlaybackState.Stopped ||
                    elapsed >= _effectiveDuration)
                {
                    Stop();
                    monitorTimer.Dispose();
                }
            };
            monitorTimer.Start();
        }

        public void Stop()
        {
            _fadeTimer?.Stop();
            _waveOut?.Stop();
            DisposeResources();
        }

        private void DisposeResources()
        {
            _fadeTimer?.Dispose();
            _fadeTimer = null;
            _audioFile?.Dispose();
            _audioFile = null;
            _waveOut?.Dispose();
            _waveOut = null;
        }

        public void Dispose()
        {
            if (_disposed) return;
            Stop();
            _disposed = true;
        }
    }
}
