//using System;
//using System.IO;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class PacketReader
//{
//    public class RingBuffer
//    {
//        private byte[] _buffer;
//        private int _bufferlength;

//        private int _head;
//        private int _tail;

//        public RingBuffer(int count)
//        {
//            _buffer = new byte[count];
//            _bufferlength = count;

//            _head = 0;
//            _tail = 0;
//        }

//        public int Space() { return _bufferlength - Size(); }
//        public int Size()
//        {
//            if (_head <= _tail)
//            {
//                return _tail - _head;
//            }
//            else
//            {
//                int p1 = _bufferlength - _head;
//                int p2 = _tail;

//                return p1 + p2;
//            }
//        }

//        public void Reset() { _head = 0; _tail = 0; }
//        public int Read(byte[] buffer, int size)
//        {
//            int read = 0;

//            //read = size <= _size ? _size : size;
//            if (Size() < size)
//            {
//                return read;
//            }
//            read = size;

//            if (_head <= _tail)
//            {
//                Array.Copy(_buffer, _head, buffer, 0, read);
//                _head += read;
//            }
//            else
//            {
//                int remain = _bufferlength - _head;
//                if (remain >= read)
//                {
//                    Array.Copy(_buffer, _head, buffer, 0, read);
//                    _head += read;
//                }
//                else
//                {
//                    Array.Copy(_buffer, _head, buffer, 0, remain);
//                    Array.Copy(_buffer, 0, buffer, remain, read - remain);
//                    _head = read - remain;
//                }
//            }

//            return read;
//        }
//        public int Peek(byte[] buffer, int size)
//        {
//            int read = 0;
//            int head = _head;

//            //read = size <= _size ? _size : size;
//            if (Size() < size)
//            {
//                return read;
//            }
//            read = size;

//            if (head <= _tail)
//            {
//                Array.Copy(_buffer, head, buffer, 0, read);
//                head += read;
//            }
//            else
//            {
//                int remain = _bufferlength - head;
//                if (remain >= read)
//                {
//                    Array.Copy(_buffer, head, buffer, 0, read);
//                    head += read;
//                }
//                else
//                {
//                    Array.Copy(_buffer, head, buffer, 0, remain);
//                    Array.Copy(_buffer, 0, buffer, remain, read - remain);
//                    head = read - remain;
//                }
//            }

//            return read;
//        }
//        public void Skip(int size)
//        {
//            _head += size;

//            if (_head >= _bufferlength)
//            {
//                _head = _head - _bufferlength;
//            }
//        }
//        public int Write(byte[] buffer, int size)
//        {
//            int write = 0;
//            if (Space() >= size)
//            {
//                write = size;

//                if (_head <= _tail)
//                {
//                    int remain = _bufferlength - _tail;
//                    if (remain > write)
//                    {
//                        Array.Copy(buffer, 0, _buffer, _tail, write);
//                        _tail += write;
//                    }
//                    else
//                    {
//                        Array.Copy(buffer, 0, _buffer, _tail, remain);
//                        Array.Copy(buffer, remain, _buffer, 0, write - remain);
//                        _tail = write - remain;
//                    }
//                }
//                else
//                {
//                    Array.Copy(buffer, 0, _buffer, _tail, write);
//                    _tail += write;
//                }
//            }
//            return write;
//        }
//    }

//    private RingBuffer _receive_buffer;

//    /// <summary>
//    /// 消息头
//    /// </summary>
//    private byte[] _header = new byte[PaintingNetConsts.PACKET_HEAD_SIZE];
//    private int _packetId = 0;
//    private int _dataSize = 0;
//    public byte[] Data { private set; get; }

//    public PacketReader()
//    {
//        _receive_buffer = new RingBuffer(PaintingNetConsts.MAX_PACKET_SIZE * 2);
//        Data = new byte[PaintingNetConsts.MAX_PACKET_SIZE * 2];
//    }

//    public void Init()
//    {

//    }

//    public void Reset()
//    {
//        _receive_buffer.Reset();
//    }

//    private bool CheckPacketID(int packet_id)
//    {
//        return true;// LuaScriptManager.Instance.Game.CheckPacketID(packet_id);
//    }

//    public bool Read(byte[] data, int offset, int count)
//    {
//        _receive_buffer.Write(data, count);

//        while (_receive_buffer.Size() > 0)
//        {
//            int read = _receive_buffer.Peek(_header, PaintingNetConsts.PACKET_HEAD_SIZE);

//            if (read != PaintingNetConsts.PACKET_HEAD_SIZE)
//            {
//                break;
//            }

//            _packetId = BitConverter.ToInt32(_header, 0);
//            _dataSize = BitConverter.ToInt32(_header, 4);

//            if (!CheckPacketID(_packetId))
//            {
//                byte[] rawData = new byte[count];
//                Array.Copy(data, rawData, count);

//                var rawText = BitConverter.ToString(rawData);

//                Debug.LogFormat("receive error. id: {0}, size: {1}.", _packetId, _dataSize);
//                Debug.Log("!! socket raw data:");
//                Debug.Log(rawText);

//                _receive_buffer.Skip(count);

//                break;
//            }

//            if (_receive_buffer.Size() >= PaintingNetConsts.PACKET_HEAD_SIZE + _dataSize)
//            {
//                _receive_buffer.Skip(PaintingNetConsts.PACKET_HEAD_SIZE);
//                _receive_buffer.Read(Data, _dataSize);

//                PaintingMessageProcessor.Inst.Proc(_packetId, Data, _dataSize);
//            }
//            else
//            {
//                break;
//            }
//        }
//        return true;
//    }
//}
