﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Beer.DaAPI.Core.Common
{
    public abstract class AggregateRootWithEvents : AggregateRoot
    {
        #region Fields

        private readonly List<DomainEvent> _changes = new List<DomainEvent>();
        public Int32 Version { get; private set; } = -1;

        #endregion

        #region Constructor

        protected AggregateRootWithEvents(Guid id) : base(id)
        {

        }

        #endregion

        #region Methods

        protected override void Apply(DomainEvent domainEvent)
        {
            Apply(domainEvent, true);
        }

        protected void Apply(DomainEvent domainEvent, Boolean incrementVersion)
        {
            //EnsureValidState();
            _changes.Add(domainEvent);

            base.Apply(domainEvent);

            if (incrementVersion == true)
            {
                Version++;
            }
        }

        public IEnumerable<DomainEvent> GetChanges() => _changes.ToList().AsEnumerable();
        
        public IEnumerable<DomainEvent> GetChangesToPersists()
        {
            var itemsToSave = from item in _changes
                       let attributes = item.GetType().GetCustomAttributes(typeof(DoNotPersistAttribute), true)
                       where attributes.Any() == false
                       select item;

            return itemsToSave.ToArray();
        }

        public void Load(IEnumerable<DomainEvent> history)
        {
            foreach (var domainEvent in history)
            {
                When(domainEvent);
                Version++;
            }
        }

        public void ClearChanges() => _changes.Clear();

        public virtual void PrepareForDelete()
        {
        }

        public void SetId(Guid id)
        {
            if(Id != Guid.Empty)
            {
                throw new InvalidOperationException("only allowed when root is not initilized");
            }

            Id = id;
        }

        #endregion
    }
}
