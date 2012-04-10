﻿//******************************************************************************************************
//  ISupportsBinaryStream.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  2/11/2012 - Steven E. Chisholm
//       Generated original version of source code. 
//
//******************************************************************************************************

using System;

namespace openHistorian.V2.Unmanaged
{
    /// <summary>
    /// Implementing this interface allows a binary stream to be attached to a buffer.
    /// </summary>
    public interface ISupportsBinaryStream : IDisposable
    {
        event EventHandler StreamDisposed;

        /// <summary>
        /// Gets the number of available simultaneous read/write sessions.
        /// </summary>
        /// <remarks>This value is used to determine if a binary stream can be cloned
        /// to improve read/write/copy performance.</remarks>
        int RemainingSupportedIoSessions { get; }

        /// <summary>
        /// Gets a block for the following Io session.
        /// </summary>
        /// <param name="ioSession">the id value for the ioSession</param>
        /// <param name="position">the block returned must contain this position</param>
        /// <param name="isWriting">indicates if the stream plans to write to this block</param>
        /// <param name="firstPointer">the pointer for the first byte of the block</param>
        /// <param name="firstPosition">the position that corresponds to <see cref="firstPointer"/></param>
        /// <param name="length">the length of the block</param>
        /// <param name="supportsWriting">notifies the calling class if this block supports 
        /// writing without requiring this function to be called again if <see cref="isWriting"/> was set to false.</param>
        void GetBlock(int ioSession, long position, bool isWriting, out IntPtr firstPointer, out long firstPosition, out int length, out bool supportsWriting);

        /// <summary>
        /// Notifies the underlying stream that this IO session is not longer required.
        /// </summary>
        /// <param name="ioSession">the io session id to release</param>
        void ReleaseIoSession(int ioSession);
        
        /// <summary>
        /// Aquire a positive ID value for a new IO session.
        /// </summary>
        /// <returns>A non-negative value that may also be non-unique to represent a io session.</returns>
        /// <remarks>If the underlying stream does not require io session ID numbers, 
        /// it is free to return a non-unique number since it will be ignoring the value anyway.
        /// this value cannot be negative</remarks>
        int GetNextIoSession();
       
        ///// <summary>
        ///// Modifies the position of the underlying stream.
        ///// </summary>
        //long Position { get; set; }
        
    }
}
