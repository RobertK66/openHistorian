﻿//******************************************************************************************************
//  ShadowCopyAllocatorTest.cs - Gbtc
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
//  1/4/2012 - Steven E. Chisholm
//       Generated original version of source code. 
//       
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GSF;
using NUnit.Framework;
using GSF.IO.Unmanaged;

namespace openHistorian.FileStructure.Test
{
    [TestFixture()]
    public class ShadowCopyAllocatorTest
    {

        static int BlockSize;
        static int BlockDataLength;
        static int AddressesPerBlock;
        static int AddressesPerBlockSquare;
        static int FirstSingleIndirectBlockIndex;
        static int FirstDoubleIndirectBlockIndex;
        static int FirstTripleIndirectIndex;
        static int LastAddressableBlockIndex;

        static ShadowCopyAllocatorTest()
        {
            BlockSize = 4096;
            BlockDataLength = BlockSize - FileStructureConstants.BlockFooterLength;
            AddressesPerBlock = BlockDataLength / 4; //rounds down
            AddressesPerBlockSquare = AddressesPerBlock * AddressesPerBlock;
            FirstSingleIndirectBlockIndex = 1;
            FirstDoubleIndirectBlockIndex = (int)Math.Min(int.MaxValue, FirstSingleIndirectBlockIndex + (long)AddressesPerBlock);
            FirstTripleIndirectIndex = (int)Math.Min(int.MaxValue, FirstDoubleIndirectBlockIndex + (long)AddressesPerBlock * (long)AddressesPerBlock);
            LastAddressableBlockIndex = (int)Math.Min(int.MaxValue, FirstTripleIndirectIndex + (long)AddressesPerBlock * (long)AddressesPerBlock * (long)AddressesPerBlock - 1);
        }

        [Test()]
        public void Test()
        {
            Assert.AreEqual(Globals.BufferPool.AllocatedBytes, 0L);
            DiskIo stream = new DiskIo(BlockSize, new MemoryStream(), 0);
            FileHeaderBlock header = new FileHeaderBlock(BlockSize, stream, OpenMode.Create, AccessMode.ReadWrite);
            header.CreateNewFile(Guid.NewGuid());
            header.CreateNewFile(Guid.NewGuid());
            header.CreateNewFile(Guid.NewGuid());
            header.WriteToFileSystem(stream);
            TestWrite(stream, 0);
            TestWrite(stream, 1);
            TestWrite(stream, 2);
            TestWrite(stream, 0);
            TestWrite(stream, 1);
            TestWrite(stream, 2);
            header = new FileHeaderBlock(BlockSize, stream, OpenMode.Open, AccessMode.ReadOnly);
            Assert.IsTrue(true);
            stream.Dispose();
            Assert.AreEqual(Globals.BufferPool.AllocatedBytes, 0L);
        }

        internal static void TestWrite(DiskIo stream, int FileNumber)
        {

            FileHeaderBlock header = new FileHeaderBlock(BlockSize, stream, OpenMode.Open, AccessMode.ReadOnly);
            header = header.CloneEditable();
            SubFileMetaData node = header.Files[FileNumber];
            IndexParser parse = new IndexParser(BlockSize, header.SnapshotSequenceNumber, stream, node);
            ShadowCopyAllocator shadow = new ShadowCopyAllocator(BlockSize, stream, header, node, parse);

            int nextPage = header.LastAllocatedBlock + 1;
            byte[] block = new byte[BlockSize];


            shadow.ShadowDataBlock(0);
            PositionData pd = parse.GetPositionData(0);
            if (node.DirectBlock != nextPage)
                throw new Exception();
            if (parse.DataClusterAddress != nextPage)
                throw new Exception();
            stream.WriteToNewBlock(parse.DataClusterAddress, BlockType.DataBlock, (int)(pd.VirtualPosition / BlockDataLength), node.FileIdNumber, header.SnapshotSequenceNumber, block);


            //should do nothing since the page has already been allocated
            shadow.ShadowDataBlock(1024);
            pd = parse.GetPositionData(1024);
            if (node.DirectBlock != nextPage)
                throw new Exception();
            if (parse.DataClusterAddress != nextPage)
                throw new Exception();
            stream.WriteToNewBlock(parse.DataClusterAddress, BlockType.DataBlock, (int)(pd.VirtualPosition / BlockDataLength), node.FileIdNumber, header.SnapshotSequenceNumber, block);


            //Allocate in the 3th indirect block
            shadow.ShadowDataBlock(FirstTripleIndirectIndex * (long)BlockDataLength);
            pd = parse.GetPositionData(FirstTripleIndirectIndex * (long)BlockDataLength);
            if (node.DirectBlock != nextPage)
                throw new Exception();
            if (parse.DataClusterAddress != nextPage + 1)
                throw new Exception();
            if (parse.FirstIndirectBlockAddress != nextPage + 4)
                throw new Exception();
            if (parse.SecondIndirectBlockAddress != nextPage + 3)
                throw new Exception();
            if (parse.ThirdIndirectBlockAddress != nextPage + 2)
                throw new Exception();
            stream.WriteToNewBlock(parse.DataClusterAddress, BlockType.DataBlock, (int)(pd.VirtualPosition / BlockDataLength), node.FileIdNumber, header.SnapshotSequenceNumber, block);

            //if (node.DirectCluster != nextPage)
            //    throw new Exception();
            //if (parse.DataClusterAddress != nextPage + 1)
            //    throw new Exception();
            //if (parse.FirstIndirectBlockAddress != nextPage + 5)
            //    throw new Exception();
            //if (parse.SecondIndirectBlockAddress != nextPage + 4)
            //    throw new Exception();
            //if (parse.ThirdIndirectBlockAddress != nextPage + 3)
            //    throw new Exception();
            //if (parse.ForthIndirectBlockAddress != nextPage + 2)
            //    throw new Exception();

            header.WriteToFileSystem(stream);
        }





    }
}
