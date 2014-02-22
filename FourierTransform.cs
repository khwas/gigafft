using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;

namespace GigaFFT
{

    public class FourierTransform
    {

        public static Complex[] FFT(Complex[] x)
        {
            int N = x.Length;
            //Debug.Assert(N <= 4096);
            if (N == 1)
            {
                return x;
            }
            Complex[] e = new Complex[N / 2];
            Complex[] d = new Complex[N / 2];
            for (int k = 0; k < N / 2; k++)
            {
                e[k] = x[2 * k];
                d[k] = x[2 * k + 1];
            }
            Complex[] E = FFT(e);
            Complex[] D = FFT(d);
            Complex[] result = new Complex[N];
            for (int k = 0; k < N / 2; k++)
            {
                Complex Dk = D[k] * Complex.FromPolarCoordinates(1, -2 * Math.PI * k / N);
                result[k] = E[k] + Dk;
                result[k + N / 2] = E[k] - Dk;
            }
            return result;
        }

        public static Complex4Ks FFT(Complex4Ks x, byte forkWidth = 2)
        {
            long N = x.Length4ks * 4096;
            Debug.Assert(N > 4096);
            Complex4Ks E4ks = null;
            Complex4Ks D4ks = null;
            Complex[] E = null;
            Complex[] D = null;
            if (N > 8192)
            {
                Complex4Ks e4ks = new Complex4Ks(N / 2);
                Complex4Ks d4ks = new Complex4Ks(N / 2);
                Debug.Assert(e4ks.Length4ks == N / (2 * 4096));
                Debug.Assert(d4ks.Length4ks == N / (2 * 4096));
                for (long k = 0; k < N / (2 * 4096); k++)
                {
                    Complex[] e4k = new Complex[4096];
                    Complex[] d4k = new Complex[4096];
                    Complex[] lo = x[k * 2];
                    Complex[] hi = x[k * 2 + 1];
                    for (int i = 0; i < 2048; i++)
                    {
                        e4k[i] = lo[2 * i];
                        d4k[i] = lo[2 * i + 1];
                    }
                    for (int i = 0; i < 2048; i++)
                    {
                        e4k[2048 + i] = hi[2 * i];
                        d4k[2048 + i] = hi[2 * i + 1];
                    }
                    e4ks[k] = e4k;
                    d4ks[k] = d4k;
                }
                x.Dispose();

                forkWidth = (byte)(forkWidth / 2);
                Task[] tasks = new Task[] { 
                                Task.Factory.StartNew(() => {E4ks = FFT(e4ks, forkWidth);e4ks.Dispose();}, forkWidth > 0 ? TaskCreationOptions.LongRunning: TaskCreationOptions.None),
                                Task.Factory.StartNew(() => {D4ks = FFT(d4ks, forkWidth);d4ks.Dispose();}, forkWidth > 0 ? TaskCreationOptions.LongRunning: TaskCreationOptions.None)
                            };
                Task.WaitAll(tasks);
            }
            else
            {
                Debug.Assert(N / 2 == 4096);
                Complex[] e = new Complex[N / 2];
                Complex[] d = new Complex[N / 2];
                Debug.Assert(x.Length4ks == 2);
                Complex[] lo = x[0];
                Complex[] hi = x[1];
                x.Dispose();
                for (int twok = 0; twok < 2048; twok++)
                {
                    e[twok] = lo[2 * twok];
                    d[twok] = lo[2 * twok + 1];
                }
                for (int twok = 0; twok < 2048; twok++)
                {
                    e[2048 + twok] = hi[2 * twok];
                    d[2048 + twok] = hi[2 * twok + 1];
                }
                E = FFT(e);
                D = FFT(d);
            }
            Complex4Ks result;
            if (N > 8192)
            {
                result = new Complex4Ks(N);
                Debug.Assert(E4ks.Length4ks == N / (2 * 4096));
                Debug.Assert(D4ks.Length4ks == N / (2 * 4096));
                for (int fourK = 0; fourK < N / (2 * 4096); fourK++)
                {
                    Complex[] resultLO = new Complex[4096];
                    Complex[] EE = E4ks[fourK];
                    Complex[] DD = D4ks[fourK];
                    for (int k = 0; k < 4096; k++)
                    {
                        Complex Dk = DD[k] * Complex.FromPolarCoordinates(1, -2 * Math.PI * (fourK * 4096 + k) / N);
                        resultLO[k] = EE[k] + Dk;
                    }
                    result[fourK] = resultLO;
                }
                for (int fourK = 0; fourK < N / (2 * 4096); fourK++)
                {
                    Complex[] resultHI = new Complex[4096];
                    Complex[] EE = E4ks[fourK];
                    Complex[] DD = D4ks[fourK];
                    for (int k = 0; k < 4096; k++)
                    {
                        Complex Dk = DD[k] * Complex.FromPolarCoordinates(1, -2 * Math.PI * (fourK * 4096 + k) / N);
                        resultHI[k] = EE[k] - Dk;
                    }
                    result[fourK + N / (2 * 4096)] = resultHI;
                }
                E4ks.Dispose();
                D4ks.Dispose();
            }
            else
            {
                Debug.Assert(N / 2 == 4096);
                result = new Complex4Ks(8192);
                Debug.Assert(result.Length4ks == 2);
                Complex[] resultLO = new Complex[4096];
                Complex[] resultHI = new Complex[4096];
                for (int k = 0; k < 4096; k++)
                {
                    Complex Dk = D[k] * Complex.FromPolarCoordinates(1, -2 * Math.PI * k / N);
                    resultLO[k] = E[k] + Dk;
                    resultHI[k] = E[k] - Dk;
                }
                result[0] = resultLO;
                result[1] = resultHI;
            }
            return result;
        }

    }
}
