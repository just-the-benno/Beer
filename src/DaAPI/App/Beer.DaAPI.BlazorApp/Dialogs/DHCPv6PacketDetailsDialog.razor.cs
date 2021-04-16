using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Packets.DHCPv6;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using Beer.DaAPI.BlazorApp.Helper;
using Beer.DaAPI.BlazorApp.ModelHelper;

namespace Beer.DaAPI.BlazorApp.Dialogs
{
    public partial class DHCPv6PacketDetailsDialog : DaAPIDialogBase
    {
        [Parameter] public DHCPv6Packet Packet { get; set; }

        private HashSet<TreeItemData> _items;

        private void GetTreeFromPacket(DHCPv6Packet packet, ICollection<TreeItemData> collection)
        {
            collection.Add(new TreeItemData { Name = "Transaction Id", Value = packet.TransactionId.ToString() });
            collection.Add(new TreeItemData { Name = "Packet Type", Value = packet.PacketType.ToString() });

            var options = new HashSet<TreeItemData>();

            var optionItem = new TreeItemData { Name = "Options", Children = options };
            collection.Add(optionItem);


            foreach (var item in packet.Options)
            {
                String name = $"{_codeToNameConverter.GetName(item.Code)} ({item.Code})";

                switch (item)
                {
                    case DHCPv6PacketIdentifierOption castedOption:
                        options.Add(new TreeItemData { Name = name, Value = castedOption.DUID.ToFriendlyString() });
                        break;
                    case DHCPv6PacketReconfigureOption castedOption:
                        options.Add(new TreeItemData { Name = name, Value = castedOption.MessageType.ToString() });
                        break;
                    case DHCPv6PacketByteArrayOption castedOption:
                        options.Add(new TreeItemData { Name = name, Value = ByteHelper.ToString(castedOption.Data) });

                        break;
                    case DHCPv6PacketVendorSpecificInformationOption castedOption:
                        {
                            var subOptions = new HashSet<TreeItemData>
                            {
                                new TreeItemData { Name = "Enterprise", Value = castedOption.EnterpriseNumber.ToString() },
                                new TreeItemData
                                {
                                    Name = "Options",
                                    Children =
                                        castedOption.Options.Select(x => new TreeItemData { Name = x.Code.ToString(), Value = ByteHelper.ToString(x.Data) }).ToHashSet()
                                }
                            };

                            options.Add(new TreeItemData { Name = name, Children = subOptions });
                        }
                        break;
                    case DHCPv6PacketVendorClassOption castedOption:
                        {
                            Int32 index = 1;
                            var subOptions = new HashSet<TreeItemData>
                            {
                                new TreeItemData { Name = "Enterprise", Value = castedOption.EnterpriseNumber.ToString() },
                                new TreeItemData
                                {
                                    Name = "Options",
                                    Children =
                                        castedOption.VendorClassData.Select(x => new TreeItemData { Name = (index++).ToString(), Value = ByteHelper.ToString(x) }).ToHashSet()
                                }
                            };

                            options.Add(new TreeItemData { Name = name, Children = subOptions });
                        }
                        break;
                    case DHCPv6PacketUserClassOption castedOption:
                        {
                            Int32 index = 1;
                            var subOptions = new HashSet<TreeItemData>
                            {
                                new TreeItemData
                                {
                                    Name = "Options",
                                    Children =
                                        castedOption.UserClassData.Select(x => new TreeItemData { Name = (index++).ToString(), Value = ByteHelper.ToString(x) }).ToHashSet()
                                }
                            };

                            options.Add(new TreeItemData { Name = name, Children = subOptions });
                        }
                        break;
                    case DHCPv6PacketUInt32Option castedOption:
                        options.Add(new TreeItemData { Name = name, Value = castedOption.Value.ToString() });
                        break;
                    case DHCPv6PacketUInt16Option castedOption:
                        options.Add(new TreeItemData { Name = name, Value = castedOption.Value.ToString() });
                        break;
                    case DHCPv6PacketByteOption castedOption:
                        options.Add(new TreeItemData { Name = name, Value = castedOption.Value.ToString() });
                        break;
                    case DHCPv6PacketTimeOption castedOption:
                        options.Add(new TreeItemData { Name = name, Value = castedOption.Value.Humanize() });
                        break;
                    case DHCPv6PacketTrueOption castedOption:
                        options.Add(new TreeItemData { Name = name });
                        break;
                    case DHCPv6PacketBooleanOption castedOption:
                        options.Add(new TreeItemData { Name = name, Value = castedOption.Value.ToString() });
                        break;
                    case DHCPv6PacketIPAddressOption castedOption:
                        options.Add(new TreeItemData { Name = name, Value = castedOption.Address.ToString() });
                        break;
                    case DHCPv6PacketIPAddressListOption castedOption:
                        {
                            Int32 index = 1;
                            var subOptions = new HashSet<TreeItemData>
                            {
                                new TreeItemData
                                {
                                    Name = "Addesses",
                                    Children =
                                        castedOption.Addresses.Select(x => new TreeItemData { Name = (index++).ToString(), Value = x.ToString() }).ToHashSet()
                                }
                            };

                            options.Add(new TreeItemData { Name = name, Children = subOptions });
                        }
                        break;
                    case DHCPv6PacketRemoteIdentifierOption castedOption:
                        options.Add(new TreeItemData { Name = name, Value = ByteHelper.ToString(castedOption.Value) });
                        break;
                    case DHCPv6PacketOptionRequestOption castedOption:
                        {
                            var subOptions = new HashSet<TreeItemData>
                            {
                                new TreeItemData
                                {
                                    Name = "Options",
                                    Children =
                                        castedOption.RequestedOptions.Select(x => new TreeItemData { Name = x.ToString(), Value = _codeToNameConverter.GetName(x) }).ToHashSet()
                                }
                            };

                            options.Add(new TreeItemData { Name = name, Children = subOptions });
                        }
                        break;
                    case DHCPv6PacketIdentityAssociationNonTemporaryAddressesOption castedOption:
                        {
                            var subOptions = new HashSet<TreeItemData>
                            {
                                new TreeItemData { Name = "Id", Value = castedOption.Id.ToString() },
                                new TreeItemData { Name = "T1", Value = castedOption.T1.Humanize() },
                                new TreeItemData { Name = "T2", Value = castedOption.T2.Humanize() }
                            };

                            var secondLevelSuboptions = new HashSet<TreeItemData>();

                            Int32 index = 0;

                            foreach (var addressSubOption in castedOption.Suboptions.OfType<DHCPv6PacketIdentityAssociationAddressSuboption>())
                            {
                                var subOptionTreeItem = new TreeItemData { Name = (index++).ToString(), Children = new() };
                                secondLevelSuboptions.Add(subOptionTreeItem);

                                AddDHCPv6PacketIdentityAssociationAddressSuboption(addressSubOption, subOptionTreeItem);
                            }

                            if (secondLevelSuboptions.Count > 0)
                            {
                                subOptions.Add(new TreeItemData { Name = "Address sub options", Children = secondLevelSuboptions });
                            }

                            options.Add(new TreeItemData { Name = name, Children = subOptions });
                        }
                        break;
                    case DHCPv6PacketIdentityAssociationTemporaryAddressesOption castedOption:
                        {
                            var subOptions = new HashSet<TreeItemData>
                            {
                                new TreeItemData { Name = "Id", Value = castedOption.Id.ToString() }
                            };

                            var secondLevelSuboptions = new HashSet<TreeItemData>();

                            Int32 index = 0;

                            foreach (var addressSubOption in castedOption.Suboptions.OfType<DHCPv6PacketIdentityAssociationAddressSuboption>())
                            {
                                var subOptionTreeItem = new TreeItemData { Name = (index++).ToString(), Children = new() };
                                secondLevelSuboptions.Add(subOptionTreeItem);

                                AddDHCPv6PacketIdentityAssociationAddressSuboption(addressSubOption, subOptionTreeItem);
                            }

                            if (secondLevelSuboptions.Count > 0)
                            {
                                subOptions.Add(new TreeItemData { Name = "Address sub options", Children = secondLevelSuboptions });
                            }

                            options.Add(new TreeItemData { Name = name, Children = subOptions });
                        }
                        break;
                    case DHCPv6PacketIdentityAssociationPrefixDelegationOption castedOption:
                        {
                            var subOptions = new HashSet<TreeItemData>
                            {
                                new TreeItemData { Name = "Id", Value = castedOption.Id.ToString() },
                                new TreeItemData { Name = "T1", Value = castedOption.T1.Humanize() },
                                new TreeItemData { Name = "T2", Value = castedOption.T2.Humanize() }
                            };

                            var secondLevelSuboptions = new HashSet<TreeItemData>();

                            Int32 index = 0;

                            foreach (var addressSubOption in castedOption.Suboptions.OfType<DHCPv6PacketIdentityAssociationPrefixDelegationSuboption>())
                            {
                                var subOptionTreeItem = new TreeItemData { Name = (index++).ToString(), Children = new() };
                                secondLevelSuboptions.Add(subOptionTreeItem);

                                AddDHCPv6PacketIdentityAssociationAddressrefixSuboption(addressSubOption, subOptionTreeItem);
                            }

                            if (secondLevelSuboptions.Count > 0)
                            {
                                subOptions.Add(new TreeItemData { Name = "Address sub options", Children = secondLevelSuboptions });
                            }

                            options.Add(new TreeItemData { Name = name, Children = subOptions });
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private static void AddDHCPv6PacketIdentityAssociationAddressSuboption(DHCPv6PacketIdentityAssociationAddressSuboption addressSubOption, TreeItemData item)
        {
            item.Children.Add(new TreeItemData { Name = "Address", Value = addressSubOption.Address.ToString() });
            item.Children.Add(new TreeItemData { Name = "Preferred Lifetime", Value = addressSubOption.PreferredLifetime.ToString() });
            item.Children.Add(new TreeItemData { Name = "Valid Lifetime", Value = addressSubOption.ValidLifetime.ToString() });

            var statusCodeItems = new HashSet<TreeItemData>();

            foreach (var addre in addressSubOption.Suboptions.OfType<DHCPv6PacketStatusCodeSuboption>())
            {
                statusCodeItems.Add(new TreeItemData { Name = "Code", Value = addre.Code.ToString() });
                statusCodeItems.Add(new TreeItemData { Name = "Message", Value = addre.Message.ToString() });
            }

            if (statusCodeItems.Count > 0)
            {
                item.Children.Add(new TreeItemData { Name = "Status Code", Children = statusCodeItems });
            }
        }

        private static void AddDHCPv6PacketIdentityAssociationAddressrefixSuboption(DHCPv6PacketIdentityAssociationPrefixDelegationSuboption addressSubOption, TreeItemData item)
        {
            item.Children.Add(new TreeItemData { Name = "Address", Value = addressSubOption.Address.ToString() });
            item.Children.Add(new TreeItemData { Name = "Prefix Length", Value = addressSubOption.PrefixLength.ToString() });
            item.Children.Add(new TreeItemData { Name = "Preferred Lifetime", Value = addressSubOption.PreferredLifetime.ToString() });
            item.Children.Add(new TreeItemData { Name = "Valid Lifetime", Value = addressSubOption.ValidLifetime.ToString() });

            var statusCodeItems = new HashSet<TreeItemData>();

            foreach (var addre in addressSubOption.Suboptions.OfType<DHCPv6PacketStatusCodeSuboption>())
            {
                statusCodeItems.Add(new TreeItemData { Name = "Code", Value = addre.Code.ToString() });
                statusCodeItems.Add(new TreeItemData { Name = "Message", Value = addre.Message.ToString() });
            }

            if (statusCodeItems.Count > 0)
            {
                item.Children.Add(new TreeItemData { Name = "Status Code", Children = statusCodeItems });
            }
        }

        private void GetTreeFromRelayPacket(DHCPv6RelayPacket relayPacket, ICollection<TreeItemData> collection)
        {
            collection.Add(new TreeItemData { Name = "Hop Count", Value = relayPacket.HopCount.ToString() });
            collection.Add(new TreeItemData { Name = "Link Address", Value = relayPacket.LinkAddress.ToString() });
            collection.Add(new TreeItemData { Name = "Peer Address", Value = relayPacket.PeerAddress.ToString() });
            collection.Add(new TreeItemData { Name = "Packet Type", Value = relayPacket.PacketType.ToString() });

            var innerPacket = relayPacket.InnerPacket;

            var children = new HashSet<TreeItemData>();
            var item = new TreeItemData { Name = "Inner Packet", Children = children };
            collection.Add(item);

            if (innerPacket is DHCPv6RelayPacket innerRelayedPacket)
            {
                GetTreeFromRelayPacket(innerRelayedPacket, children);
            }
            else if (innerPacket is DHCPv6Packet innersPacket)
            {
                GetTreeFromPacket(innersPacket, children);
            }
        }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            if (Packet == null) { return; }

            _items = new HashSet<TreeItemData>
            {
                new TreeItemData { Name = "Source", Value = Packet.Header.Source.ToString() },
                new TreeItemData { Name = "Destionation", Value = Packet.Header.Destionation.ToString() }
            };

            if (Packet is DHCPv6RelayPacket relayPacket)
            {
                GetTreeFromRelayPacket(relayPacket, _items);
            }
            else
            {
                GetTreeFromPacket(Packet, _items);
            }
        }

    }
}
