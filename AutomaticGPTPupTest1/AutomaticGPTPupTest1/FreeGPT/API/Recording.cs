using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Diagnostics;
using System.Text;
namespace AutomaticGPTTest1
{
    public class RecorderSettings
    {
        public string DeviceName { get; set; }
        public int FrameWidth { get; set; }
        public int FrameHeight { get; set; }
        public double Fps { get; set; }
    }

    public class Recorder : IDisposable
    {
        private readonly VideoCaptureAPIs _videoCaptureApi = VideoCaptureAPIs.DSHOW;
        private readonly ManualResetEventSlim _threadStopEvent = new(false);
        private readonly VideoCapture _videoCapture;
        //private VideoWriter _videoWriter;

        private Mat _capturedFrame = new();
        private Thread _captureThread;
        private Thread _writerThread;
        private readonly SemaphoreSlim _asyncLock = new SemaphoreSlim(1, 1);

        private bool IsVideoCaptureValid => _videoCapture is not null && _videoCapture.IsOpened();

        public event EventHandler<Mat> FrameCaptured;

        public Recorder(int deviceIndex, int frameWidth, int frameHeight, double fps)
        {
            _videoCapture = VideoCapture.FromCamera(deviceIndex, _videoCaptureApi);
            _videoCapture.Open(deviceIndex, _videoCaptureApi);

            if (frameWidth != -1)
                _videoCapture.FrameWidth = frameWidth;
            if (frameHeight != -1)
                _videoCapture.FrameHeight = frameHeight;

            Debug.WriteLine(_videoCapture.FrameWidth);
            Debug.WriteLine(_videoCapture.FrameHeight);

            _videoCapture.Fps = fps;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        ~Recorder()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                StopRecording();

                _videoCapture?.Release();
                _videoCapture?.Dispose();
            }
        }

        public void StartRecording()
        {
            if (_writerThread is not null)
                return;

            if (!IsVideoCaptureValid)
                ThrowHelper.ThrowVideoCaptureNotReadyException();

            //_videoWriter = new VideoWriter(path, FourCC.XVID, _videoCapture.Fps, new Size(_videoCapture.FrameWidth, _videoCapture.FrameHeight));

            _threadStopEvent.Reset();

            _captureThread = new Thread(CaptureFrameLoop);
            _captureThread.Start();

            _writerThread = new Thread(AddCameraFrameToRecordingThread);
            _writerThread.Start();
        }

        public void StopRecording()
        {
            _threadStopEvent.Set();

            _writerThread?.Join();
            _writerThread = null;

            _captureThread?.Join();
            _captureThread = null;

            _threadStopEvent.Reset();

            //_videoWriter?.Release();
            //_videoWriter?.Dispose();
            //_videoWriter = null;
        }

        private void CaptureFrameLoop()
        {
            while (!_threadStopEvent.Wait(0))
            {
                if (_asyncLock.CurrentCount == 0)
                    _videoCapture.Read(_capturedFrame);
            }
        }

        private void AddCameraFrameToRecordingThread()
        {
            var waitTimeBetweenFrames = 1_000 / _videoCapture.Fps;
            var lastWrite = DateTime.Now;

            while (!_threadStopEvent.Wait(0))
            {
                if (DateTime.Now.Subtract(lastWrite).TotalMilliseconds < waitTimeBetweenFrames)
                    continue;
                lastWrite = DateTime.Now;
                try
                {
                    if (_asyncLock.CurrentCount == 0)
                        continue;
                    _asyncLock.Wait();
                    if (FrameCaptured != null)
                        FrameCaptured?.Invoke(null, _capturedFrame);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                finally
                {
                    _asyncLock.Release();
                }
            }
        }

        public Bitmap GetFrameBitmap()
        {
            if (!IsVideoCaptureValid)
                ThrowHelper.ThrowVideoCaptureNotReadyException();

            using var frame = new Mat();
            return !_videoCapture.Read(frame) ? null : frame.ToBitmap();
        }
    }

    public class DeviceNotFoundException : Exception
    {
        public string DeviceName { get; }

        /// <inheritdoc />
        public DeviceNotFoundException()
        {
        }

        /// <inheritdoc />
        public DeviceNotFoundException(string message) : base(message)
        {
        }

        /// <inheritdoc />
        public DeviceNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public DeviceNotFoundException(string message, string deviceName) : base(message)
        {
            DeviceName = deviceName;
        }

        public DeviceNotFoundException(string message, Exception innerException, string deviceName) : base(message, innerException)
        {
            DeviceName = deviceName;
        }
    }

    public class VideoCaptureNotReadyException : Exception
    {
        /// <inheritdoc />
        public VideoCaptureNotReadyException()
        {
        }

        /// <inheritdoc />
        public VideoCaptureNotReadyException(string message) : base(message)
        {
        }

        /// <inheritdoc />
        public VideoCaptureNotReadyException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    internal static class ThrowHelper
    {
        public static void ThrowDeviceNotFoundException(string deviceName)
        {
            var sb = new StringBuilder($"Das Gerät mit dem Namen '{deviceName}' konnte nicht gefunden werden. Die folgenden Geräte stehen zur Verfügung: ");
            throw new DeviceNotFoundException(sb.ToString(), null, deviceName);
        }

        public static void ThrowVideoCaptureNotReadyException()
        {
            throw new VideoCaptureNotReadyException();
        }
    }
}