﻿using Common.Messaging;
using Common.Serialization;
using GameInterface.Serialization;
using GameInterface.Serialization.External;
using ProtoBuf;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Missions.Messages
{
    [ProtoContract(SkipConstructor = true)]
    public class NetworkMissionJoinInfo : INetworkEvent
    {
        [ProtoMember(1)]
        public readonly Guid PlayerId;
        [ProtoMember(2)]
        public readonly Vec3 StartingPosition;

        public CharacterObject CharacterObject
        {
            get { return UnpackCharacter(); }
            set { _packedCharacter = PackCharacter(value); }
        }
        private CharacterObject _characterObject;

        [ProtoMember(3)]
        private byte[] _packedCharacter;

        [ProtoMember(4)]
        public readonly Guid[] UnitId;

        [ProtoMember(5)]
        public readonly Vec3[] UnitStartingPosition;

        [ProtoMember(6)]
        public readonly string[] UnitIdString;

        [ProtoMember(7)]
        public readonly bool IsPlayerAlive;

        public NetworkMissionJoinInfo(CharacterObject characterObject, bool isPlayerAlive, Guid playerId, Vec3 startingPosition, Guid[] unitId, Vec3[] unitStartingPosition, string[] unitIdString)
        {
            PlayerId = playerId;
            StartingPosition = startingPosition;
            CharacterObject = characterObject;
            UnitId = unitId;
            UnitStartingPosition = unitStartingPosition;
            UnitIdString = unitIdString;
            IsPlayerAlive = isPlayerAlive;
        }

        private byte[] PackCharacter(CharacterObject characterObject)
        {
            var factory = new BinaryPackageFactory();
            var character = new CharacterObjectBinaryPackage(characterObject, factory);
            character.Pack();

            return BinaryFormatterSerializer.Serialize(character);
        }

        private CharacterObject UnpackCharacter()
        {
            if (_characterObject != null) return _characterObject;

            var factory = new BinaryPackageFactory();
            var character = BinaryFormatterSerializer.Deserialize<CharacterObjectBinaryPackage>(_packedCharacter);
            character.BinaryPackageFactory = factory;

            _characterObject = character.Unpack<CharacterObject>();

            return _characterObject;
        }
    }
}