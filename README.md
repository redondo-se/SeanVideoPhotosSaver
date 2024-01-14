# SeanVideoPhotosSaver
A Windows screen saver that displays photos and videos

# More Info

This Windows screensaver shows photos and videos from the folder of your choice in 4 different themes:

 - Full Screen
 - Centered
 - Two Diagonal
 - Randomly placed with a random angle

## Requirements

The folder containing photos and videos can be either a local folder or a network CIFS/Samba share.
The screensaver is written in WPF and requires .Net 4.6.1 to be installed.

## Installation

Extract the contents of the archive to a folder. The files must remain in this location. Right click the .scr file and choose install.
Go to the Windows screen saver configuration, choose SeanVideoPhotosSaver, and then click Settings to specify the folder to use.

## Additional Notes

 - The screen saver rotates between themes each time it launches
 - The list of photos which have not yet been displayed are stored in:
%localappdata%\SeanVideoPhotosSaver
 - The screen saver randomly chooses photos and videos to display, but won't repeat until all have been displayed
 - You might need to disable Media Foundation for video files to allow 4K and higher resolution videos to play smooth. This can be done via the registry or with a tool such as [Win7DSFilterTweaker](https://www.videohelp.com/software/Preferred-Filter-Tweaker).
