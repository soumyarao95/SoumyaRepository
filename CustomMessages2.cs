/*
 * CustomMessagesPointCloud.cs
 *
 * Allows for sending kinect depth and RGBA for point 
 * cloud reconstruction.
 */

using HoloToolkit.Sharing;
using HoloToolkit.Unity;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;
using System;
using System.Collections.Generic;

namespace HoloToolkit.Sharing
{

    public class CustomMessages2 : Singleton<CustomMessages2>
    {

        private const int BYTES_PER_PIXEL = 4;
        //private const int MAX_PACKET_SIZE = 60000;

        // Message enum containing information bytes to share
        // The first message type has to start with UserMessageIDStart
        // so as not to conflict with HoloToolkit internal messages
        public enum TestMessageID : byte
        {
            StartID = MessageID.UserMessageIDStart,
            Max
        }

        public enum UserMessageChannels
        {
            Anchors = MessageChannel.UserMessageChannelStart,
        }

        // Cache the local user's ID to use when sending messages
        public long localUserID
        {
            get; set;
        }

        // Broadcasted message must have an identifying ID


        public delegate void MessageCallback(NetworkInMessage msg);

        private Dictionary<TestMessageID, MessageCallback> _MessageHandlers = new Dictionary<TestMessageID, MessageCallback>();

        public Dictionary<TestMessageID, MessageCallback> MessageHandlers
        {
            get
            {
                return _MessageHandlers;
            }
        }

        // Helper object that we use to route incoming message callbacks to the member
        // functions of this class
        NetworkConnectionAdapter connectionAdapter;

        // Cache the connection object for the sharing service
        NetworkConnection serverConnection;

        void Start()
        {
            InitializeMessageHandlers();
        }

        void InitializeMessageHandlers()
        {

            SharingStage sharingStage = SharingStage.Instance;

            if (sharingStage == null)
            {
                Debug.Log("Cannot Initialize CustomMessages. No SharingStage instance found.");
                return;
            }

            serverConnection = sharingStage.Manager.GetServerConnection();
            if (serverConnection == null)
            {
                Debug.Log("Cannot initialize CustomMessages. Cannot get a server connection.");
                return;
            }

            connectionAdapter = new NetworkConnectionAdapter();
            connectionAdapter.MessageReceivedCallback += OnMessageReceived;

            // Cache the local user ID
            this.localUserID = SharingStage.Instance.Manager.GetLocalUser().GetID();
            /////////////////////////////////////////////////////////////////////////////////////////
            // for (byte index = (byte)TestMessageID.BodyData; index < (byte)TestMessageID.Max; index++)
            for (byte index = (byte)TestMessageID.StartID; index < (byte)TestMessageID.Max; index++)
            {

                if (MessageHandlers.ContainsKey((TestMessageID)index) == false)
                {
                    MessageHandlers.Add((TestMessageID)index, null);
                }

                serverConnection.AddListener(index, connectionAdapter);
            }
        }

        private NetworkOutMessage CreateMessage(byte MessageType)
        {
            NetworkOutMessage msg = serverConnection.CreateMessage(MessageType);
            msg.Write(MessageType);
            // Add the local userID so that the remote clients know whose message they are receiving
            //msg.Write(localUserID); // TODO Is this necessary if using one hololens?
            return msg;
        }


        public void SendDepthData(MsgTag tag, ushort[] depthData)
        {

            // If we are connected to a session, broadcast our info
            if (this.serverConnection != null && this.serverConnection.IsConnected())
            {

                /*ushort[] source = new ushort[217088];
                source = depthData;
                byte[] target = new byte[source.Length * 2];
                Buffer.BlockCopy(source, 0, target, 0, source.Length * 2);
                uint len = (uint)target.Length;*/


                //int length = 217088;
                //int offset = 0;
                //if ((2 * length) > MAX_PACKET_SIZE) // Will need to divide depth data into at least two messages
                //{
                //    offset = (int)Math.Ceiling((float)(length) / 2f);
                //}

                // Start first message
                // 1) Start with UserMessageIDStart
                NetworkOutMessage msg = CreateMessage((int)TestMessageID.StartID);

                // 2) Add message type ID
                msg.Write((byte)tag);

                //// 3) Add message length
                //if (offset == 0)  // will just send one depth message
                //{
                //    msg1.Write((int)length);
                //} else    // will send two depth messages
                //{
                //    msg1.Write((int)offset);
                //}

                // 3) Add message length
                //msg.Write((int)length);

                // 4) Write message
                int length = depthData.Length;
                for (int i = 0; i < length; i++)
                {
                    msg.Write((short)depthData[i]);
                }


                //msg.WriteArray(target, len);

                //Debug.Log("msg.write in cs2 is:" + msg1.ToString());

                // Send the message as a broadcast
                this.serverConnection.Broadcast(
                    msg,
                    MessagePriority.Immediate,
                    MessageReliability.UnreliableSequenced,
                    MessageChannel.Avatar);

                //// Send second message
                //if (offset != 0)
                //{
                //    // Start second message
                //    // 1) Start with UserMessageIDStart
                //    NetworkOutMessage msg2 = CreateMessage((int)TestMessageID.StartID);
                //    // 2) Add message type ID
                //    msg2.Write((byte)MsgID.DEPTH2);
                //    // 3) Add message length
                //    msg2.Write((int)(length - offset));
                //    // 4) Add message
                //    for (int i = offset; i < length; i++)
                //    {
                //        msg2.Write((short)depthData[i]);
                //    }
                //    // Send the message as a broadcast
                //    this.serverConnection.Broadcast(
                //        msg2,
                //        MessagePriority.Immediate,
                //        MessageReliability.UnreliableSequenced,
                //        MessageChannel.Avatar);
                //}

            }

        }

