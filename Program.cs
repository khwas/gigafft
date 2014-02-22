using System;
using System.Diagnostics;
using System.Numerics;

namespace GigaFFT
{
    class Program
    {
        static void Main(string[] args)
        {

            {
                //GigaFFTTest.Test1();
                //GigaFFTTest.Test2();
                //GigaFFTTest.Test3();
                //GigaFFTTest.Test4();
                //GigaFFTTest.Test5();
                //GigaFFTTest.Test6();
                //GigaFFTTest.Test7();
                //GigaFFTTest.Test8();
                //GigaFFTTest.Test9();
                //GigaFFTTest.Test10();
                //GigaFFTTest.Test11();
                //GigaFFTTest.Test12();
                //GigaFFTTest.Test13();
                //GigaFFTTest.Test14();
                //GigaFFTTest.Test15();
                //GigaFFTTest.Test16();
                //GigaFFTTest.Test17();
                //GigaFFTTest.Test18();


                Stopwatch watch1 = Stopwatch.StartNew();
                Complex4Ks data = new Complex4Ks(8 * 1024 * 1024);
                watch1.Stop();
                Console.WriteLine("New    " + watch1.Elapsed.TotalSeconds);

                Stopwatch watch2 = Stopwatch.StartNew();
                //data[0] = new Complex[4096];
                //data[1] = new Complex[4096];
                //data[3][10] = new Complex(1, 0);
                //data[3][20] = new Complex(2.2, 0);
                for (int i = 0; i < data.Length4ks; i++)
                {
                    Complex[] part = new Complex[4096];
                    for (int ii = 0; ii < 4096; ii++)
                    {
                        //part[ii] = new Complex(i, ii);
                        part[ii] = Complex.Zero;
                    }
                    data[i] = part;
                }
                Complex[] zz = new Complex[4096];
                zz[22] = Complex.One;
                data[0] = zz;
                watch2.Stop();
                Console.WriteLine("Write  " + watch2.Elapsed.TotalSeconds);

                Stopwatch watch4 = Stopwatch.StartNew();
                Complex4Ks data2 = FourierTransform.FFT(data);
                watch4.Stop();
                Console.WriteLine("FFT    " + watch4.Elapsed.TotalSeconds);
                Console.ReadLine();

                //for (int i = 0; i < data2.Length4ks; i++)
                //{
                //    Complex[] entry = data2[i];
                //    for (int ii = 0; ii < 4096; ii++)
                //    {
                //        Console.WriteLine(entry[ii]);
                //    }
                //}

            }
        }
    }
}
