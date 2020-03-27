# AutoScroller
Auto-scrolls a UWP ScrollViewer when something is dragged against its edge. 

## Usage
Instantiate the AutoScroller with the target ScrollViewer. Set IsAutoScrollingEnabled to true to enable autoscrolling.

Set IsAutoScrollingEnabled to false before removing references to the AutoScroller. This will remove any event handlers that it set on the ScrollViewer.

Set AreTouchEventsHandled to true if you want it to respond to touch events. The default is false. This is because the default touch behaviour in a ScrollViewer is to pan, which conflicts with the style of autoscrolling applied by the AutoScroller. (This default behaviour should be disabled before setting AreTouchEventsHandled to true.)

Set IsPointerCapturedOnPress to true if elements in the ScrollViewer don't already capture the pointer. This is false by default. Setting this to true while other pointer captures are active might interfere with the existing captures. This scenario has not been tested.

Change the speed of scrolling by setting ScrollPixelsPerTick. It is the number of pixels scrolled per tenth of a second when autoscrolling is occurring. The default value is 20.
