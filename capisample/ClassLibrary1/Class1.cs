using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Runtime.Serialization.Formatters.Binary;

namespace ClassLibrary1
{
    public class function
    {
        public static Matrix<double> GetTF(Matrix<double> x, Matrix<double> y)
        {
            double[] mean1 = new double[3];
            double[] mean2 = new double[3];
            DenseMatrix pointbefore = (DenseMatrix)x;
            DenseMatrix pointafter = (DenseMatrix)y;
            for (int j = 0; j < 3; j++)
            {
                mean1[j] = (pointbefore[0, j] + pointbefore[1, j] + pointbefore[2, j] + pointbefore[3, j]) / 4;
                mean2[j] = (pointafter[0, j] + pointafter[1, j] + pointafter[2, j] + pointafter[3, j]) / 4;
            }
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    pointbefore[i, j] = pointbefore[i, j] - mean1[j];
                    pointafter[i, j] = pointafter[i, j] - mean2[j];
                }
            }
            pointbefore = (DenseMatrix)pointbefore.Transpose();
            DenseMatrix H = pointbefore * pointafter;
            DenseMatrix U = (DenseMatrix)(H.Svd(true).U);
            DenseMatrix V = (DenseMatrix)(H.Svd(true).VT);
            U = (DenseMatrix)U.Transpose();
            V = (DenseMatrix)V.Transpose();
            DenseMatrix R = V * U;
            if (R.Determinant() < 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    V[i, 2] = -V[i, 2];
                }
                R = V * U;
            }
            Vector t = -1 * R * mean1 + mean2;
            //Console.WriteLine("{0},{1}", R, t);
            double[,] TF ={{R[0,0],R[0,1],R[0,2],t[0]},
                           {R[1,0],R[1,1],R[1,2],t[1]},
                           {R[2,0],R[2,1],R[2,2],t[2]},
                           {0, 0, 0, 1}};
            Matrix<double> result_TF = DenseMatrix.OfArray(TF);
            return result_TF;

        }
        public static double[] TF_translation(Matrix<double> tf)
        {
            double[] trans = { tf[0, 3], tf[1, 3], tf[2, 3] };
            return trans;
        }
        public static double[] rotm2angle(Matrix<double> R)
        {
            double X = Math.Atan2(-R[1, 2], Math.Sqrt(R[1, 0] * R[1, 0] + R[1, 1] * R[1, 1]));
            double Y = Math.Atan2(R[0, 2], R[2, 2]);
            double Z = Math.Atan2(R[1, 0], R[1, 1]);
            double[] result_angle = { X, Y, Z };
            return result_angle;
        }
        public static byte[] ToBytes(double[] da)
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, da);
            byte[] ba = ms.ToArray();
            ms.Close();
            return ba;
        }
        public static void AppendTextToConsole(string txt)//控制台显示
        {
            Console.WriteLine(string.Format("{0}", txt));
        }
    }
}
