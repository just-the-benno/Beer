﻿using Beer.DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using static Beer.DaAPI.Core.Scopes.DHCPv4.DHCPv4Lease;

namespace Beer.DaAPI.Core.Scopes.DHCPv4
{
    public static class DHCPv4LeaseEvents
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
       
        public abstract class DHCPv4ScopeRelatedEvent : EntityBasedDomainEvent
        {
            public Guid ScopeId { get; set; }

            protected DHCPv4ScopeRelatedEvent()
            {
            }

            protected DHCPv4ScopeRelatedEvent(Guid id) : base(id)
            {
            }
        }

        [DoNotPersist]
        public class DHCPv4AddressSuspendedEvent : DHCPv4ScopeRelatedEvent
        {
            public IPv4Address Address { get; set; }
            public DateTime SuspendedTill { get; set; }

            public DHCPv4AddressSuspendedEvent()
            {

            }

            public DHCPv4AddressSuspendedEvent(Guid leaseId, IPv4Address address, DateTime suspendTill) : base(leaseId)
            {
                Address = address;
                SuspendedTill = suspendTill;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
        [DoNotPersist]
        public class DHCPv4LeaseActivatedEvent : DHCPv4ScopeRelatedEvent
        {
            public DHCPv4LeaseActivatedEvent()
            {

            }

            public DHCPv4LeaseActivatedEvent(Guid leaseId) : base(leaseId)
            {

            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
        [DoNotPersist]
        public class DHCPv4LeaseCanceledEvent : DHCPv4ScopeRelatedEvent
        {
            public LeaseCancelReasons Reason { get; set; }
            public Boolean ForceRemove { get; set; }

            public DHCPv4LeaseCanceledEvent()
            {

            }

            public DHCPv4LeaseCanceledEvent(Guid leaseId) : this(leaseId, LeaseCancelReasons.NotSpecified)
            {

            }

            public DHCPv4LeaseCanceledEvent(Guid leaseId, LeaseCancelReasons reason) : base(leaseId)
            {
                Reason = reason;
            }

            public DHCPv4LeaseCanceledEvent(Guid leaseId, Guid scopeId, LeaseCancelReasons reason) : this(leaseId, reason)
            {
                ScopeId = scopeId;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
        [DoNotPersist]
        public class DHCPv4LeaseExpiredEvent : DHCPv4ScopeRelatedEvent
        {

            public DHCPv4LeaseExpiredEvent()
            {

            }

            public DHCPv4LeaseExpiredEvent(Guid leaseId) : base(leaseId)
            {
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
        [DoNotPersist]
        public class DHCPv4LeaseCreatedEvent : DHCPv4ScopeRelatedEvent
        {
            #region Properties

            public IPv4Address Address { get; set; }
            public Byte[] ClientIdenfier { get; set; }
            public DateTime StartedAt { get; set; }
            public DateTime ValidUntil { get; set; }
            public Byte[] UniqueIdentifier { get; set; }
            public Guid? AncestorId { get; set; }
            public TimeSpan RenewalTime { get; set; }
            public TimeSpan PreferredLifetime { get; set; }
            public Byte[] ClientMacAddress { get; set; }

            #endregion

            #region Constructor

            #endregion

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
        [DoNotPersist]
        public class DHCPv4LeaseReleasedEvent : DHCPv4ScopeRelatedEvent
        {
            public DHCPv4LeaseReleasedEvent()
            {

            }

            public DHCPv4LeaseReleasedEvent(Guid leaseId) : base(leaseId)
            {

            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
        [DoNotPersist]
        public class DHCPv4LeaseRenewedEvent : DHCPv4ScopeRelatedEvent
        {
            public DateTime End { get; set; }
            public Boolean Reset { get; set; }

            public TimeSpan ReboundSpan { get; set; }
            public TimeSpan RenewSpan { get; set; }

            public DHCPv4LeaseRenewedEvent()
            {

            }

            public DHCPv4LeaseRenewedEvent(Guid leaseId, DateTime end, TimeSpan renewSpan, TimeSpan reboundSpan, Boolean reset)
            {
                EntityId = leaseId;
                End = end;
                Reset = reset;
                RenewSpan = renewSpan;
                ReboundSpan = reboundSpan;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
        [DoNotPersist]
        public class DHCPv4LeaseRevokedEvent : DHCPv4ScopeRelatedEvent
        {
            public DHCPv4LeaseRevokedEvent()
            {

            }

            public DHCPv4LeaseRevokedEvent(Guid leaseId) : base(leaseId)
            {

            }
        }

        [DoNotPersist]
        public class DHCPv4LeaseRemovedEvent : DHCPv4ScopeRelatedEvent
        {
            public DHCPv4LeaseRemovedEvent()
            {

            }

            public DHCPv4LeaseRemovedEvent(Guid leaseId, Guid scopeId) : base(leaseId)
            {
                ScopeId = scopeId;
            }
        }

    }
}
