using CommunityToolkit.Maui.Views;
using E_Raamatud.Model;
using E_Raamatud.Services;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;

namespace E_Raamatud.View
{
    public partial class AudioPlayerPage : ContentPage
    {
        private MediaElement _mediaElement;
        private System.Timers.Timer _progressTimer;
        private bool _isDragging = false;
        private bool _isPlaying = false;

        private List<AudioChapter> _chapters = new();
        private int _currentChapterIndex = 0;
        private readonly int _raamatId;

        public AudioPlayerPage(int raamatId, string title, string audiofail, string coverImage)
        {
            InitializeComponent();

            _raamatId = raamatId;
            TitleLabel.Text = title;
            CoverImage.Source = coverImage;

            _chapters = AudioChapterResolver.Resolve(audiofail);
            ChapterListView.ItemsSource = _chapters;

            SetupMediaElement();
            SetupProgressTimer();

            _ = InitWithSavedProgressAsync();
        }

        // Setup

        private void SetupMediaElement()
        {
            _mediaElement = new MediaElement
            {
                ShouldAutoPlay = false,
                ShouldShowPlaybackControls = false,
                IsVisible = false
            };
            _mediaElement.MediaEnded += OnMediaEnded;

            if (Content is Grid grid)
                grid.Add(_mediaElement);
        }

        private void SetupProgressTimer()
        {
            _progressTimer = new System.Timers.Timer(500);
            _progressTimer.Elapsed += OnTimerTick;
            _progressTimer.AutoReset = true;
        }

        //Progress Save / Restore

        private async Task InitWithSavedProgressAsync()
        {
            if (_chapters.Count == 0) return;

            try
            {
                int userId = SessionService.CurrentUser?.Id ?? 0;
                if (userId > 0)
                {
                    var saved = await DatabaseService.Instance.GetReadingProgressAsync(userId, _raamatId);
                    if (saved != null && (saved.AudioChapter > 0 || saved.AudioPosition > 0))
                    {
                        LoadChapter(saved.AudioChapter, autoPlay: false);
                        // Short delay to let the media source load before seeking
                        await Task.Delay(800);
                        if (saved.AudioPosition > 0)
                            _mediaElement.SeekTo(TimeSpan.FromSeconds(saved.AudioPosition));
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"InitWithSavedProgressAsync error: {ex.Message}");
            }

            LoadChapter(0);
        }

        private async Task SaveAudioProgressAsync()
        {
            try
            {
                int userId = SessionService.CurrentUser?.Id ?? 0;
                if (userId <= 0) return;

                var position = _mediaElement.Position.TotalSeconds;

                var existing = await DatabaseService.Instance.GetReadingProgressAsync(userId, _raamatId);
                if (existing != null)
                {
                    existing.AudioChapter = _currentChapterIndex;
                    existing.AudioPosition = position;
                    await DatabaseService.Instance.UpdateReadingProgressAsync(existing);
                }
                else
                {
                    await DatabaseService.Instance.InsertReadingProgressAsync(new ReadingProgress
                    {
                        Kasutaja_ID = userId,
                        Raamat_ID = _raamatId,
                        AudioChapter = _currentChapterIndex,
                        AudioPosition = position
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SaveAudioProgressAsync error: {ex.Message}");
            }
        }

        // Chapter Loading

        private void LoadChapter(int index, bool autoPlay = false)
        {
            if (index < 0 || index >= _chapters.Count) return;

            _currentChapterIndex = index;
            var chapter = _chapters[index];

            ChapterLabel.Text = chapter.Title;
            ChapterCountLabel.Text = $"Peatükk {index + 1} / {_chapters.Count}";
            TimeLabel.Text = "0:00 / 0:00";
            SeekBar.Value = 0;
            ChapterListView.SelectedItem = chapter;

            _mediaElement.Stop();
            _isPlaying = false;
            _progressTimer.Stop();
            PlayPauseButton.Text = "▶";

            var path = chapter.FilePath;
            if (System.IO.Path.IsPathRooted(path) && System.IO.File.Exists(path))
                _mediaElement.Source = MediaSource.FromFile(path);
            else
                _mediaElement.Source = MediaSource.FromUri(new Uri(path));

            if (autoPlay)
            {
                _mediaElement.Play();
                PlayPauseButton.Text = "⏸";
                _progressTimer.Start();
                _isPlaying = true;
            }
        }

        // Playback Controls

        private void OnPlayPause(object sender, EventArgs e)
        {
            if (_isPlaying)
            {
                _mediaElement.Pause();
                PlayPauseButton.Text = "▶";
                _progressTimer.Stop();
                _isPlaying = false;
                _ = SaveAudioProgressAsync();
            }
            else
            {
                _mediaElement.Play();
                PlayPauseButton.Text = "⏸";
                _progressTimer.Start();
                _isPlaying = true;
            }
        }

        private void OnRewind(object sender, EventArgs e)
        {
            var newPos = _mediaElement.Position - TimeSpan.FromSeconds(15);
            _mediaElement.SeekTo(newPos < TimeSpan.Zero ? TimeSpan.Zero : newPos);
        }

        private void OnFastForward(object sender, EventArgs e)
        {
            var newPos = _mediaElement.Position + TimeSpan.FromSeconds(30);
            var duration = _mediaElement.Duration;
            _mediaElement.SeekTo(newPos > duration ? duration : newPos);
        }

        private void OnPrevChapter(object sender, EventArgs e)
        {
            if (_currentChapterIndex > 0)
            {
                _ = SaveAudioProgressAsync();
                LoadChapter(_currentChapterIndex - 1, autoPlay: _isPlaying);
            }
        }

        private void OnNextChapter(object sender, EventArgs e)
        {
            if (_currentChapterIndex < _chapters.Count - 1)
            {
                _ = SaveAudioProgressAsync();
                LoadChapter(_currentChapterIndex + 1, autoPlay: _isPlaying);
            }
        }

        private void OnMediaEnded(object sender, EventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (_currentChapterIndex < _chapters.Count - 1)
                {
                    _ = SaveAudioProgressAsync();
                    LoadChapter(_currentChapterIndex + 1, autoPlay: true);
                }
                else
                {
                    PlayPauseButton.Text = "▶";
                    _progressTimer.Stop();
                    _isPlaying = false;
                    SeekBar.Value = 0;
                    TimeLabel.Text = $"0:00 / {FormatTime(_mediaElement.Duration)}";
                    _ = SaveAudioProgressAsync();
                }
            });
        }

        // Chapter List

        private void OnToggleChapterList(object sender, EventArgs e)
        {
            ChapterListFrame.IsVisible = !ChapterListFrame.IsVisible;
        }

        private void OnChapterSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection?.Count > 0 && e.CurrentSelection[0] is AudioChapter selected)
            {
                _ = SaveAudioProgressAsync();
                LoadChapter(selected.Index - 1, autoPlay: true);
            }
        }

