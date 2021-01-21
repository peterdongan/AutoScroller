// Copyright (c) Peter Dongan. All rights reserved.
// Project: https://github.com/peterdongan/AutoScroller
// Licensed under the MIT licence. https://opensource.org/licenses/MIT

using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.Foundation;
using Windows.Devices.Input;

namespace AutoScrolling
{
    /// <summary>
    /// Enables autoscrolling when you drag something beyond the edge of a scrollviewer.
    /// </summary>
    public class AutoScroller
    {
        private readonly ScrollViewer _targetScrollViewer;

        private DispatcherTimer _scrollingTimer;
        private bool _isToScrollLeft;
        private bool _isToScrollUp;
        private bool _isToScrollDown;
        private bool _isToScrollRight;
        private bool _isAutoScrollOn;

        /* Event handlers. They are added via AddHandler which allows them to handle events where handled was set to true.
         * Set them to null when not applied. Check if they are null to see if the handler is applied.*/
        private PointerEventHandler _targetScrollViewerPointerPressedEventHandler;
        private PointerEventHandler _targetScrollViewerPointerMovedEventHandler;
        private PointerEventHandler _targetScrollViewerPointerReleasedOrCancelledEventHandler;

        /// <summary>
        /// If set to true, the scrollviewer will capture the pointer when it is pressed.
        /// This needs to be set to true if the pointer is not already being captured by the element being interacted with within the ScrollViewer.
        /// It should be set to false if the pointer is being captured by elements in the ScrollViewer.
        /// The default is false.
        /// </summary>
        public bool IsPointerCapturedOnPress { get; set; }

        /// <summary>
        /// If set to false, autoscrolling will not be applied for touch interactions.
        /// Only set this to true if you are also preventing the default touch/drag behaviour, which is to pan the scrollviewer.
        /// </summary>
        public bool AreTouchEventsHandled { get; set; }

        /// <summary>
        /// The number of pixels that will be scrolled per 100 milliseconds when scrolling is activated. Default is 5.
        /// </summary>
        public double ScrollPixelsPerTick { get; set; }

        /// <summary>
        /// Will the target scrollviewer scroll automatically when an object is dragged to its boundary. 
        /// Setting this to true applies event handlers to the target scrollviewer. Set it to false to remove the event handlers.
        /// </summary>
        public bool IsAutoScrollingEnabled
        {
            get
            {
                return _isAutoScrollOn;
            }
            set
            {
                if (value != _isAutoScrollOn)
                {
                    if (value)
                    {
                        _targetScrollViewerPointerPressedEventHandler = new PointerEventHandler(TargetScrollviewer_PointerPressed);
                        _targetScrollViewer.AddHandler(UIElement.PointerPressedEvent, _targetScrollViewerPointerPressedEventHandler, true);
                    }
                    else
                    {
                        StopScrolling();
                        RemoveAllSubscriptions();
                    }
                    _isAutoScrollOn = value;
                }
            }
        }

        /// <summary>
        /// Enables autoscrolling when you drag something beyond the edge of a scrollviewer.
        /// </summary>
        /// <param name="targetScrollViewer">The ScrollViewer to use automatic scrolling.</param>
        /// <param name="handleTouchEvents">If set to false, autoscrolling will not be applied for touch interactions.  Only set this to true if you are also preventing the default touch/drag behaviour, which is to pan the scrollviewer.</param>
        /// <param name="scrollPixelsPerTick">Scroll rate, specified in the number of pixels per tenth of a second. Default is 20.</param>
        public AutoScroller(ScrollViewer targetScrollViewer, bool handleTouchEvents = false, double scrollPixelsPerTick = 20)
        {
            IsPointerCapturedOnPress = false;
            AreTouchEventsHandled = handleTouchEvents;
            _targetScrollViewer = targetScrollViewer;
            ScrollPixelsPerTick = scrollPixelsPerTick;
        }

        private void RemoveAllSubscriptions()
        {
            if (_targetScrollViewerPointerPressedEventHandler != null)
            {
                _targetScrollViewer.RemoveHandler(UIElement.PointerPressedEvent, _targetScrollViewerPointerPressedEventHandler);
                _targetScrollViewerPointerPressedEventHandler = null;
            }
            if (_targetScrollViewerPointerReleasedOrCancelledEventHandler != null)
            {
                _targetScrollViewer.RemoveHandler(UIElement.PointerReleasedEvent, _targetScrollViewerPointerReleasedOrCancelledEventHandler);
                _targetScrollViewer.RemoveHandler(UIElement.PointerCanceledEvent, _targetScrollViewerPointerReleasedOrCancelledEventHandler);
                _targetScrollViewerPointerPressedEventHandler = null;
            }
            if (_targetScrollViewerPointerMovedEventHandler != null)
            {
                _targetScrollViewer.RemoveHandler(UIElement.PointerMovedEvent, _targetScrollViewerPointerMovedEventHandler);
                _targetScrollViewerPointerMovedEventHandler = null;
            }
        }

        private void StartScrollingLeft()
        {
            if (!_isTimerOn)
                StartTimer();
            _isToScrollLeft = true;
        }

        private void StopScrollingLeft()
        {
            if (_isTimerOn)
                StopTimer();
            _isToScrollLeft = false;
        }

        private void StartScrollingRight()
        {
            _isToScrollRight = true;
            if (!_isTimerOn)
                StartTimer();
        }

        private void StopScrollingRight()
        {
            _isToScrollRight = false;
        }

        private void StartScrollingUp()
        {
            _isToScrollUp = true;
            if (!_isTimerOn)
                StartTimer();
        }

        private void StopScrollingUp()
        {
            _isToScrollUp = false;
        }


