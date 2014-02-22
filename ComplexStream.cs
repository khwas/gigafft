using System;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Numerics;

namespace GigaFFT
{

    public class Complex4Ks : IDisposable
    {
        public static long totalRAM = 3L * 1024 * 1024 * 1024;

        private static string namePrefix = Guid.NewGuid().ToString();
        private static long nameSuffix = 0;
        private static Object nameSuffixLock = new Object();
        private static Object totalRAMLock = new Object();

        private MemoryMappedFile file;
        private readonly long suffix;
        private const int fourK = 4096;
        private readonly long length4ks;
        private Complex[][] array;

        public Complex4Ks(long complexEntryCount)
        {
            Debug.Assert(complexEntryCount > fourK);
            Debug.Assert(complexEntryCount % 0x1000 == 0);
            Debug.Assert((complexEntryCount != 0) && ((complexEntryCount & (complexEntryCount - 1)) == 0), "complexEntryCount must be power of 2");
            this.length4ks = complexEntryCount / fourK;
            Debug.Assert(complexEntryCount == fourK * this.length4ks);
            bool ramIsLow = this.length4ks > 1024;
            if (!ramIsLow)
            {
                lock (totalRAMLock)
                {
                    ramIsLow = totalRAM <= 0L;
                }
            }
            if (ramIsLow)
            {
                lock (nameSuffixLock)
                {
                    this.suffix = nameSuffix++;
                }
                this.file = MemoryMappedFile.CreateFromFile(FileName, FileMode.CreateNew, namePrefix + "-" + suffix, complexEntryCount * 16L, MemoryMappedFileAccess.ReadWrite);
            }
            else
            {
                lock (totalRAMLock)
                {
                    totalRAM -= this.length4ks * 4096 * 16;
                }
                this.array = new Complex[this.length4ks][];
            }
        }

        public string FileName
        {
            get
            {
                return this.array == null ? @"d:\temp\" + namePrefix + "-" + suffix : null;
            }
        }

        public Complex[] this[long index]
        {
            get
            {
                if (this.array != null)
                {
                    return this.array[index];
                }
                using (MemoryMappedViewAccessor accessor = this.file.CreateViewAccessor(index * fourK * 16L, fourK * 16L))
                {
                    Complex[] result = new Complex[fourK];
                    accessor.ReadArray<Complex>(0, result, 0, 4096);
                    return result;
                }
            }
            set
            {
                if (this.array != null)
                {
                    this.array[index] = value;
                }
                else
                {
                    using (MemoryMappedViewAccessor accessor = this.file.CreateViewAccessor(index * fourK * 16L, fourK * 16L))
                    {
                        accessor.WriteArray<Complex>(0, value, 0, 4096);
                    }
                }
            }
        }

        public long Length4ks
        {
            get
            {
                return this.length4ks;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (file != null)
            {
                if (disposing)
                {
                    file.Dispose();
                    File.Delete(FileName);
                    file = null;
                }
            }
            if (array != null)
            {
                array = null;
                lock (totalRAMLock)
                {
                    totalRAM += this.length4ks * 4096 * 16;
                }
            }
        }

        ~Complex4Ks()
        {
            Dispose(false);
        }

    }

}
