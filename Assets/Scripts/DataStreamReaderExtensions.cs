using Unity.Collections;
using UnityEngine;

namespace Jy.Packets
{
    public static class DataStreamReaderExtensions
    {
        #region READ
        public static void Read(this ref DataStreamReader reader, out ulong value)
        {
            value = reader.ReadByte();
        }

        public static void Read(this ref DataStreamReader reader, out uint value)
        {
            value = reader.ReadUInt();
        }

        public static void Read(this ref DataStreamReader reader, out int value)
        {
            value = reader.ReadInt();
        }

        public static void Read(this ref DataStreamReader reader, out float value)
        {
            value = reader.ReadFloat();
        }

        public static void Read(this ref DataStreamReader reader, out bool value)
        {
            value = reader.ReadByte() != 0;
        }

        public static void Read(this ref DataStreamReader reader, out Vector2 value)
        {
            reader.Read(out value.x);
            reader.Read(out value.y);
        }

        //nullable vector2
        public static void Read(this ref DataStreamReader reader, out Vector2? value)
        {
            reader.Read(out bool hasValue);
            if (hasValue)
            {
                reader.Read(out Vector2 readValue);
                value = readValue;
            }
            else
            {
                value = null;
            }
        }

        public static void Read(this ref DataStreamReader reader, out Vector3 value)
        {
            reader.Read(out value.x);
            reader.Read(out value.y);
            reader.Read(out value.z);
        }

        public static void Read(this ref DataStreamReader reader, out Quaternion value)
        {
            reader.Read(out value.x);
            reader.Read(out value.y);
            reader.Read(out value.z);
            reader.Read(out value.w);
        }

        public static void Read(this ref DataStreamReader reader, out PacketTypes value)
        {
            reader.Read(out uint readValue);
            value = (PacketTypes)readValue;
        }

        #endregion

        #region WRITE
        public static void Write(this ref DataStreamWriter writer, ulong value)
        {
            writer.Write(value);
        }

        public static void Write(this ref DataStreamWriter writer, uint value)
        {
            writer.Write(value);
        }

        public static void Write(this ref DataStreamWriter writer, int value)
        {
            writer.Write(value);
        }

        public static void Write(this ref DataStreamWriter writer, float value)
        {
            writer.Write(value);
        }

        public static void Write(this ref DataStreamWriter writer, bool value)
        {
            writer.Write((byte)(value ? 1 : 0));
        }


        //vector2
        public static void Write(this ref DataStreamWriter writer, Vector2 value)
        {
            writer.Write(value.x);
            writer.Write(value.y);
        }

        //nullable vector2
        public static void Write(this ref DataStreamWriter writer, Vector2? value)
        {
            if (value.HasValue)
            {
                writer.Write(true);
                writer.Write(value.Value);
            }
            else
            {
                writer.Write(false);
            }
        }

        public static void Write(this ref DataStreamWriter writer, Vector3 value)
        {
            writer.Write(value.x);
            writer.Write(value.y);
            writer.Write(value.z);
        }

        public static void Write(this ref DataStreamWriter writer, Quaternion value)
        {
            writer.Write(value.x);
            writer.Write(value.y);
            writer.Write(value.z);
            writer.Write(value.w);
        }

        public static void Write(this ref DataStreamWriter writer, PacketTypes value)
        {
            writer.Write((uint)value);
        }
        #endregion
    }
}
