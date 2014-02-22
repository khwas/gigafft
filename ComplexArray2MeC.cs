using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;

namespace GigaFFT
{
    public class ComplexArray2MeC : MMFArray32MeB
    {
        private const ulong COMPLEX_SIZE_16_BYTES = 16UL;
        private const ulong COMPLEX_ARRAY_16_MEBYTES = 16UL * 1024 * 1024;
        private static ulong ComplexSize16Bytes = (ulong)Marshal.SizeOf(typeof(Complex));

        private long offsetLO;
        private Complex[] bufferLO;
        private long offsetHI;
        private Complex[] bufferHI;

        public ComplexArray2MeC(ulong totalCapacityComplexStructs)
            : base(totalCapacityComplexStructs * COMPLEX_SIZE_16_BYTES)
        {
            checked
            {
                Debug.Assert(COMPLEX_ARRAY_16_MEBYTES == 16777216UL, "Error in copy paste for COMPLEX_ARRAY_16_MEBYTES. Expected 16777216UL");
                Debug.Assert(ComplexSize16Bytes == COMPLEX_SIZE_16_BYTES, "Unexpected value of ComplexSize16Bytes. Expected 16.");
                Debug.Assert((totalCapacityComplexStructs / 2) * 2 == totalCapacityComplexStructs, "totalCapacityComplexStructs must be an even ulong");
                this.Length = totalCapacityComplexStructs;
                this.LengthDiv2 = this.Length / 2;
                this.offsetLO = -1;
                this.bufferLO = new Complex[4096];
                this.offsetHI = -1;
                this.bufferHI = new Complex[4096];
            }
        }

        public ulong Length
        {
            get;
            private set;
        }

        private ulong LengthDiv2
        {
            get;
            set;
        }

        public Complex this[ulong index]
        {
            get
            {
                ushort immva = (ushort)((index * COMPLEX_SIZE_16_BYTES) / COMPLEX_ARRAY_16_MEBYTES);
                long offset = (long)((index * COMPLEX_SIZE_16_BYTES) - (immva * COMPLEX_ARRAY_16_MEBYTES));
                Debug.Assert(offset >= 0);
                if (index < this.LengthDiv2)
                {
                    if (immva != this.immvaLO)
                    {
                        this.immvaLO = immva;
                        this.mmvaLO.Dispose();
                        this.mmvaLO = this.files[immva].CreateViewAccessor();
                        this.offsetLO = -1;
                    }
                    if (this.offsetLO != (offset & 0x7FFFFFFFFFFF0000L))
                    {
                        this.offsetLO = offset & 0x7FFFFFFFFFFF0000L;
                        this.mmvaLO.ReadArray<Complex>(offsetLO, this.bufferLO, 0, 4096);
                    }
                    //this.mmvaLO.Read(offset, out result);
                    return this.bufferLO[index % 0x1000];
                }
                else
                {
                    if (immva != this.immvaHI)
                    {
                        this.immvaHI = immva;
                        this.mmvaHI.Dispose();
                        this.mmvaHI = this.files[immva].CreateViewAccessor();
                        this.offsetLO = -1;
                    }
                    if (this.offsetHI != (offset & 0x7FFFFFFFFFFF0000L))
                    {
                        this.offsetHI = offset & 0x7FFFFFFFFFFF0000L;
                        this.mmvaHI.ReadArray<Complex>(offsetHI, this.bufferHI, 0, 4096);
                    }
                    //this.mmvaHI.Read(offset, out result);
                    return this.bufferHI[index % 0x1000];
                }
            }
            set
            {
                ushort immva = (ushort)((index * COMPLEX_SIZE_16_BYTES) / COMPLEX_ARRAY_16_MEBYTES);
                long offset = (long)((index * COMPLEX_SIZE_16_BYTES) - (immva * COMPLEX_ARRAY_16_MEBYTES));
                Debug.Assert(offset >= 0);
                if (index < this.LengthDiv2)
                {
                    if (immva != this.immvaLO)
                    {
                        this.immvaLO = immva;
                        this.mmvaLO.Dispose();
                        this.mmvaLO = this.files[immva].CreateViewAccessor();
                    }
                    this.mmvaLO.Write(offset, ref value);
                }
                else
                {
                    if (immva != this.immvaHI)
                    {
                        this.immvaHI = immva;
                        this.mmvaHI.Dispose();
                        this.mmvaHI = this.files[immva].CreateViewAccessor();
                    }
                    this.mmvaHI.Write(offset, ref value);
                }
            }
        }
    }
}
