using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HAS.Registration.Feature
{
    public abstract class Entity<TId>
    {
        public TId Id { get; protected set; }
        public int Version { get; private set; }

        public Entity()
        {

        }

        public Entity(TId id)
        {
            Id = id;
        }
    }
}
