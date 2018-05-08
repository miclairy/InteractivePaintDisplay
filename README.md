#Interactive Paint Display #
This is an interactive public display for a cafe using the kinect. It is essitentually a paint application but 
if you draw on a face then the drawing will move with the face.

Requires the kinect and kinect SDK
Built in Visual Studio
Please excuse the messiness of this code. Beware there is a lot of dead code in here

##Controls:##
* Item 1
* Item 2
  * Item 2a
  * Item 2b
* Wave to get a pen
* Push hand forewards to start drawing
* Pull hand back to stop drawing
* Drop hand when finished
* Move hand about after getting a pen to draw
* Change colour by placing hands close together and rotating the non pen hand around the other
* Switch hand used for drawing by waving other hand

##Features:##
* Works with two people drawing at once.
* If you draw on another person face the drawing will move with them
* Adapted control algorithm so you can easily draw from 0.5m to 4.65m away from the kinect.
