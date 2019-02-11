// DataSender.cs
using HoloToolkit.Sharing;
using HoloToolkit.Unity;
using System.Collections.Generic;
using UnityEngine;
using System;
using Windows.Kinect;
using System.Threading;

/// <summary>
///  Broadcasts depth data and color data over the network.
///  Requires CustomMessagesPointCloud.cs
/// </summary>
public class DataSender : Singleton<DataSender>
{
    public GameObject MultiSourceManager;
    private MultiSourceManager _MultiManager;

    public GameObject DepthSourceView;
    private DepthSourceView _SourceView;

    //public GameObject DataManager;
    //private ServerDataManager _DataManager;

    private ushort[] _DepthData;
    private byte[] _ColorData;
    private Texture2D _ColorTexture;
    private ColorSpacePoint[] _ColorSpace;
    public int Counter;

    public float timeToGo;
    public int ColorWidth = 0;
    public int ColorHeight = 0;

    void Start()
    {
        //timeToGo = Time.fixedTime + 0.01f;
        //InvokeRepeating("Update", 0.1f, 2.0f);
        Counter = 0;
    }

    void Update()
    {
        

            if (MultiSourceManager == null)
            {
                return;
            }

            _MultiManager = MultiSourceManager.GetComponent<MultiSourceManager>();
            if (_MultiManager == null)
            {
                return;
            }

            if (DepthSourceView == null)
            {
                return;
            }

            _SourceView = DepthSourceView.GetComponent<DepthSourceView>();
            if (_SourceView == null)
            {
                return;
            }

            if (_MultiManager.isReaderClosed())
            {
                return;
            }

            _DepthData = _MultiManager.GetDepthData();
            if (_DepthData == null)
            {
                return;
            }


            _ColorSpace = _SourceView.GetColorSpace();
            if (_ColorSpace == null)
            {
                return;
            }

            _ColorData = _MultiManager.GetColorData();
            if (_ColorData == null)
            {
                return;
            }

            _ColorTexture = _MultiManager.GetColorTexture();
            if (_ColorTexture == null)
            {
                return;
            }

            ColorWidth = _MultiManager.ColorWidth;
            if (ColorWidth == 0)
            {
                return;
            }

            ColorHeight = _MultiManager.ColorHeight;
            if (ColorHeight == 0)
            {
                return;
            }

        //CustomMessages2.Instance.SendGeneralData(_MultiManager.ColorWidth, _MultiManager.ColorHeight);

        //CustomMessages2.Instance.SendWidth(_MultiManager.ColorWidth);
        //CustomMessages2.Instance.SendHeight(_MultiManager.ColorHeight);
        //Debug.Log("_DepthData.Length:" + _DepthData.Length);
        //CustomMessages2.Instance.Send(MsgTag.LENGTH, _DepthData.Length);
        //CustomMessages2.Instance.Send(MsgTag.COLOR_WIDTH, ColorWidth);
        //CustomMessages2.Instance.Send(MsgTag.COLOR_HEIGHT, ColorHeight);
        //Debug.Log("_colorspace.Length in sender is/...........:" + _ColorSpace.Length);

        //if (Time.fixedTime >= timeToGo)
        //Debug.Log("counter before if is: " + Counter);
        if (Counter % 60 == 0)
        {
            //Debug.Log("counter in if is: " + Counter);
            CustomMessages2.Instance.SendDepthData(MsgTag.DEPTH, _DepthData);
            CustomMessages2.Instance.SendColorData(MsgTag.COLOR, _ColorData);
            CustomMessages2.Instance.SendColorSpace(MsgTag.COLORSPACE, _ColorSpace);
            //timeToGo = Time.fixedTime + 0.01f;


            //Thread.Sleep(500);


            //Debug.Log("_Daepthdata in sender is" + _DepthData);

            /*
            Debug.Log("DSPWidth: " + _MultiManager.DSPWidth);
            Debug.Log("DSPHeight: " + _MultiManager.DSPHeight);*/

            //CustomMessagesPointCloud.Instance.SendColorData(_ColorData);

            
        }
        Counter++;
    }
}