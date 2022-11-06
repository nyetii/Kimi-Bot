using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kimi.Services
{
    class Misc
    {
        //public virtual async Task<double[]> RatioAsync(int top1, int top2)
        //{
        //    double f1;

        //    double multiplication = (18.0 * 100.0);
        //    double result = multiplication / 25.0;

        //    double[] ratio = new double[2] { await PercentageAsync(top1, 100), await PercentageAsync(top2, result)};

        //    return await Task.FromResult(ratio);
        //}

        //public virtual async Task<double> PercentageAsync(double value, double percentage)
        //{
        //    double result1;
        //    double result2;

        //    result1 = (value / percentage) * 100.0;
        //    return await Task.FromResult(result1);
        //}

        public async Task<double[]> RatioAsync(int length)
        {
            double third = length / 0.33;
            double length2 = third / 0.72;
            double[] ratio = new double[2] { third, length2 };
            return await Task.FromResult(ratio);
        }

        public async Task<string[]> WhitespacesFromRatioAsync(int length)
        {
            double[] ratio = await RatioAsync(length);
            ratio[0] = ratio[0] > 96 ? 96 : ratio[0];
            ratio[1] = ratio[1] > 112 ? 112 : ratio[1];
            ratio[0] /= 1.8;
            ratio[1] /= 2.0;
            string str1 = null;
            string str2 = null;

            for(int i = 0; i < ratio[0]; i++)
            {
                str1 += " ";
            }
            for (int i = 0; i < ratio[1]; i++)
            {
                str2 += " ";
            }

            string[] whitespaces = new string[2] { str1, str2 };
            return await Task.FromResult(whitespaces);
        }
    }
}
