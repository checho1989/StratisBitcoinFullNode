﻿using System;
using System.Collections.Generic;

namespace Stratis.Features.SQLiteWalletRepository
{
    internal class ByteArrayEqualityComparer : IEqualityComparer<byte[]>
    {
        public int GetHashCode(byte[] obj)
        {
            ulong hash = 17;

            foreach (byte objByte in obj)
            {
                hash = (hash << 5) - hash + objByte;
            }

            return (int)hash;
        }

        public bool Equals(byte[] obj1, byte[] obj2)
        {
            if (obj1.Length != obj2.Length)
                return false;

            for (int i = 0; i < obj1.Length; i++)
                if (obj1[i] != obj2[i])
                    return false;

            return true;
        }
    }

    internal class ObjectsOfInterest
    {
        private byte[] hashArray;
        private int maxHashArrayLengthLog;
        private uint bitIndexLimiter;
        protected HashSet<byte[]> tentative;

        public ObjectsOfInterest(int MaxHashArrayLengthLog)
        {
            this.maxHashArrayLengthLog = MaxHashArrayLengthLog;
            this.hashArray = new byte[1 << this.maxHashArrayLengthLog];
            this.bitIndexLimiter = ((uint)1 << (this.maxHashArrayLengthLog + 3)) - 1;
            this.tentative = new HashSet<byte[]>(new ByteArrayEqualityComparer());
        }

        private uint GetHashCode(byte[] obj)
        {
            ulong hash = 17;

            foreach (byte objByte in obj)
            {
                hash = (hash << 5) - hash + objByte;
            }

            return (uint)hash;
        }

        protected bool MayContain(byte[] obj)
        {
            uint hashArrayBitIndex = this.GetHashCode(obj) & this.bitIndexLimiter;

            return (this.hashArray[hashArrayBitIndex >> 3] & (1 << (int)(hashArrayBitIndex & 7))) != 0;
        }

        protected bool? Contains(byte[] obj)
        {
            if (this.tentative.Contains(obj))
                return true;

            if (!this.MayContain(obj))
                return false;

            // May contain...
            return null;
        }

        protected void Add(byte[] obj)
        {
            uint hashArrayBitIndex = this.GetHashCode(obj) & this.bitIndexLimiter;

            this.hashArray[hashArrayBitIndex >> 3] |= (byte)(1 << (int)(hashArrayBitIndex & 7));
        }

        protected void AddTentative(byte[] obj)
        {
            if (!this.MayContain(obj))
                this.tentative.Add(obj);
        }

        public void Confirm(Func<byte[], bool> exists)
        {
            foreach (byte[] obj in this.tentative)
                if (exists(obj))
                    Add(obj);

            this.tentative.Clear();
        }
    }

}