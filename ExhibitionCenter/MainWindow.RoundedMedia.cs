using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using LibVLCSharp.Shared;

using VlcMedia = LibVLCSharp.Shared.Media;
using VlcMediaPlayer = LibVLCSharp.Shared.MediaPlayer;

namespace ExhibitionCenter
{
    public partial class MainWindow
    {
        private LibVLC? _roundedLibVLC;
        private VlcMediaPlayer? _roundedMediaPlayer;
        private VlcMedia? _roundedMedia;

        private WriteableBitmap? _roundedVideoBitmap;
        private ImageBrush? _roundedVideoBrush;

        private IntPtr _roundedVideoBuffer = IntPtr.Zero;
        private byte[]? _roundedFrameBytes;

        private uint _roundedVideoWidth;
        private uint _roundedVideoHeight;
        private uint _roundedVideoPitch;
        private uint _roundedVideoLines;

        private int _roundedFramePending;
        private int _roundedPlayGeneration;

        private bool _roundedShutdownHooked;


        private void EnsureRoundedVideoSurface()
        {
            if (_roundedMediaPlayer is not null)
                return;

            Core.Initialize();

            _roundedLibVLC =
                new LibVLC(
                    "--no-video-title-show");

            _roundedMediaPlayer =
                new VlcMediaPlayer(
                    _roundedLibVLC);

            _roundedMediaPlayer.Volume = 100;

            _roundedMediaPlayer.Playing +=
                RoundedMediaPlayer_Playing;

            _roundedMediaPlayer.EndReached +=
                RoundedMediaPlayer_EndReached;

            _roundedMediaPlayer.EncounteredError +=
                RoundedMediaPlayer_EncounteredError;

            if (!_roundedShutdownHooked)
            {
                Closed +=
                    MainWindow_RoundedMediaClosed;

                _roundedShutdownHooked = true;
            }
        }


        private async void PlayRoundedVideo(
            string path)
        {
            int generation =
                Interlocked.Increment(
                    ref _roundedPlayGeneration);

            try
            {
                EnsureRoundedVideoSurface();

                if (_roundedMediaPlayer is null ||
                    _roundedLibVLC is null)
                {
                    return;
                }

                StopRoundedVideoInternal();

                _roundedMedia =
                    new VlcMedia(
                        _roundedLibVLC,
                        path,
                        FromType.FromPath);

                await _roundedMedia.Parse(
                    MediaParseOptions.ParseLocal,
                    5000);

                if (generation !=
                    Volatile.Read(
                        ref _roundedPlayGeneration))
                {
                    return;
                }

                uint sourceWidth = 1280;
                uint sourceHeight = 720;

                foreach (var track in _roundedMedia.Tracks)
                {
                    if (track.TrackType !=
                        TrackType.Video)
                    {
                        continue;
                    }

                    if (track.Data.Video.Width > 0 &&
                        track.Data.Video.Height > 0)
                    {
                        sourceWidth =
                            track.Data.Video.Width;

                        sourceHeight =
                            track.Data.Video.Height;

                        break;
                    }
                }

                PrepareRoundedVideoBuffer(
                    sourceWidth,
                    sourceHeight);

                if (_roundedMediaPlayer is null ||
                    _roundedMedia is null)
                {
                    return;
                }

                _roundedMediaPlayer.SetVideoFormat(
                    "RV32",
                    _roundedVideoWidth,
                    _roundedVideoHeight,
                    _roundedVideoPitch);

                _roundedMediaPlayer.SetVideoCallbacks(
                    RoundedVideoLock,
                    null,
                    RoundedVideoDisplay);

                VideoSurface.Visibility =
                    Visibility.Visible;

                bool started =
                    _roundedMediaPlayer.Play(
                        _roundedMedia);

                if (!started)
                {
                    ShowRoundedVideoFailure();
                }
            }
            catch
            {
                ShowRoundedVideoFailure();
            }
        }


        private void PrepareRoundedVideoBuffer(
            uint sourceWidth,
            uint sourceHeight)
        {
            ReleaseRoundedVideoBuffer();

            double scale = Math.Min(
                1.0,
                Math.Min(
                    1280.0 / sourceWidth,
                    720.0 / sourceHeight));

            _roundedVideoWidth =
                Math.Max(
                    2,
                    (uint)Math.Round(
                        sourceWidth * scale));

            _roundedVideoHeight =
                Math.Max(
                    2,
                    (uint)Math.Round(
                        sourceHeight * scale));

            _roundedVideoPitch =
                Align32(
                    _roundedVideoWidth * 4);

            _roundedVideoLines =
                Align32(
                    _roundedVideoHeight);

            long nativeSize =
                (long)_roundedVideoPitch *
                _roundedVideoLines;

            long visibleSize =
                (long)_roundedVideoPitch *
                _roundedVideoHeight;

            _roundedVideoBuffer =
                Marshal.AllocHGlobal(
                    new IntPtr(nativeSize));

            _roundedFrameBytes =
                new byte[
                    checked((int)visibleSize)];

            _roundedVideoBitmap =
                new WriteableBitmap(
                    (int)_roundedVideoWidth,
                    (int)_roundedVideoHeight,
                    96,
                    96,
                    PixelFormats.Bgra32,
                    null);

            _roundedVideoBrush =
                new ImageBrush(
                    _roundedVideoBitmap)
                {
                    Stretch =
                        Stretch.UniformToFill,

                    AlignmentX =
                        AlignmentX.Center,

                    AlignmentY =
                        AlignmentY.Center
                };

            VideoSurface.Fill =
                _roundedVideoBrush;
        }


