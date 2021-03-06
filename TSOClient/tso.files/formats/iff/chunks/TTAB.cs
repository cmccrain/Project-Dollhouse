﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TSO.Files.utils;

namespace TSO.Files.formats.iff.chunks
{
    public class TTAB : IffChunk
    {
        public TTABInteraction[] Interactions;
        public Dictionary<uint, TTABInteraction> InteractionByIndex;

        public override void Read(Iff iff, Stream stream)
        {
            using (var io = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN))
            {
                Interactions = new TTABInteraction[io.ReadUInt16()];
                InteractionByIndex = new Dictionary<uint, TTABInteraction>();
                var version = io.ReadUInt16();
                IOProxy iop;
                if (version != 9 && version != 10) iop = new TTABNormal(io);
                else iop = new TTABFieldEncode(io); //haven't guaranteed that this works, since none of the objects in the test lot use it.

                for (int i = 0; i < Interactions.Length; i++)
                {
                    var result = new TTABInteraction();
                    result.ActionFunction = iop.ReadUInt16();
                    result.TestFunction = iop.ReadUInt16();
                    result.MotiveEntries = new TTABMotiveEntry[iop.ReadUInt32()];
                    result.Flags = iop.ReadUInt32();
                    result.TTAIndex = iop.ReadUInt32();
                    if (version > 6) result.AttenuationCode = iop.ReadUInt32();
                    result.AttenuationValue = iop.ReadFloat();
                    result.AutonomyThreshold = iop.ReadUInt32();
                    result.JoiningIndex = iop.ReadInt32();
                    for (int j = 0; j < result.MotiveEntries.Length; j++)
                    {
                        var motive = new TTABMotiveEntry();
                        if (version > 6) motive.EffectRangeMinimum = iop.ReadInt16();
                        motive.EffectRangeMaximum = iop.ReadInt16();
                        if (version > 6) motive.PersonalityModifier = iop.ReadUInt16();
                        result.MotiveEntries[j] = motive;
                    }
                    if (version > 9) result.Unknown = iop.ReadUInt32();
                    Interactions[i] = result;
                    InteractionByIndex.Add(result.TTAIndex, result);
                }
            }
        }
    }

    abstract class IOProxy
    {
        public abstract ushort ReadUInt16();
        public abstract short ReadInt16();
        public abstract int ReadInt32();
        public abstract uint ReadUInt32();
        public abstract float ReadFloat();

        public IoBuffer io;
        public IOProxy(IoBuffer io)
        {
            this.io = io;
        }
    }

   class TTABNormal : IOProxy
    {
        public override ushort ReadUInt16() { return io.ReadUInt16(); }
        public override short ReadInt16() { return io.ReadInt16(); }
        public override int ReadInt32() { return io.ReadInt32(); }
        public override uint ReadUInt32() { return io.ReadUInt32(); }
        public override float ReadFloat() { return io.ReadFloat(); }

        public TTABNormal(IoBuffer io) : base(io) { }
    }

    class TTABFieldEncode : IOProxy
    {
        private byte bitPos = 0;
        private byte curByte = 0;
        static byte[] widths = { 5, 8, 13, 16 };
        static byte[] widths2 = { 6, 11, 21, 32 };

        public void setBytePos(int n)
        {
            io.Seek(SeekOrigin.Begin, n);
            curByte = io.ReadByte();
            bitPos = 0;
        }

        public override ushort ReadUInt16() {
            return (ushort)ReadField(false);
        }

        public override short ReadInt16()
        {
            return (short)ReadField(false);
        }

        public override int ReadInt32()
        {
            return (int)ReadField(true);
        }

        public override uint ReadUInt32()
        {
            return (uint)ReadField(true);
        }

        public override float ReadFloat()
        {
            return (float)ReadField(true);
            //this is incredibly wrong
        }

        private long ReadField(bool big)
        {
            if (ReadBit() == 0) return 0;

            uint code = ReadBits(2);
            byte width = (big)?widths2[code]:widths[code];
            long value = ReadBits(width);
            value |= -(value & (1 << (width-1)));

            return value;
        }

        private uint ReadBits(int n)
        {
            uint total = 0;
            for (int i = 0; i < n; i++)
            {
                total += (uint)((n - i) << ReadBit());
            }
            return total;
        }

        private byte ReadBit()
        {
            byte result = (byte)(curByte & ((7 - (bitPos++)) << 1));
            if (bitPos > 7)
            {
                bitPos = 0;
                curByte = io.ReadByte();
            }
            return result;
        }

        public TTABFieldEncode(IoBuffer io) : base(io) { }
    }

    public struct TTABInteraction
    {
        public ushort ActionFunction;
        public ushort TestFunction;
        public TTABMotiveEntry[] MotiveEntries;
        public uint Flags;
        public uint TTAIndex;
        public uint AttenuationCode;
        public float AttenuationValue;
        public uint AutonomyThreshold;
        public int JoiningIndex;
        public uint Unknown;
    }

    public struct TTABMotiveEntry
    {
        public short EffectRangeMinimum;
        public short EffectRangeMaximum;
        public ushort PersonalityModifier;
    }

    public enum TTABFlags
    {
        Debug = 1<<7
    }
}
