﻿using Common.Extensions;
using System;
using System.Reflection;
using TaleWorlds.Core;

namespace GameInterface.Serialization.Impl
{
    /// <summary>
    /// Binary package for BannerComponent
    /// </summary>
    [Serializable]
    public class BannerComponentBinaryPackage : BinaryPackageBase<BannerComponent>
    {
        public BannerComponentBinaryPackage(BannerComponent obj, BinaryPackageFactory binaryPackageFactory) : base(obj, binaryPackageFactory)
        {
        }

        public override void Pack()
        {
            foreach (FieldInfo field in ObjectType.GetAllInstanceFields())
            {
                object obj = field.GetValue(Object);
                StoredFields.Add(field, BinaryPackageFactory.GetBinaryPackage(obj));
            }
        }

        protected override void UnpackInternal()
        {
            TypedReference reference = __makeref(Object);
            foreach (FieldInfo field in StoredFields.Keys)
            {
                field.SetValueDirect(reference, StoredFields[field].Unpack());
            }
        }
    }
}