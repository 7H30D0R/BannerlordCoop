﻿using Common.Extensions;
using System;
using System.Reflection;
using TaleWorlds.Core;

namespace GameInterface.Serialization.External
{
    [Serializable]
    public class WeaponComponentDataBinaryPackage : BinaryPackageBase<WeaponComponentData>
    {
        public WeaponComponentDataBinaryPackage(WeaponComponentData obj, BinaryPackageFactory binaryPackageFactory) : base(obj, binaryPackageFactory)
        {
        }
    }
}