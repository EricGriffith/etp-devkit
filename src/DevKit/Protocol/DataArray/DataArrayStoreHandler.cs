﻿//----------------------------------------------------------------------- 
// ETP DevKit, 1.1
//
// Copyright 2016 Energistics
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-----------------------------------------------------------------------

using System.Collections.Generic;
using Avro.IO;
using Energistics.Common;
using Energistics.Datatypes;

namespace Energistics.Protocol.DataArray
{
    /// <summary>
    /// Base implementation of the <see cref="IDataArrayStore"/> interface.
    /// </summary>
    /// <seealso cref="Energistics.Common.EtpProtocolHandler" />
    /// <seealso cref="Energistics.Protocol.DataArray.IDataArrayStore" />
    public class DataArrayStoreHandler : EtpProtocolHandler, IDataArrayStore
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataArrayStoreHandler"/> class.
        /// </summary>
        public DataArrayStoreHandler() : base(Protocols.DataArray, "store", "customer")
        {
        }

        /// <summary>
        /// Sends a data array as a response for GetDataArray and GetDataArraySlice.
        /// </summary>
        /// <param name="dimensions">The dimensions.</param>
        /// <param name="data">The data array.</param>
        /// <returns>The message identifier.</returns>
        public long DataArray(IList<long> dimensions, AnyArray data)
        {
            var header = CreateMessageHeader(Protocols.DataArray, MessageTypes.DataArray.DataArray);

            var message = new DataArray
            {
                Dimensions = dimensions,
                Data = data
            };

            return Session.SendMessage(header, message);
        }

        /// <summary>
        /// Handles the GetDataArray event from a store.
        /// </summary>
        public event ProtocolEventHandler<GetDataArray> OnGetDataArray;

        /// <summary>
        /// Handles the GetDataArraySlice event from a store.
        /// </summary>
        public event ProtocolEventHandler<GetDataArraySlice> OnGetDataArraySlice;

        /// <summary>
        /// Handles the PutDataArray event from a store.
        /// </summary>
        public event ProtocolEventHandler<PutDataArray> OnPutDataArray;

        /// <summary>
        /// Handles the PutDataArraySlice event from a store.
        /// </summary>
        public event ProtocolEventHandler<PutDataArraySlice> OnPutDataArraySlice;

        /// <summary>
        /// Decodes the message based on the message type contained in the specified <see cref="MessageHeader" />.
        /// </summary>
        /// <param name="header">The message header.</param>
        /// <param name="decoder">The message decoder.</param>
        /// <param name="body">The message body.</param>
        protected override void HandleMessage(MessageHeader header, Decoder decoder, string body)
        {
            switch (header.MessageType)
            {
                case (int)MessageTypes.DataArray.GetDataArray:
                    HandleGetDataArray(header, decoder.Decode<GetDataArray>(body));
                    break;

                case (int)MessageTypes.DataArray.GetDataArraySlice:
                    HandleGetDataArraySlice(header, decoder.Decode<GetDataArraySlice>(body));
                    break;

                case (int)MessageTypes.DataArray.PutDataArray:
                    HandlePutDataArray(header, decoder.Decode<PutDataArray>(body));
                    break;

                case (int)MessageTypes.DataArray.PutDataArraySlice:
                    HandlePutDataArraySlice(header, decoder.Decode<PutDataArraySlice>(body));
                    break;

                default:
                    base.HandleMessage(header, decoder, body);
                    break;
            }
        }

        /// <summary>
        /// Handles the GetDataArray message from a store.
        /// </summary>
        /// <param name="header">The message header.</param>
        /// <param name="message">The GetDataArray message.</param>
        protected virtual void HandleGetDataArray(MessageHeader header, GetDataArray message)
        {
            Notify(OnGetDataArray, header, message);
        }

        /// <summary>
        /// Handles the GetDataArraySlice message from a store.
        /// </summary>
        /// <param name="header">The message header.</param>
        /// <param name="message">The GetDataArraySlice message.</param>
        protected virtual void HandleGetDataArraySlice(MessageHeader header, GetDataArraySlice message)
        {
            Notify(OnGetDataArraySlice, header, message);
        }

        /// <summary>
        /// Handles the PutDataArray message from a store.
        /// </summary>
        /// <param name="header">The message header.</param>
        /// <param name="message">The PutDataArray message.</param>
        protected virtual void HandlePutDataArray(MessageHeader header, PutDataArray message)
        {
            Notify(OnPutDataArray, header, message);
        }

        /// <summary>
        /// Handles the PutDataArraySlice message from a store.
        /// </summary>
        /// <param name="header">The message header.</param>
        /// <param name="message">The PutDataArraySlice message.</param>
        protected virtual void HandlePutDataArraySlice(MessageHeader header, PutDataArraySlice message)
        {
            Notify(OnPutDataArraySlice, header, message);
        }
    }
}