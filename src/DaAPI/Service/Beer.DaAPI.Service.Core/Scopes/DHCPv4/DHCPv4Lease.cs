﻿using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Scopes.DHCPv4;
using System;
using System.Collections.Generic;
using System.Text;
using static Beer.DaAPI.Core.Scopes.DHCPv4.DHCPv4LeaseEvents;

namespace Beer.DaAPI.Core.Scopes.DHCPv4
{
    public class DHCPv4Lease : Lease<DHCPv4Lease, IPv4Address>
    {
        #region Properties

        public DHCPv4ClientIdentifier Identifier { get; private set; }
        public override Byte[] ClientUniqueIdentifier => Identifier.DUID != null ? Identifier.DUID.GetAsByteStream() : Identifier.HwAddress;

        #endregion

        #region constructor and factories

        internal DHCPv4Lease(
            Guid id,
            IPv4Address address,
            DateTime start,
            DateTime end,
            TimeSpan renewSpan,
            TimeSpan rebindingSpan,
            DHCPv4ClientIdentifier identifier,
            Byte[] uniqueIdentifier,
            Guid? ancestorId,
            Action<DomainEvent> addtionalApplier
             ) : base(id, address, start, end, renewSpan, rebindingSpan, uniqueIdentifier, ancestorId, addtionalApplier)
        {
            Identifier = identifier;
        }

        #endregion

        #region Methods

        internal override void Renew(TimeSpan lifetime, TimeSpan renewalTime, TimeSpan preferredLifetime, Boolean reset)
        {
            if (reset == false)
            {
                CanRenew(lifetime);
            }

            DateTime now = DateTime.UtcNow;

            base.Apply(new DHCPv4LeaseRenewedEvent(this.Id,
                now + lifetime, renewalTime, preferredLifetime,
                reset));
        }

        internal override void Reactived(TimeSpan lifetime, TimeSpan renewalTime, TimeSpan preferredLifetime)
        {
            CanReactived(lifetime);
            DateTime now = DateTime.UtcNow;

            base.Apply(new DHCPv4LeaseRenewedEvent(this.Id, now + lifetime, renewalTime, preferredLifetime, false));
        }

        internal override void Revoke() => base.Apply(new DHCPv4LeaseRevokedEvent(this.Id));
        internal override void Release() => base.Apply(new DHCPv4LeaseReleasedEvent(this.Id));

        internal override void Suspend(TimeSpan? suspentionTime)
        {
            TimeSpan timeToAdd = GetSuspensionTime(suspentionTime);

            base.Apply(new DHCPv4AddressSuspendedEvent(this.Id, this.Address, DateTime.UtcNow + timeToAdd));
        }

        internal override void Cancel(LeaseCancelReasons reason)
        {
            CanCancel();
            base.Apply(new DHCPv4LeaseCanceledEvent(Id, reason));
        }

        internal override void RemovePendingState()
        {
            CanRemovePendingState();
            base.Apply(new DHCPv4LeaseActivatedEvent(this.Id));
        }

        internal override void Expired()
        {
            CanExpire();
            base.Apply(new DHCPv4LeaseExpiredEvent(this.Id));
        }

        #endregion

        #region When

        protected override void When(DomainEvent domainEvent)
        {
            switch (domainEvent)
            {
                case DHCPv4LeaseRenewedEvent e:
                    HandleRenew(e.End, e.RenewSpan, e.ReboundSpan, e.Reset);
                    break;
                case DHCPv4LeaseRevokedEvent _:
                    HandleRevoked();
                    break;
                case DHCPv4LeaseReleasedEvent _:
                    HandleReleased();
                    break;
                case DHCPv4AddressSuspendedEvent _:
                    HandleSuspended();
                    break;
                case DHCPv4LeaseActivatedEvent _:
                    HandleActivated();
                    break;
                case DHCPv4LeaseCanceledEvent _:
                    HandleCanceled();
                    break;
                case DHCPv4LeaseExpiredEvent _:
                    HandleExpire();
                    break;
                default:
                    break;
            }
        }

        #endregion

    }
}
