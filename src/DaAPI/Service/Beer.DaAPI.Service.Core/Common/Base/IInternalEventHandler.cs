﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Beer.DaAPI.Core.Common
{
    public interface IInternalEventHandler
    {
        void Handle(DomainEvent domainEvent);
    }
}