        public void SendColorData(MsgTag tag, byte[] colorData)
        {
            // If we are connected to a session, broadcast our info
            if (this.serverConnection != null && this.serverConnection.IsConnected())
            {
                // RED message
                NetworkOutMessage msg = CreateMessage((int)TestMessageID.StartID);
                msg.Write((byte)tag);

                // 4) Add message
                uint len = (uint)colorData.Length;
                //Debug.Log("length of color data in cs2 is :" + len);
                msg.WriteArray(colorData, len);
                //Debug.Log("MSG.WRITE ARRAY IN CS2" + msg.ToString());
                /*for (int i = 0; i < colorData.Length; i += BYTES_PER_PIXEL)
                {
                   
                }*/

                // Send the message as a broadcast
                this.serverConnection.Broadcast(
                    msg,
                    MessagePriority.Immediate,
                    MessageReliability.UnreliableSequenced,
                    MessageChannel.Avatar);
            }
        }

        public void SendColorSpace(MsgTag tag, ColorSpacePoint[] ColorSpace)
        {
            // If we are connected to a session, broadcast our info
            if (this.serverConnection != null && this.serverConnection.IsConnected())
            {
                // RED message
                NetworkOutMessage msg = CreateMessage((int)TestMessageID.StartID);
                msg.Write((byte)tag);

                // 4) Add message
                int length = ColorSpace.Length;
                //Debug.Log("colorspace length in CS2 is.........................: " + length);
                //byte
                for (int i = 0; i < length; i++)
                {
                    msg.Write(ColorSpace[i].X);
                    //Debug.Log("COLOR SPACE XXXXXXXXXXX IN CS2 IS: " + msg.ToString());
                    msg.Write(ColorSpace[i].Y);
                    //Debug.Log("COLOR SPACE YYYYYYYYYYY IN CS2 IS: " + msg.ToString());
                }

                // Send the message as a broadcast
                this.serverConnection.Broadcast(
                    msg,
                    MessagePriority.Immediate,
                    MessageReliability.UnreliableSequenced,
                    MessageChannel.Avatar);
            }
        }

        /// <summary>
        /// Sends the Kinect processed RGB32. 
        /// Ignores the alpha channel.
        /// </summary>
        /// <param name="colorData"> array of color data in RGB32 </param>