        private static uint Align32(
            uint value)
        {
            if (value % 32 == 0)
                return value;

            return
                ((value / 32) + 1) * 32;
        }


        private IntPtr RoundedVideoLock(
            IntPtr opaque,
            IntPtr planes)
        {
            IntPtr buffer =
                _roundedVideoBuffer;

            if (buffer == IntPtr.Zero)
                return IntPtr.Zero;

            Marshal.WriteIntPtr(
                planes,
                buffer);

            return IntPtr.Zero;
        }


        private void RoundedVideoDisplay(
            IntPtr opaque,
            IntPtr picture)
        {
            if (_roundedVideoBuffer ==
                    IntPtr.Zero ||
                _roundedFrameBytes is null ||
                _roundedVideoBitmap is null)
            {
                return;
            }

            if (Interlocked.Exchange(
                    ref _roundedFramePending,
                    1) != 0)
            {
                return;
            }

            try
            {
                Marshal.Copy(
                    _roundedVideoBuffer,
                    _roundedFrameBytes,
                    0,
                    _roundedFrameBytes.Length);

                byte[] frame =
                    _roundedFrameBytes;

                WriteableBitmap bitmap =
                    _roundedVideoBitmap;

                int width =
                    (int)_roundedVideoWidth;

                int height =
                    (int)_roundedVideoHeight;

                int pitch =
                    (int)_roundedVideoPitch;

                Dispatcher.BeginInvoke(
                    DispatcherPriority.Render,
                    new Action(
                        () =>
                        {
                            try
                            {
                                bitmap.WritePixels(
                                    new Int32Rect(
                                        0,
                                        0,
                                        width,
                                        height),
                                    frame,
                                    pitch,
                                    0);
                            }
                            catch
                            {
                            }
                            finally
                            {
                                Volatile.Write(
                                    ref _roundedFramePending,
                                    0);
                            }
                        }));
            }
            catch
            {
                Volatile.Write(
                    ref _roundedFramePending,
                    0);
            }
        }


        private void RoundedMediaPlayer_Playing(
            object? sender,
            EventArgs e)
        {
            Dispatcher.BeginInvoke(
                new Action(
                    () =>
                    {
                        MediaStatusText.Visibility =
                            Visibility.Collapsed;

                        PlayButton.Visibility =
                            Visibility.Collapsed;

                        VideoSurface.Visibility =
                            Visibility.Visible;
                    }));
        }


        private void RoundedMediaPlayer_EndReached(
            object? sender,
            EventArgs e)
        {
            Dispatcher.BeginInvoke(
                new Action(
                    StopVideo));
        }


        private void RoundedMediaPlayer_EncounteredError(
            object? sender,
            EventArgs e)
        {
            Dispatcher.BeginInvoke(
                new Action(
                    ShowRoundedVideoFailure));
        }


        private void ShowRoundedVideoFailure()
        {
            VideoSurface.Visibility =
                Visibility.Collapsed;

            MediaStatusText.Text =
                "تعذر تشغيل الفيديو";

            MediaStatusText.Visibility =
                Visibility.Visible;

            PlayButton.Visibility =
                Visibility.Visible;

            if (_selectedItem is not null)
            {
                LoadPoster(
                    _selectedItem);
            }
        }


        private void StopRoundedVideo()
        {
            Interlocked.Increment(
                ref _roundedPlayGeneration);

            StopRoundedVideoInternal();
        }


        private void StopRoundedVideoInternal()
        {
            try
            {
                _roundedMediaPlayer?.Stop();
            }
            catch
            {
            }

            try
            {
                _roundedMedia?.Dispose();
            }
            catch
            {
            }

            _roundedMedia = null;

            ReleaseRoundedVideoBuffer();
        }


        private void ReleaseRoundedVideoBuffer()
        {
            if (_roundedVideoBuffer !=
                IntPtr.Zero)
            {
                try
                {
                    Marshal.FreeHGlobal(
                        _roundedVideoBuffer);
                }
                catch
                {
                }

                _roundedVideoBuffer =
                    IntPtr.Zero;
            }

            _roundedFrameBytes = null;
            _roundedVideoBitmap = null;
            _roundedVideoBrush = null;

            Volatile.Write(
                ref _roundedFramePending,
                0);
        }


        private void MainWindow_RoundedMediaClosed(
            object? sender,
            EventArgs e)
        {
            try
            {
                _roundedMediaPlayer?.Stop();
            }
            catch
            {
            }

            try
            {
                _roundedMedia?.Dispose();

                _roundedMediaPlayer?.Dispose();

                _roundedLibVLC?.Dispose();
            }
            catch
            {
            }

            _roundedMedia = null;
            _roundedMediaPlayer = null;
            _roundedLibVLC = null;

            ReleaseRoundedVideoBuffer();
        }
    }
}