        private void StartScrollingDown()
        {
            _isToScrollDown = true;
            if (!_isTimerOn)
                StartTimer();
        }

        private void StopScrollingDown()
        {
            _isToScrollDown = false;
        }

        private void StopScrolling()
        {
            if (_isTimerOn)
                StopTimer();
            _isToScrollDown = _isToScrollLeft = _isToScrollRight = _isToScrollUp = false;
        }

        private void ScrollingTimer_Tick(object o, object sender)
        {
            double newHorizontalOffset = _targetScrollViewer.HorizontalOffset;
            double newVerticalOffset = _targetScrollViewer.VerticalOffset;

            if (_isToScrollRight)
            {
                if (_targetScrollViewer.HorizontalOffset == _targetScrollViewer.ScrollableWidth)
                {
                    StopScrollingRight();
                }
                else
                {
                    newHorizontalOffset += ScrollPixelsPerTick;
                }
            }
            else if (_isToScrollLeft)
            {
                if (_targetScrollViewer.HorizontalOffset == 0)
                {
                    StopScrollingLeft();
                }
                else
                {
                    newHorizontalOffset -= ScrollPixelsPerTick;
                }
            }

            if (_isToScrollDown)
            {
                if (_targetScrollViewer.VerticalOffset == _targetScrollViewer.ScrollableHeight)
                {
                    StopScrollingDown();
                }
                else
                {
                    newVerticalOffset += ScrollPixelsPerTick;
                }
            }
            else if (_isToScrollUp)
            {
                if (_targetScrollViewer.VerticalOffset == 0)
                {
                    StopScrollingUp();
                }
                else
                {
                    newVerticalOffset -= ScrollPixelsPerTick;
                }
            }

            _targetScrollViewer.ChangeView(newHorizontalOffset, newVerticalOffset, _targetScrollViewer.ZoomFactor);
        }

        private void TargetScrollViewer_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            Point pos = e.GetCurrentPoint(_targetScrollViewer).Position;

            if (pos.X < 0 && _targetScrollViewer.HorizontalOffset > 0)
                StartScrollingLeft();
            else if (_isToScrollLeft == true)
                StopScrollingLeft();

            if (pos.Y < 0 && _targetScrollViewer.VerticalOffset > 0)
                StartScrollingUp();
            else if (_isToScrollUp == true)
                StopScrollingUp();

            if (pos.X > ((double)_targetScrollViewer.ActualWidth))
                StartScrollingRight();
            else if (_isToScrollRight == true)
                StopScrollingRight();

            if (pos.Y > ((double)_targetScrollViewer.ActualHeight))
                StartScrollingDown();
            else if (_isToScrollDown == true)
                StopScrollingDown();
        }

        private void TargetScrollviewer_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _targetScrollViewer.RemoveHandler(UIElement.PointerPressedEvent, _targetScrollViewerPointerPressedEventHandler); //Ignore pointers beyond the first for multi-touch interactions to avoid having to keep track of multiple handlers of each type.
            if (e.Pointer.PointerDeviceType != PointerDeviceType.Touch)
            {

                if (IsPointerCapturedOnPress)
                {
                    _targetScrollViewer.CapturePointer(e.Pointer);
                }
                _targetScrollViewerPointerReleasedOrCancelledEventHandler = new PointerEventHandler(TargetScrollViewer_PointerReleasedOrCancelled);
                _targetScrollViewer.AddHandler(UIElement.PointerReleasedEvent, _targetScrollViewerPointerReleasedOrCancelledEventHandler, true);
                _targetScrollViewer.AddHandler(UIElement.PointerCanceledEvent, _targetScrollViewerPointerReleasedOrCancelledEventHandler, true);
                _targetScrollViewerPointerMovedEventHandler = new PointerEventHandler(TargetScrollViewer_PointerMoved);
                _targetScrollViewer.AddHandler(UIElement.PointerMovedEvent, _targetScrollViewerPointerMovedEventHandler, true);
            }
        }

        private void TargetScrollViewer_PointerReleasedOrCancelled(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Touch)
                UIElement.TryStartDirectManipulation(e.Pointer);
            if (IsPointerCapturedOnPress)
            {
                _targetScrollViewer.ReleasePointerCapture(e.Pointer);
            }
            _targetScrollViewer.RemoveHandler(UIElement.PointerReleasedEvent, _targetScrollViewerPointerReleasedOrCancelledEventHandler);
            _targetScrollViewer.RemoveHandler(UIElement.PointerCanceledEvent, _targetScrollViewerPointerReleasedOrCancelledEventHandler);
            if (_targetScrollViewerPointerMovedEventHandler != null)
            {
                _targetScrollViewer.RemoveHandler(UIElement.PointerMovedEvent, _targetScrollViewerPointerMovedEventHandler);
                _targetScrollViewerPointerMovedEventHandler = null;
            }
            _targetScrollViewerPointerMovedEventHandler = null;
            _targetScrollViewerPointerReleasedOrCancelledEventHandler = null;

            StopScrolling();
            _targetScrollViewer.AddHandler(UIElement.PointerPressedEvent, _targetScrollViewerPointerPressedEventHandler, true);
        }

        private bool _isTimerOn = false;

        private void StartTimer()
        {
            _scrollingTimer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 0, 0, 100) // 100 Milliseconds 
            };
            _scrollingTimer.Tick += ScrollingTimer_Tick;
            _scrollingTimer.Start();
            _isTimerOn = true;
        }

        private void StopTimer()
        {
            if (_scrollingTimer != null)
            {
                _scrollingTimer.Stop();
                _scrollingTimer.Tick -= ScrollingTimer_Tick;
                _isTimerOn = false;
            }
        }
    }
}