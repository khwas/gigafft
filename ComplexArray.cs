using System;
using System.Diagnostics;
using System.Numerics;

namespace GigaFFT
{
    public class ComplexArray : IDisposable
    {
        private bool lightWeight;
        private Complex[][] smallArray = null;
        private ComplexArray2MeC largeArray = null;

        public ComplexArray(ulong totalCapacityComplexStructs)
        {
            Debug.Assert(totalCapacityComplexStructs > 0);
            this.lightWeight = totalCapacityComplexStructs <= 1024UL * 1024UL;
            if (this.lightWeight)
            {
                ulong stripeCount = (totalCapacityComplexStructs + 4096UL - 1UL) / 4096UL;
                Debug.Assert((stripeCount != 0) && ((stripeCount & (stripeCount - 1)) == 0), "stripeCount must be power of 2");
                this.smallArray = new Complex[stripeCount][];
                for (int stripe = 0; stripe < this.smallArray.Length; stripe++)
                {
                    this.smallArray[stripe] = new Complex[Math.Min(totalCapacityComplexStructs, 4096UL)];
                }
            }
            else
            {
                this.largeArray = new ComplexArray2MeC(totalCapacityComplexStructs);
            }
        }

        public ulong Length
        {
            get
            {
                return lightWeight ? (smallArray.Length == 1 ? (ulong)smallArray[0].Length : (ulong)smallArray.Length * 4096UL) : largeArray.Length;
            }
        }

        public Complex this[ulong index]
        {
            get
            {
                return lightWeight ? smallArray[index / 4096UL][index % 4096UL] : largeArray[index];
            }
            set
            {
                if (lightWeight)
                {
                    smallArray[index / 4096UL][index % 4096UL] = value;
                }
                else
                {
                    largeArray[index] = value;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (smallArray != null || largeArray != null)
            {
                if (disposing)
                {
                    if (!lightWeight)
                    {
                        largeArray.Dispose();
                    }
                    smallArray = null;
                    largeArray = null;
                }
            }
        }

        ~ComplexArray()
        {
            Dispose(false);
        }
    }
}
