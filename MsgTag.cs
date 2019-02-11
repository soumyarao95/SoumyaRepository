using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Kinect = Windows.Kinect;


[Serializable]

    /*public int ColorWidth { get; set; }
    public int ColorHeight { get; set; }
    public ushort[] _DepthData;*/

    public enum MsgTag : byte
    {
        DEPTH,   // Depth 
        COLOR,
        COLORSPACE
        //WIDTH,
        //HEIGHT,
        //LENGTH,
        //COLOR_WIDTH,
        //COLOR_HEIGHT
        //GENERAL,  // Frame width/height
        //DEPTH2,
        //RED,      // Red color channel
        //GREEN,    // Green color channel
        //BLUE      // Blue color channel

    }