        // Seek Bar

        private void OnSeekBarDragStarted(object sender, EventArgs e) => _isDragging = true;

        private void OnSeekBarDragCompleted(object sender, EventArgs e)
        {
            _isDragging = false;
            var duration = _mediaElement.Duration;
            if (duration.TotalSeconds > 0)
                _mediaElement.SeekTo(TimeSpan.FromSeconds(SeekBar.Value * duration.TotalSeconds));
            _ = SaveAudioProgressAsync();
        }

        // Speed

        private void OnSpeedChanged(object sender, EventArgs e)
        {
            if (sender is Button btn && double.TryParse(btn.CommandParameter?.ToString(),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double speed))
            {
                _mediaElement.Speed = speed;
                foreach (var b in new[] { SpeedBtn075, SpeedBtn100, SpeedBtn150, SpeedBtn200 })
                {
                    b.BackgroundColor = Color.FromArgb("#2a3a4a");
                    b.TextColor = Color.FromArgb("#aaccdd");
                }
                btn.BackgroundColor = Color.FromArgb("#549082");
                btn.TextColor = Colors.White;
            }
        }

        // Timer Tick

        private void OnTimerTick(object sender, ElapsedEventArgs e)
        {
            if (_isDragging) return;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    var pos = _mediaElement.Position;
                    var dur = _mediaElement.Duration;
                    if (dur.TotalSeconds > 0)
                    {
                        SeekBar.Value = pos.TotalSeconds / dur.TotalSeconds;
                        TimeLabel.Text = $"{FormatTime(pos)} / {FormatTime(dur)}";
                    }
                }
                catch (Exception ex) { Debug.WriteLine($"Timer error: {ex.Message}"); }
            });
        }

        // Helpers

        private static string FormatTime(TimeSpan t) =>
            t.TotalHours >= 1
                ? $"{(int)t.TotalHours}:{t.Minutes:D2}:{t.Seconds:D2}"
                : $"{t.Minutes}:{t.Seconds:D2}";

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _ = SaveAudioProgressAsync();
            _progressTimer?.Stop();
            _progressTimer?.Dispose();
            _mediaElement?.Stop();
        }
    }
}