        /// <summary>
        /// Sends the Kinect processed frame height and width.
        /// </summary>
        /// <param name="width"> the frame width </param>
        /// <param name="height"> the frame height </param>
        /*public void SendGeneralData(int width, int height)
        {
            // If we are connected to a session, broadcast our info
            if (this.serverConnection != null && this.serverConnection.IsConnected())
            {
                // Create an outgoing network message to contain all the info we want to send
                // 1) Start with UserMessageIDStart
                NetworkOutMessage msg = CreateMessage((int)TestMessageID.StartID);

                // 2) Add message type ID
                msg.Write((byte)MsgID.GENERAL);

                // 3) Add message length
                //msg.Write((int)2);

                // 4) Add message
                msg.Write((int)width);
                msg.Write((int)height);

                // Send the message as a broadcast
                this.serverConnection.Broadcast(
                    msg,
                    MessagePriority.Immediate,
                    MessageReliability.UnreliableSequenced,
                    MessageChannel.Avatar);
            }
        }

        public void SendWidth(MsgTag msgtag, int width)
        {
            // If we are connected to a session, broadcast our info
            if (this.serverConnection != null && this.serverConnection.IsConnected())
            {
                // Create an outgoing network message to contain all the info we want to send
                // 1) Start with UserMessageIDStart
                NetworkOutMessage msg = CreateMessage((int)TestMessageID.StartID);

                // 2) Add message type ID
                msg.Write((byte)msgtag);

                // 3) Add message length
                //msg.Write((int)2);

                // 4) Add message
                msg.Write((int)width);
                Debug.Log("width is: " + msg.ToString());

                // Send the message as a broadcast
                this.serverConnection.Broadcast(
                    msg,
                    MessagePriority.Immediate,
                    MessageReliability.UnreliableSequenced,
                    MessageChannel.Avatar);
            }
        }

        public void SendHeight(int height)
        {
            // If we are connected to a session, broadcast our info
            if (this.serverConnection != null && this.serverConnection.IsConnected())
            {
                // Create an outgoing network message to contain all the info we want to send
                // 1) Start with UserMessageIDStart
                NetworkOutMessage msg = CreateMessage((int)TestMessageID.StartID);

                // 2) Add message type ID
                msg.Write((byte)MsgTag.HEIGHT);

                // 3) Add message length
                //msg.Write((int)2);

                // 4) Add message
                msg.Write((int)height);
                Debug.Log("height is: " + msg.ToString());
                // Send the message as a broadcast
                this.serverConnection.Broadcast(
                    msg,
                    MessagePriority.Immediate,
                    MessageReliability.UnreliableSequenced,
                    MessageChannel.Avatar);
            }
        }

        public void Send(MsgTag tag, int var)
        {
            // If we are connected to a session, broadcast our info
            if (this.serverConnection != null && this.serverConnection.IsConnected())
            {
                // Create an outgoing network message to contain all the info we want to send
                // 1) Start with UserMessageIDStart
                NetworkOutMessage msg = CreateMessage((int)TestMessageID.StartID);

                // 2) Add message type ID
                msg.Write((byte)tag);

                // 3) Add message length
                //msg.Write((int)2);

                // 4) Add message
                msg.Write(var);
                Debug.Log(tag.ToString() + " is: " + msg.ToString());
                // Send the message as a broadcast
                this.serverConnection.Broadcast(
                    msg,
                    MessagePriority.Immediate,
                    MessageReliability.UnreliableSequenced,
                    MessageChannel.Avatar);
            }
        }

        /// <summary>
        /// Sends the Kinect processed depth frame.
        /// Since depth is floating point numbers, the message
        /// needs to be divided into two.
        /// </summary>
        /// <param name="depthData"> array of depth data </param>

        public void SendColorData(byte[] colorData)
     {
     // If we are connected to a session, broadcast our info
     if (this.serverConnection != null && this.serverConnection.IsConnected())
     {
         // RED message
         NetworkOutMessage rMsg = CreateMessage((int)TestMessageID.StartID);
         rMsg.Write((byte)MsgTag.RED);
         rMsg.Write(colorData.Length / BYTES_PER_PIXEL);

         // GREEN message
         NetworkOutMessage gMsg = CreateMessage((int)TestMessageID.StartID);
         gMsg.Write((byte)MsgTag.GREEN);
         gMsg.Write(colorData.Length / BYTES_PER_PIXEL);

         // BLUE message
         NetworkOutMessage bMsg = CreateMessage((int)TestMessageID.StartID);
         bMsg.Write((byte)MsgTag.BLUE);
         bMsg.Write(colorData.Length / BYTES_PER_PIXEL);

         // 4) Add message
         for (int i = 0; i < colorData.Length; i += BYTES_PER_PIXEL)
         {
             rMsg.Write(colorData[i + 0]);
             gMsg.Write(colorData[i + 1]);
             bMsg.Write(colorData[i + 2]);
             // ignore alpha channel
         }

         // Send the message as a broadcast
         this.serverConnection.Broadcast(
             rMsg,
             MessagePriority.Immediate,
             MessageReliability.UnreliableSequenced,
             MessageChannel.Avatar);
         this.serverConnection.Broadcast(
             gMsg,
             MessagePriority.Immediate,
             MessageReliability.UnreliableSequenced,
             MessageChannel.Avatar);
         this.serverConnection.Broadcast(
             bMsg,
             MessagePriority.Immediate,
             MessageReliability.UnreliableSequenced,
             MessageChannel.Avatar);
     }
 }

     */
        void OnDestroy()
        {

            if (this.serverConnection != null)
            {

                for (byte index = (byte)TestMessageID.StartID; index < (byte)TestMessageID.Max; index++)
                {
                    this.serverConnection.RemoveListener(index, this.connectionAdapter);
                }
                this.connectionAdapter.MessageReceivedCallback -= OnMessageReceived;
            }
        }

        void OnMessageReceived(NetworkConnection connection, NetworkInMessage msg)
        {

            byte messageType = msg.ReadByte();
            MessageCallback messageHandler = MessageHandlers[(TestMessageID)messageType];
            if (messageHandler != null)
            {
                messageHandler(msg);
            }
        }

        /*
        #region HelperFunctionsForWriting

        void AppendVector3(NetworkOutMessage msg, Vector3 vector)
        {
            msg.Write(vector.x);
            msg.Write(vector.y);
            msg.Write(vector.z);
        }

        #endregion HelperFunctionsForWriting

        #region HelperFunctionsForReading

        public Vector3 ReadVector3(NetworkInMessage msg)
        {
            return new Vector3(msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat());
        }

        #endregion HelperFunctionsForReading
        */

        void appendWidth(NetworkOutMessage msg, int width)
        {
            msg.Write(width);
        }

        void appendHeight(NetworkOutMessage msg, int height)
        {
            msg.Write(height);
        }

        void appendDepth(NetworkOutMessage msg, int[] depth)
        {
            int arrayLength = depth.Length;
            for (int i = 0; i < arrayLength; i++)
            {
                msg.Write(depth[i]);
            }

        }
    }
}
