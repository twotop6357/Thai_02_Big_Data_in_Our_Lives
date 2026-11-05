using System;
using System.Collections.Generic;
using Exhibition.Blocks;
using NUnit.Framework;

public sealed class RfidConfigurationTests
{
    [Test] public void DefaultIpOrderMatchesFourPhysicalPositions()
    {
        var config = new RfidConfiguration(); config.Validate();
        for (int i = 0; i < 4; i++)
        {
            Assert.AreEqual(i, config.readers[i].slot);
            Assert.AreEqual("192.168.0." + (121 + i), config.readers[i].ip);
        }
        Assert.AreEqual(10123, config.listenPort);
        Assert.AreEqual("UidBurst", config.format.framing);
        Assert.AreEqual(16, config.format.fixedFrameBytes);
        Assert.IsEmpty(config.cards, "Actual card IDs must be registered on site.");
    }
    [TestCase("aa bb:00-12", "AABB0012")]
    [TestCase("00000001", "00000001")]
    public void HexPreservesLeadingZeroBytes(string input, string expected) => Assert.AreEqual(expected, RfidHex.Normalize(input));
    [TestCase("")] [TestCase("ABC")] [TestCase("GG")] [TestCase("0xA0")] [TestCase("--")]
    public void InvalidHexIsRejected(string input) => Assert.Throws<FormatException>(() => RfidHex.Normalize(input));
    [Test] public void DuplicateUidWithDifferentSpacingIsRejected()
    {
        var config = new RfidConfiguration { cards = new[] {
            new RfidCardMapping { uid = "AA BB", kind = "Group", id = "child" },
            new RfidCardMapping { uid = "aabb", kind = "Product", id = "bags" }
        } };
        Assert.Throws<ArgumentException>(() => config.Validate());
    }
    [Test] public void DuplicateSlotOrEndpointIsRejected()
    {
        var config = new RfidConfiguration(); config.readers[3].slot = 0;
        Assert.Throws<ArgumentException>(() => config.Validate());
        config = new RfidConfiguration(); config.readers[3].ip = config.readers[0].ip;
        Assert.Throws<ArgumentException>(() => config.Validate());
    }
    [Test] public void RemovalCodeCannotBeMappedAsACard()
    {
        var config = new RfidConfiguration(); config.format.removedValues = new[] { "0000" };
        config.cards = new[] { new RfidCardMapping { uid = "0000", kind = "Color", id = "red" } };
        Assert.Throws<ArgumentException>(() => config.Validate());
    }
    [TestCase(-1)] [TestCase(65536)] [TestCase(0)]
    public void InvalidPortIsRejected(int port)
    {
        var config = new RfidConfiguration { listenPort = port };
        Assert.Throws<ArgumentException>(() => config.Validate());
    }
    [TestCase(99)] [TestCase(60001)] [TestCase(0)]
    public void InvalidCheckIntervalIsRejected(int milliseconds) =>
        Assert.Throws<ArgumentException>(() => new RfidPresenceSettings { checkIntervalMilliseconds = milliseconds }.Validate());
    [Test] public void DefaultCheckIntervalIsOneSecond()
    {
        var presence = new RfidPresenceSettings(); presence.Validate();
        Assert.AreEqual(1000, presence.checkIntervalMilliseconds);
    }
    [Test] public void PreviousClientModeVersionIsRejected()
    {
        var config = new RfidConfiguration { version = 1 };
        Assert.Throws<ArgumentException>(() => config.Validate());
    }
    [Test] public void SourceAddressSelectsItsConfiguredSlotOnly()
    {
        var config = new RfidConfiguration(); config.Validate();
        for (int i = 0; i < 4; i++)
            Assert.AreEqual(i, config.FindReader(System.Net.IPAddress.Parse("192.168.0." + (121 + i))).slot);
        Assert.IsNull(config.FindReader(System.Net.IPAddress.Parse("192.168.0.125")));
    }
    [Test] public void FixedFramesSurviveTcpFragmentationAndCoalescing()
    {
        var decoder = new RfidFrameDecoder(new RfidWireFormat { framing = "FixedLength", fixedFrameBytes = 4 });
        var frames = new List<byte[]>();
        decoder.Feed(new byte[] { 0, 1 }, 2, 0, frames.Add); Assert.IsEmpty(frames);
        decoder.Feed(new byte[] { 2, 3, 4, 5, 6, 7, 8 }, 7, 1, frames.Add);
        Assert.AreEqual(2, frames.Count); CollectionAssert.AreEqual(new byte[] { 0, 1, 2, 3 }, frames[0]);
        CollectionAssert.AreEqual(new byte[] { 4, 5, 6, 7 }, frames[1]);
        decoder.Feed(new byte[] { 9, 10, 11 }, 3, 2, frames.Add);
        CollectionAssert.AreEqual(new byte[] { 8, 9, 10, 11 }, frames[2]);
    }
    [Test] public void DelimiterCanSpanReadsAndSplitsMultipleFrames()
    {
        var decoder = new RfidFrameDecoder(new RfidWireFormat { framing = "Delimiter", delimiterHex = "0D0A" });
        var frames = new List<byte[]>();
        decoder.Feed(new byte[] { 1, 2, 13 }, 3, 0, frames.Add);
        decoder.Feed(new byte[] { 10, 3, 13, 10 }, 4, 1, frames.Add);
        Assert.AreEqual(2, frames.Count); CollectionAssert.AreEqual(new byte[] { 1, 2 }, frames[0]);
        CollectionAssert.AreEqual(new byte[] { 3 }, frames[1]);
    }
    [Test] public void IdleGapWaitsForWholeBurst()
    {
        var decoder = new RfidFrameDecoder(new RfidWireFormat { framing = "IdleGap", idleGapMilliseconds = 100 });
        var frames = new List<byte[]>();
        decoder.Feed(new byte[] { 1, 2 }, 2, 0, frames.Add);
        decoder.Feed(new byte[] { 3 }, 1, 50, frames.Add);
        decoder.FlushIdle(149, frames.Add); Assert.IsEmpty(frames);
        decoder.FlushIdle(150, frames.Add); Assert.AreEqual(1, frames.Count);
        CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, frames[0]);
    }
    [Test] public void OversizedFrameIsDiscardedAndNextFrameRecovers()
    {
        var decoder = new RfidFrameDecoder(new RfidWireFormat { framing = "Delimiter", delimiterHex = "0D0A" });
        var frames = new List<byte[]>(); var oversized = new byte[700];
        decoder.Feed(oversized, oversized.Length, 0, frames.Add);
        decoder.Feed(new byte[] { 13, 10, 4, 5, 13, 10 }, 6, 1, frames.Add);
        Assert.AreEqual(1, frames.Count); CollectionAssert.AreEqual(new byte[] { 4, 5 }, frames[0]);
    }
    [Test] public void UidExtractionCanExcludeHeaderChecksumAndDecodeAsciiHex()
    {
        var format = new RfidWireFormat { uidOffsetBytes = 2, uidLengthBytes = 4 };
        Assert.AreEqual("0011AABB", format.Extract(new byte[] { 0xAA, 0x08, 0, 0x11, 0xAA, 0xBB, 0xFF }));
        format.asciiHex = true;
        Assert.AreEqual("0011AABB", format.Extract(System.Text.Encoding.ASCII.GetBytes("AA080011AABBFF")));
        Assert.Throws<FormatException>(() => format.Extract(System.Text.Encoding.ASCII.GetBytes("AA")));
    }
    [Test] public void ReconnectDoesNotCombineAnOldPartialFrameWithANewConnection()
    {
        var format = new RfidWireFormat { framing = "FixedLength", fixedFrameBytes = 2 };
        var old = new RfidFrameDecoder(format); var frames = new List<byte[]>();
        old.Feed(new byte[] { 1 }, 1, 0, frames.Add);
        var next = new RfidFrameDecoder(format); next.Feed(new byte[] { 2, 3 }, 2, 1, frames.Add);
        Assert.AreEqual(1, frames.Count); CollectionAssert.AreEqual(new byte[] { 2, 3 }, frames[0]);
    }
    [Test] public void SixteenByteOpaqueValueIsKeptWholeWithoutAsciiOrHeaderInterpretation()
    {
        const string sample = "08 41 FF 54 33 01 4B 31 30 32 30 00 00 39 38 EF";
        var config = new RfidConfiguration(); config.Validate();
        var bytes = RfidHex.Bytes(sample); var frames = new List<byte[]>();
        var decoder = new RfidFrameDecoder(config.format);
        decoder.Feed(bytes, 16, 0, frames.Add); decoder.FlushIdle(100, frames.Add);
        Assert.AreEqual(1, frames.Count);
        Assert.AreEqual("0841FF5433014B3130323000003938EF", config.format.Extract(frames[0]));
        Assert.IsEmpty(config.cards, "The sample is not a real card registration.");
    }

    [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(8)]
    public void RepeatedSevenByteValueIsOneUidWithoutAFixedFrameSize(int copies)
    {
        // Synthetic value, with the same seven-byte shape as the equipment report.
        var one = RfidHex.Bytes("0504A1736922D5");
        var burst = new List<byte>(); for (int i = 0; i < copies; i++) burst.AddRange(one);
        var format = new RfidWireFormat();
        Assert.AreEqual("0504A1736922D5", format.Extract(burst.ToArray(), out int reports));
        Assert.AreEqual(copies, reports);
    }

    [Test] public void BurstFramingSurvivesEveryTcpSplitOfTwoRepeatedUids()
    {
        var bytes = RfidHex.Bytes("0504A1736922D50504A1736922D5");
        for (int split = 1; split < bytes.Length; split++)
        {
            var format = new RfidWireFormat(); var decoder = new RfidFrameDecoder(format);
            var frames = new List<byte[]>();
            var first = new byte[split]; var last = new byte[bytes.Length - split];
            Array.Copy(bytes, first, split); Array.Copy(bytes, split, last, 0, last.Length);
            decoder.Feed(first, first.Length, 0, frames.Add);
            decoder.Feed(last, last.Length, 70, frames.Add); decoder.FlushIdle(169, frames.Add);
            Assert.IsEmpty(frames, "Do not turn a TCP fragment into a card.");
            decoder.FlushIdle(170, frames.Add);
            Assert.AreEqual(1, frames.Count);
            Assert.AreEqual("0504A1736922D5", format.Extract(frames[0]));
        }
    }

    [TestCase("0001A2B3")] [TestCase("0504A1736922D5")]
    [TestCase("00010203040506070809")] [TestCase("000102030405060708090A0B0C0D0E0F")]
    public void UidLengthAndLeadingZeroesAreNotForcedToSixteenBytes(string uid)
    {
        var format = new RfidWireFormat(); var decoder = new RfidFrameDecoder(format);
        var frames = new List<byte[]>(); var bytes = RfidHex.Bytes(uid + uid);
        decoder.Feed(bytes, bytes.Length, 0, frames.Add); decoder.FlushIdle(100, frames.Add);
        Assert.AreEqual(uid, format.Extract(frames[0]));
    }

    [Test] public void CardChangesDoNotMixBytesAcrossQuietDelimitedBursts()
    {
        var format = new RfidWireFormat(); var decoder = new RfidFrameDecoder(format); var frames = new List<byte[]>();
        var a = RfidHex.Bytes("0504A1736922D50504A1736922D5"); var b = RfidHex.Bytes("0001A2B30001A2B3");
        decoder.Feed(a, a.Length, 0, frames.Add); decoder.Feed(b, b.Length, 150, frames.Add); decoder.FlushIdle(250, frames.Add);
        Assert.AreEqual(2, frames.Count); Assert.AreEqual("0504A1736922D5", format.Extract(frames[0]));
        Assert.AreEqual("0001A2B3", format.Extract(frames[1]));
    }

    [Test] public void NewSingleValueNeedsTwoReportsAndReconnectResetsConfirmation()
    {
        var stable = new RfidUidStability();
        Assert.IsFalse(stable.Observe("0001A2B3", 1)); Assert.IsTrue(stable.Observe("0001A2B3", 1));
        Assert.IsTrue(stable.Observe("0001A2B3", 1)); Assert.IsFalse(stable.Observe("0004A2B3", 1));
        Assert.IsTrue(stable.Observe("0004A2B3", 1)); stable.Reset(); Assert.IsFalse(stable.Observe("0004A2B3", 1));
        Assert.IsTrue(stable.Observe("0504A1736922D5", 2));
    }

    [Test] public void InvalidOrOverlongBurstCannotBeSavedAsATruncatedUid()
    {
        var decoder = new RfidFrameDecoder(new RfidWireFormat()); var frames = new List<byte[]>();
        var bytes = new byte[600]; decoder.Feed(bytes, bytes.Length, 0, frames.Add); decoder.FlushIdle(100, frames.Add);
        Assert.IsEmpty(frames);
        var valid = RfidHex.Bytes("0504A1736922D50504A1736922D5");
        decoder.Feed(valid, valid.Length, 200, frames.Add); decoder.FlushIdle(300, frames.Add);
        Assert.AreEqual(1, frames.Count); Assert.AreEqual("0504A1736922D5", new RfidWireFormat().Extract(frames[0]));
    }
}
