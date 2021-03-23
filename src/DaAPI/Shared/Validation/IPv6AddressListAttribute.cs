﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Beer.DaAPI.Shared.Validation
{

    public class IPv6AddressListAttribute : IPAddressListAttribute
    {
        public override AddressFamily ValidAddressFamily => AddressFamily.InterNetworkV6;
    }
}
