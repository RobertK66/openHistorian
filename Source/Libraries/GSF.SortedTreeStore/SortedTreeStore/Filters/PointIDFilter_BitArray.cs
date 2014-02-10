﻿//******************************************************************************************************
//  PointIDFilter_BitArray.cs - Gbtc
//
//  Copyright © 2013, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  11/9/2013 - Steven E. Chisholm
//       Generated original version of source code. 
//     
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSF.Collections;
using GSF.IO;
using GSF.SortedTreeStore.Engine;
using openHistorian;

namespace GSF.SortedTreeStore.Filters
{
    public partial class PointIDFilter
    {
        /// <summary>
        /// A filter that uses a <see cref="BitArray"/> to set true and false values
        /// </summary>
        public class BitArrayFilter<TKey>
            : KeyMatchFilterBase<TKey>
            where TKey : EngineKeyBase<TKey>, new()
        {
            readonly BitArray m_points;

            public ulong MaxValue = ulong.MaxValue;
            public ulong MinValue = ulong.MinValue;

            public long[] ArrayBits;

            /// <summary>
            /// Creates a new filter backed by a <see cref="BitArray"/>.
            /// </summary>
            /// <param name="stream">The the stream to load from.</param>
            /// <param name="pointCount">the number of points in the stream.</param>
            /// <param name="maxValue">the maximum value stored in the bit array. Cannot be larger than int.MaxValue-1</param>
            public BitArrayFilter(BinaryStreamBase stream, int pointCount, ulong maxValue)
            {
                if (maxValue >= int.MaxValue)
                    throw new ArgumentOutOfRangeException("maxValue", "Cannot be larger than int.MaxValue-1");

                MaxValue = maxValue;
                m_points = new BitArray(false, (int)maxValue + 1);
                while (pointCount > 0)
                {
                    //Since a bitarray cannot have more than 32bit 
                    m_points.SetBit((int)stream.ReadUInt32());
                    pointCount--;
                }
                ArrayBits = m_points.GetInternalData();

                foreach (int point in m_points.GetAllSetBits())
                {
                    MinValue = (ulong)point;
                    break;
                }
            }

            /// <summary>
            /// Creates a bit array filter from <see cref="points"/>
            /// </summary>
            /// <param name="points">the points to use.</param>
            /// <param name="maxValue">the maximum value stored in the bit array. Cannot be larger than int.MaxValue-1</param>
            public BitArrayFilter(IEnumerable<ulong> points, ulong maxValue)
            {
                MaxValue = maxValue;
                m_points = new BitArray(false, (int)maxValue + 1);
                foreach (ulong pt in points)
                {
                    m_points.SetBit((int)pt);
                }
                ArrayBits = m_points.GetInternalData();

                foreach (int point in m_points.GetAllSetBits())
                {
                    MinValue = (ulong)point;
                    break;
                }

            }

            public override Guid FilterType
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override void Load(BinaryStreamBase stream)
            {
                throw new NotImplementedException();
            }

            public override void Save(BinaryStreamBase stream)
            {
                stream.Write((byte)1); //Stored as array of uint[]
                stream.Write(MaxValue);
                stream.Write(m_points.SetCount);
                foreach (int x in m_points.GetAllSetBits())
                {
                    stream.Write((uint)x);
                }
            }

            public override bool Contains(TKey key)
            {
                int point = (int)key.PointID;
                return (key.PointID <= MaxValue &&
                    ((ArrayBits[point >> BitArray.BitsPerElementShift] & (1L << (point & BitArray.BitsPerElementMask))) != 0));
            }

            /// <summary>
            /// The boundaries of the page.
            /// </summary>
            /// <param name="lowerBounds">the lower inclusive bounds of the page</param>
            /// <param name="upperBounds">the upper exclusive bounds of the page</param>
            /// <returns></returns>
            public override bool PageCannotContainPoints(TKey lowerBounds, TKey upperBounds)
            {
                //ToDo: Consider this implementation.  Could work very well with SCADA systems or a system with tens of thousands of points.
                // lp = lower point;  up = upper point
                // if either condition is true, the page cannot 
                // contain the filter.
                // Otherwise, it can.
                //
                // lp  up  [ filter ]
                //
                // or 
                //
                // [ filter ] lp   up
                // 
                if (m_points.SetCount == 0)
                    return true;
                if (lowerBounds.Timestamp != upperBounds.Timestamp)
                    return false;
                if (lowerBounds.PointID > MaxValue || upperBounds.PointID < MinValue)
                    return true;

                return false;
            }

        }
    }
}
