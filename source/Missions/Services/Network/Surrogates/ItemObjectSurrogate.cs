﻿using Common.Serialization;
using GameInterface.Serialization;
using GameInterface.Serialization.External;
using ProtoBuf;
using TaleWorlds.Core;

namespace Missions.Services.Network.Surrogates
{
    [ProtoContract(SkipConstructor = true)]
    public class ItemObjectSurrogate
    {
        [ProtoMember(1)]
        public byte[] data { get; }

        public ItemObjectSurrogate(ItemObject obj)
        {
            // Required to not overwrite data
            // For some reason protobuf sends 2 character objects
            // and one is null
            if (obj == null) return;

            if (ContainerProvider.TryResolve(out IBinaryPackageFactory packageFactory) == false) return;

            IBinaryPackage package = packageFactory.GetBinaryPackage(obj);

            data = BinaryFormatterSerializer.Serialize(package);
        }

        private ItemObject Deserialize()
        {
            if (data == null) return default;

            if (ContainerProvider.TryResolve(out IBinaryPackageFactory packageFactory) == false) return default;

            var package = BinaryFormatterSerializer.Deserialize<ItemObjectBinaryPackage>(data);

            return package.Unpack<ItemObject>(packageFactory);
        }

        public static implicit operator ItemObjectSurrogate(ItemObject obj)
        {
            return new ItemObjectSurrogate(obj);
        }

        public static implicit operator ItemObject(ItemObjectSurrogate surrogate)
        {
            return surrogate.Deserialize();
        }
    }
}
