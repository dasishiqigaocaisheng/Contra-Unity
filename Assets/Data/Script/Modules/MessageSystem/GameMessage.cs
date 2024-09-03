using System.Runtime.InteropServices;
using System.Collections.Generic;
using UnityEngine;

namespace Modules.MessageSystem
{
    public struct GameMessage
    {
        [StructLayout(LayoutKind.Explicit)]
        public struct MessageField
        {
            [FieldOffset(0)]
            public sbyte Sbyte;
            [FieldOffset(0)]
            public byte Byte;
            [FieldOffset(0)]
            public short Short;
            [FieldOffset(0)]
            public ushort Ushort;
            [FieldOffset(0)]
            public int Int;
            [FieldOffset(0)]
            public uint Uint;
            [FieldOffset(0)]
            public long Long;
            [FieldOffset(0)]
            public ulong Ulong;
            [FieldOffset(0)]
            public float Float;
            [FieldOffset(0)]
            public double Double;
            [FieldOffset(0)]
            public decimal Decimal;
            [FieldOffset(0)]
            public bool Bool;
            [FieldOffset(0)]
            public char Char;
            [FieldOffset(0)]
            public string String;
            [FieldOffset(0)]
            public object Object;
            [FieldOffset(0)]
            public dynamic Dynamic;

            [FieldOffset(0)]
            public Vector2 Vector2;
            [FieldOffset(0)]
            public Vector2Int Vector2Int;
            [FieldOffset(0)]
            public Vector3 Vector3;
            [FieldOffset(0)]
            public Vector3Int Vector3Int;
            [FieldOffset(0)]
            public Vector4 Vector4;
            [FieldOffset(0)]
            public Quaternion Quaternion;

            public static implicit operator sbyte(MessageField mf)
            {
                return mf.Sbyte;
            }

            public static implicit operator MessageField(sbyte dat)
            {
                return new MessageField { Sbyte = dat };
            }

            public static implicit operator byte(MessageField mf)
            {
                return mf.Byte;
            }

            public static implicit operator MessageField(byte dat)
            {
                return new MessageField { Byte = dat };
            }

            public static implicit operator short(MessageField mf)
            {
                return mf.Short;
            }

            public static implicit operator MessageField(short dat)
            {
                return new MessageField { Short = dat };
            }

            public static implicit operator int(MessageField mf)
            {
                return mf.Int;
            }

            public static implicit operator MessageField(int dat)
            {
                return new MessageField { Int = dat };
            }

            public static implicit operator uint(MessageField mf)
            {
                return mf.Uint;
            }

            public static implicit operator MessageField(uint dat)
            {
                return new MessageField { Uint = dat };
            }

            public static implicit operator long(MessageField mf)
            {
                return mf.Long;
            }

            public static implicit operator MessageField(long dat)
            {
                return new MessageField { Long = dat };
            }

            public static implicit operator ulong(MessageField mf)
            {
                return mf.Ulong;
            }

            public static implicit operator MessageField(ulong dat)
            {
                return new MessageField { Ulong = dat };
            }

            public static implicit operator float(MessageField mf)
            {
                return mf.Float;
            }

            public static implicit operator MessageField(float dat)
            {
                return new MessageField { Float = dat };
            }

            public static implicit operator double(MessageField mf)
            {
                return mf.Double;
            }

            public static implicit operator MessageField(double dat)
            {
                return new MessageField { Double = dat };
            }

            public static implicit operator decimal(MessageField mf)
            {
                return mf.Decimal;
            }

            public static implicit operator MessageField(decimal dat)
            {
                return new MessageField { Decimal = dat };
            }

            public static implicit operator bool(MessageField mf)
            {
                return mf.Bool;
            }

            public static implicit operator MessageField(bool dat)
            {
                return new MessageField { Bool = dat };
            }

            public static implicit operator char(MessageField mf)
            {
                return mf.Char;
            }

            public static implicit operator MessageField(char dat)
            {
                return new MessageField { Char = dat };
            }

            public static implicit operator string(MessageField mf)
            {
                return mf.String;
            }

            public static implicit operator MessageField(string dat)
            {
                return new MessageField { String = dat };
            }

            public static implicit operator Vector2(MessageField mf)
            {
                return mf.Vector2;
            }

            public static implicit operator MessageField(Vector2 dat)
            {
                return new MessageField { Vector2 = dat };
            }

            public static implicit operator Vector2Int(MessageField mf)
            {
                return mf.Vector2Int;
            }

            public static implicit operator MessageField(Vector2Int dat)
            {
                return new MessageField { Vector2Int = dat };
            }

            public static implicit operator Vector3(MessageField mf)
            {
                return mf.Vector3;
            }

            public static implicit operator MessageField(Vector3 dat)
            {
                return new MessageField { Vector3 = dat };
            }

            public static implicit operator Vector3Int(MessageField mf)
            {
                return mf.Vector3Int;
            }

            public static implicit operator MessageField(Vector3Int dat)
            {
                return new MessageField { Vector3Int = dat };
            }

            public static implicit operator Vector4(MessageField mf)
            {
                return mf.Vector4;
            }

            public static implicit operator MessageField(Vector4 dat)
            {
                return new MessageField { Vector4 = dat };
            }

            public static implicit operator Quaternion(MessageField mf)
            {
                return mf.Quaternion;
            }

            public static implicit operator MessageField(Quaternion dat)
            {
                return new MessageField { Quaternion = dat };
            }
        }

        public MessageUserInfo Sender;

        public MessageUserInfo Receiver;

        public int Type;

        public MessageField Msg0;

        public MessageField Msg1;

        public MessageField Msg2;

        public MessageField Msg3;

        public object Msg4;

        public object[] ExtraMsg;


    }
}
