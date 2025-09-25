using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus read coil functions/requests.
    /// </summary>
    public class ReadCoilsFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadCoilsFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
		public ReadCoilsFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
        }

        /// <inheritdoc/>
        public override byte[] PackRequest()
        {
            ModbusReadCommandParameters paramCom = this.CommandParameters as ModbusReadCommandParameters;

            byte[] request = new byte[12];

            Buffer.BlockCopy(
                (Array)BitConverter.GetBytes(
                    IPAddress.HostToNetworkOrder(
                        (short)paramCom.TransactionId)), 0, (Array)request, 0, 2);

            Buffer.BlockCopy(
                (Array)BitConverter.GetBytes(
                    IPAddress.HostToNetworkOrder((short)paramCom.ProtocolId)
                ),
                0,
                (Array)request,
                2,
                2
            );

            Buffer.BlockCopy(
                (Array)BitConverter.GetBytes(
                    IPAddress.HostToNetworkOrder((short)paramCom.Length)
                ),
                0,
                (Array)request,
                4,
                2
            );

            request[6] = paramCom.UnitId;
            request[7] = paramCom.FunctionCode;

            Buffer.BlockCopy(
                (Array)BitConverter.GetBytes(
                    IPAddress.HostToNetworkOrder((short)paramCom.StartAddress)
                ),
                0,
                (Array)request,
                8,
                2
            );

            Buffer.BlockCopy(
                (Array)BitConverter.GetBytes(
                    IPAddress.HostToNetworkOrder((short)paramCom.Quantity)
                ),
                0,
                (Array)request,
                10,
                2
            );
            return request;

        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {

            Dictionary<Tuple<PointType, ushort>, ushort> dictionary = new Dictionary<Tuple<PointType, ushort>, ushort>();
            ModbusReadCommandParameters parameters = (ModbusReadCommandParameters)CommandParameters;

            ushort startAddress = parameters.StartAddress;
            int byteCount = response[8];

            for (int i = 0; i < byteCount; ++i)
            {
                for (int j = 0; j < 8; ++j)
                {
                    if (parameters.Quantity <= 8 * i + j)
                        break;

                    ushort value = (ushort)(response[9 + i] & 0x1);
                    response[9 + i] /= 2;

                    dictionary.Add(new Tuple<PointType, ushort>(PointType.DIGITAL_OUTPUT, startAddress++), value);
                }
            }

            return dictionary;
        }
    }
}