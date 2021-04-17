using Beer.DaAPI.BlazorApp.Helper;
using Beer.DaAPI.BlazorApp.ModelHelper;
using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Packets.DHCPv4;
using Humanizer;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.DaAPI.BlazorApp.Dialogs
{
    public partial class DHCPv4PacketDetailsDialog : DaAPIDialogBase
    {
        [Parameter] public DHCPv4Packet Packet { get; set; }

        private HashSet<TreeItemData> _items;

        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            _items = new HashSet<TreeItemData>();

            if (Packet == null) { return; }

            _items.Add(new TreeItemData { Name = "Source", Value = Packet.Header.Source.ToString() });
            _items.Add(new TreeItemData { Name = "Destionation", Value = Packet.Header.Destionation.ToString() });

            _items.Add(new TreeItemData { Name = "Op Code", Value = Packet.OpCode.ToString() });
            _items.Add(new TreeItemData { Name = "Hardware Address Type", Value = Packet.HardwareType.ToString() });
            _items.Add(new TreeItemData { Name = "Hardware Address Length", Value = Packet.HardwareAddressLength.ToString() });
            _items.Add(new TreeItemData { Name = "Hops", Value = Packet.Hops.ToString() });
            _items.Add(new TreeItemData { Name = "Transaction Id", Value = Packet.TransactionId.ToString() });
            _items.Add(new TreeItemData { Name = "Flags", Value = Packet.Flags.ToString() });
            _items.Add(new TreeItemData { Name = "Client IP Address", Value = Packet.ClientIPAdress.ToString() });
            _items.Add(new TreeItemData { Name = "Your (client) IP Address", Value = Packet.YourIPAdress.ToString() });
            _items.Add(new TreeItemData { Name = "Next Server IP Address", Value = Packet.ServerIPAdress.ToString() });
            _items.Add(new TreeItemData { Name = "Relay Agent IP Address", Value = Packet.GatewayIPAdress.ToString() });
            _items.Add(new TreeItemData { Name = "Client MAC Address", Value = ByteHelper.ToString(Packet.ClientHardwareAddress,'-')});
            _items.Add(new TreeItemData { Name = "Server Host Name", Value = Packet.ServerHostname.ToString() });
            _items.Add(new TreeItemData { Name = "Bootfile Name", Value = Packet.FileName.ToString() });

            HashSet<TreeItemData> options = new();

            foreach (var option in Packet.Options)
            {
                String name = $"{_codeToNameConverter.GetName(option.OptionType)} ({option.OptionType})";

                switch (option)
                {
                    case DHCPv4PacketAddressListOption castedOption:
                        {
                            Int32 index = 1;
                            options.Add(new TreeItemData { Name = name, Children = castedOption.Addresses.Select(x => new TreeItemData { Name = (index++).ToString(), Value = x.ToString() }).ToHashSet() });
                        }
                        break;
                    case DHCPv4PacketAddressOption castedOption:
                        options.Add(new TreeItemData { Name = name, Value = castedOption.Address.ToString() });
                        break;
                    case DHCPv4PacketBooleanOption castedOption:
                        options.Add(new TreeItemData { Name = name, Value = castedOption.Value.ToString() });
                        break;
                    case DHCPv4PacketByteOption castedOption:
                        options.Add(new TreeItemData { Name = name, Value = castedOption.Value.ToString() });
                        break;
                    case DHCPv4PacketMessageTypeOption castedOption:
                        options.Add(new TreeItemData { Name = name, Value = castedOption.Value.ToString() });
                        break;
                    case DHCPv4PacketParameterRequestListOption castedOption:
                        {
                            Int32 index = 1;
                            options.Add(new TreeItemData { Name = name, Children = castedOption.RequestOptions.Select(x => new TreeItemData { Name = (index++).ToString(), Value = $"{_codeToNameConverter.GetName(x)} ({x})" }).ToHashSet() });
                        }
                        break;
                    case DHCPv4PacketRawByteOption castedOption:
                        options.Add(new TreeItemData { Name = name, Value = ByteHelper.ToString(castedOption.OptionData) });
                        break;
                    case DHCPv4PacketRouteListOption castedOption:
                        {
                            Int32 index = 1;
                            options.Add(new TreeItemData { Name = name, Children = castedOption.Routes.Select(x => new TreeItemData { Name = (index++).ToString(), Value = $"{x.Network}/{x.SubnetMask.GetSlashNotation()}" }).ToHashSet() });
                        }
                        break;
                    case DHCPv4PacketTextOption castedOption:
                        options.Add(new TreeItemData { Name = name, Value = castedOption.Value.ToString() });
                        break;
                    case DHCPv4PacketTimeSpanOption castedOption:
                        options.Add(new TreeItemData { Name = name, Value = castedOption.Value.Humanize() });
                        break;
                    case DHCPv4PacketUInt16Option castedOption:
                        options.Add(new TreeItemData { Name = name, Value = castedOption.Value.ToString() });
                        break;
                    case DHCPv4PacketUInt32Option castedOption:
                        options.Add(new TreeItemData { Name = name, Value = castedOption.Value.ToString() });
                        break;
                    case DHCPv4PacketClientIdentifierOption castedOption:
                        {
                            HashSet<TreeItemData> identifierChilds = new();

                            if(castedOption.Identifier.DUID != null)
                            {
                                identifierChilds.Add(new TreeItemData { Name = "IaId", Value = castedOption.Identifier.IaId.ToString() });
                                identifierChilds.Add(new TreeItemData { Name = "DUID", Value = castedOption.Identifier.DUID.ToFriendlyString() });
                            }

                            if (castedOption.Identifier.HwAddress != null && castedOption.Identifier.HwAddress.Any())
                            {
                                identifierChilds.Add(new TreeItemData { Name = "HW", Value = ByteHelper.ToString(castedOption.Identifier.HwAddress,'-') });
                            }

                            if ( String.IsNullOrEmpty(castedOption.Identifier.IdentifierValue) == false)
                            {
                                identifierChilds.Add(new TreeItemData { Name = "Identifier", Value = castedOption.Identifier.IdentifierValue });
                            }

                            options.Add(new TreeItemData { Name = name, Children = identifierChilds });
                        }
                        break;
                    default:
                        break;
                }
            }

            _items.Add(new TreeItemData { Name = "Options", Children = options });

        }
    }
}
