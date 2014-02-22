using System;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace GigaFFT
{
    public class MMFArray32MeB : IDisposable
    {
        private static string namePrefix = Guid.NewGuid().ToString();
        private static ulong nameSuffix = 0;
        protected MemoryMappedFile[] files;
        protected ulong[] fileNameSuffixes;
        protected MemoryMappedViewAccessor mmvaLO;
        protected MemoryMappedViewAccessor mmvaHI;
        protected ushort immvaLO;
        protected ushort immvaHI;
        #if DEBUG
        private bool readMode;
        #endif

        public MMFArray32MeB(ulong totalCapacityBytes) 
        {
            checked
            {
                Debug.Assert((totalCapacityBytes != 0) && ((totalCapacityBytes & (totalCapacityBytes - 1)) == 0), "Total Capacity must be power of 2");
                Debug.Assert((totalCapacityBytes >= 33554432UL) && (totalCapacityBytes <= 1099511627776UL), "Total Capacity must fit between 32MeB..1TeB");
                this.files = new MemoryMappedFile[totalCapacityBytes / 16777216UL]; // 2..65536 files 16MeB each
                this.fileNameSuffixes = new ulong[this.files.Length];
                Debug.Assert((ulong)this.files.Length * 16777216UL == totalCapacityBytes);
                for (ushort index = 0; index < (ushort)this.files.Length; index++)
                {
                    lock (namePrefix)
                    {
                        this.fileNameSuffixes[index] = nameSuffix++;
                    }
                    string fileName = namePrefix + "-" + this.fileNameSuffixes[index].ToString();
                    this.files[index] = MemoryMappedFile.CreateFromFile(@"d:\temp\" + fileName, FileMode.CreateNew, fileName, 16777216L, MemoryMappedFileAccess.ReadWrite);
                }
                this.immvaLO = 0;
                this.immvaHI = (ushort)(this.files.Length / 2);
                this.mmvaLO = this.files[immvaLO].CreateViewAccessor();
                this.mmvaHI = this.files[immvaHI].CreateViewAccessor();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (mmvaLO != null || mmvaHI != null || files != null)
            {
                if (disposing)
                {
                    checked
                    {
                        mmvaLO.Dispose();
                        mmvaHI.Dispose();
                        for (ulong index = 0; index < (ulong)files.Length; index++ )
                        {
                            files[index].Dispose();
                            string fileName = @"d:\temp\" + namePrefix + "-" + this.fileNameSuffixes[index].ToString();
                            File.Delete(fileName);
                        }
                    }
                    mmvaLO = null;
                    mmvaHI = null;
                    files = null;
                }
            }
        }

        ~MMFArray32MeB()
        {
            Dispose(false);
        }
    }

}
