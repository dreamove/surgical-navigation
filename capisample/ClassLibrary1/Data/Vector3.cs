//----------------------------------------------------------------------------
//
// Copyright 2020 by Northern Digital Inc.
// All Rights Reserved
//
//----------------------------------------------------------------------------
// By using this Sample Code the licensee agrees to be bound by the
// provisions of the agreement found in the LICENSE.txt file.

using System;

namespace NDI.CapiSample.Data
{
    public class Vector3
    {
        public double x, y, z;

        /// <summary>
        /// Initialize to missing by default.
        /// </summary>
        public Vector3()
        {
            x = y = z = Constants.BAD_FLOAT;
        }

        public Vector3(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
     
        }

        public override string ToString()
        {
            if (this.x == Constants.BAD_FLOAT)
            {
                return "MISSING,MISSING,MISSING";
            }
            else
            {
                //return String.Format("{0,8:F3},{1,8:F3},{2,8:F3}", x, y, z)    ,{3,8:F3},{4,8:F3},{5,8:F3},{6,8:F3},{7,8:F3},{8,8:F3},{9,8:F3},{10,8:F3},{11,8:F3};
                return String.Format("{0,8:F3},{1,8:F3},{2,8:F3}", x, y, z);
            }
        }
    }
}
