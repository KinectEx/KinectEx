KinectEx
========
KinectEx is a set of libraries and utilities that add significant capability to Microsoft's version 2 Kinect for Windows sensor and SDK. It is still under active development and contributions / pull requests are encouraged.

## Main Features

### Kinect DVR

DVR as in Digital Video Recorder, though this is a bit of a misnomer. This feature adds the ability to record and playback Body, Color, Depth and Infrared frames without the need for an external service. It is similar to Kinect Studio, but is written in "pure" managed .Net. As such, playback can occur on any platform that supports the .Net platform, including mobile devices (though admittedly the library has not yet been tested on anything less powerful than a Surface Pro--works fine both for recording and playback, BTW).

The other major benifit of this library over Kinect Studio is that it supports resizing and compressing color frames by using real-time JPEG compression and decompression. The result is a **much** smaller file. Because of the smaller size, the performance can be as good or better than storing the frames uncompressed, especially with a fast CPU. If you don't require full fidelity in your color recordings, then this provides a useful alternative to the full-frame, uncompressed recordings offered via Kinect Studio.

Limiting factors currently include the loss of the built-in CoordinateMapper functionality. Adding this capability "offline" remains a high-priority item on my feature backlog. It also uses a proprietary file format, so if you need any of the advanced capabilities of Kinect Studio or you want to use this to record files to be used as input into the Visual Gesture Builder, then you'll be better served to stick with Kinect Studio.

### Body Joint Smoothing

The other major component currently included in the KinectEx library is joint smoothing. This capability was provided natively in later versions of Microsoft's version 1 SDk, but has yet to be added to version 2. The smoothing capability uses a pluggable architecture that can support multiple smoothing algorithms. Currently, the library supports a double-exponential smoothing algorithm similar to the version 1 implementation. It also includes an algorithm that is based on a standard Kalman filter. Both offer similar performance and are highly configurable using simple configuration options. Both integrate jitter reduction, as well as statistical smoothing.

### Other Stuff

There are also some other useful utilities provided, including:

  - A number of Body extension methods that provide useful measurements like angles between bones, distance between joints, etc.
  - Named "bones" (in addition to joints) as natively trackable and measurable entities.
  - Helper classes to make it easy to get WriteableBitmap representations of Color, Depth, and Infrared frames (both live and during replay).
  - A Body extension that makes it simple to get a WriteableBitmap representation of a body/joint skeleton (or overlay one on top of an existing WriteableBitmap).
  - A fully unsealed and serializable interface and class structure that mimics and extends the sealed SDK structs. These provide methods that make it nearly transparent to transition between these structures.
  - Json.NET filters and type converters that make it super-simple to serialize and deserialize bodies to JSON.

## Getting Started

There is not yet a NuGet distribution for KinectEx. Nor yet is there extensive "getting started" documentation. Class library documentation is fairly robust (available at http://kinectex.github.io/KinectEx/html/index.htm). And more documentation is certainly on the backlog, as well.

For now, though, getting started likely means cloning the repository and looking closely at the sample code provided. There are four demo applications--WPF and Windows Store representations of a Recorder application (which includes sample code to demonstrate smoothing) and a Replay application. Documentation in these samples could be better, but hopefully any gaps here will be filled with the self-documentation inherent in code. All needed code is embedded in the *.xaml.cs files.

Note that if Visual Studio doesn't find them automatically, the library and samples will look for the following NuGet packages:

  - Json.NET
  - SharpDX
  - WriteableBitmapEx

## Attribution

Though a ground-up rewrite, the DVR functionality owes significant attribution to David Cathue's excellent KinectToolbox (a toolkit for the version 1 sensor). Since David's off doing awesome things with JavaScript, I figured he'd be unlikely to update this library for version 2, so I did it for him. :)

Appreciation is also due to the primary contributors to three truly amazing open source projects: James Newton-King for Json.NET, Alexandre Mutel for SharpDX (and SharpDoc, which is used to auto-generate the library documentation), and René Schulte for WriteableBitmapEx.

And of course to all who have contributed to this library past and future through pull requests and suggestions.
