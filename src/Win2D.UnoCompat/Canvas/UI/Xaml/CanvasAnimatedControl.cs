using System.Diagnostics;
using Windows.Foundation;
using Windows.UI.Core;

namespace Microsoft.Graphics.Canvas.UI.Xaml
{
    public delegate void TypedCanvasAnimatedUpdateEventHandler(CanvasAnimatedControl sender, CanvasAnimatedUpdateEventArgs args);
    public delegate void TypedCanvasCreateResourcesEventHandler(object sender, CanvasCreateResourcesEventArgs args);

    public sealed class CanvasAnimatedUpdateEventArgs : EventArgs
    {
        public CanvasAnimatedUpdateEventArgs(long updateCount, TimeSpan elapsedTime)
        {
            UpdateCount = updateCount;
            ElapsedTime = elapsedTime;
        }

        public long UpdateCount { get; }
        public TimeSpan ElapsedTime { get; }

        public CanvasTimingInformation Timing => new()
        {
            UpdateCount = UpdateCount,
            ElapsedTime = ElapsedTime,
            IsRunning = !IsPaused,
        };

        internal bool IsPaused { get; set; }
    }

    public struct CanvasTimingInformation
    {
        public long UpdateCount;
        public TimeSpan ElapsedTime;
        public bool IsRunning;
    }

    public sealed class CanvasCreateResourcesEventArgs : EventArgs
    {
        public CanvasCreateResourcesEventArgs(CanvasDevice device)
        {
            Device = device;
        }

        public CanvasDevice Device { get; }
    }

    public sealed class CanvasAnimatedDrawEventArgs : EventArgs
    {
        public CanvasAnimatedDrawEventArgs(CanvasDrawingSession drawingSession, CanvasTimingInformation timing)
        {
            DrawingSession = drawingSession;
            Timing = timing;
        }

        public CanvasDrawingSession DrawingSession { get; }
        public CanvasTimingInformation Timing { get; }
    }

    public sealed class CanvasAnimatedControl : CanvasControl
    {
        private readonly CanvasAnimatedControlCore _core = new();

        public event TypedCanvasAnimatedUpdateEventHandler? Update;
        public new event TypedCanvasCreateResourcesEventHandler? CreateResources;
        public new event EventHandler<CanvasAnimatedDrawEventArgs>? Draw;
        public event EventHandler<object>? GameLoopStarting;
        public event EventHandler<object>? GameLoopStopped;

        public CanvasAnimatedControl()
        {
            _core.CreateResources += (_, args) => CreateResources?.Invoke(this, args);
            _core.Update += (_, args) => Update?.Invoke(this, args);
        }

        public double FramesPerSecond
        {
            get => _core.FramesPerSecond;
            set => _core.FramesPerSecond = value;
        }

        public bool IsPaused { get => _core.IsPaused; set => _core.IsPaused = value; }

        public float PixelsPerDip { get; set; } = 1f;

        public new Size Size => new(ActualWidth, ActualHeight);

        public TimeSpan TargetElapsedTime => _core.TargetElapsedTime;

        public long UpdateCount => _core.UpdateCount;

        public bool IsFixedTimeStep { get; set; } = true;

        public int SyncInterval { get; set; } = 1;

        public bool HasGameLoopThreadAccess => false;

        public void Tick()
        {
            _core.Tick();
            Invalidate();
        }

        public void ResetElapsedTime()
        {
            _core.ResetElapsedTime();
        }

        public void RunOnGameLoopThreadAsync(Action handler)
        {
            DispatcherQueue.TryEnqueue(() => handler());
        }

        public object CreateCoreIndependentInputSource(object inputDeviceTypes)
        {
            return new object();
        }
    }

    public sealed class CanvasAnimatedControlCore
    {
        private readonly Stopwatch _clock = new();
        private long _updateCount;
        private bool _resourcesCreated;
        private double _framesPerSecond = 60;

        public CanvasAnimatedControlCore()
        {
            _clock.Start();
        }

        public event EventHandler<CanvasAnimatedUpdateEventArgs>? Update;
        public event EventHandler<CanvasCreateResourcesEventArgs>? CreateResources;

        public double FramesPerSecond
        {
            get => _framesPerSecond;
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value));
                _framesPerSecond = value;
            }
        }

        public bool IsPaused { get; set; }
        public TimeSpan TargetElapsedTime => TimeSpan.FromSeconds(1d / FramesPerSecond);
        public long UpdateCount => _updateCount;

        public void Tick()
        {
            if (IsPaused)
                return;

            EnsureResourcesCreated();
            TimeSpan elapsed = _clock.Elapsed;
            _clock.Restart();
            _updateCount++;
            var args = new CanvasAnimatedUpdateEventArgs(_updateCount, elapsed);
            args.IsPaused = IsPaused;
            Update?.Invoke(this, args);
        }

        public void ResetElapsedTime() => _clock.Restart();

        private void EnsureResourcesCreated()
        {
            if (_resourcesCreated)
                return;

            _resourcesCreated = true;
            CreateResources?.Invoke(this, new CanvasCreateResourcesEventArgs(CanvasDevice.GetSharedDevice()));
        }
    }
}
