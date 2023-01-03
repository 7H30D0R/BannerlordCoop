﻿using Common.Serialization;
using Missions.Messages;
using Missions.Packets.Events;
using Missions.Serialization.Surrogates;
using ProtoBuf.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using Xunit;

namespace IntroductionServerTests
{
    public class NetworkMessageSerializationTest
    {
        [Fact]
        public void Serialize_Test()
        {
            RuntimeTypeModel.Default.SetSurrogate<Vec3, Vec3Surrogate>();
            RuntimeTypeModel.Default.SetSurrogate<Vec2, Vec2Surrogate>();

            var character = (CharacterObject)FormatterServices.GetUninitializedObject(typeof(CharacterObject));
            MissionJoinInfo missionJoinInfo = new MissionJoinInfo(character, default(Guid), default(Vec3));
            EventPacket eventPacket = new EventPacket(missionJoinInfo);
            byte[] bytes = ProtoBufSerializer.Serialize(eventPacket);

            Assert.NotNull(bytes);

            MissionJoinInfo newEvent = (MissionJoinInfo)eventPacket.Event;

        }
    }
}
