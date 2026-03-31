using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Text;
using Ufex.FileType.Classifiers;

namespace Ufex.FileType.Tests;

[TestClass]
public class SignatureClassifierTests
{
    [TestMethod]
    public void DetectFileType_MatchesSimpleBytesSignature()
    {
        using TestHarness harness = new TestHarness(
            """
            <Ufex>
                <FileTypes>
                    <FileType>
                        <ID>FT_SIMPLE</ID>
                        <Description>Simple bytes signature</Description>
                        <Signatures>
                            <Signature>
                                <Rule type="bytes" offset="0">0xDEADBEEF</Rule>
                            </Signature>
                        </Signatures>
                    </FileType>
                </FileTypes>
            </Ufex>
            """);

        string[] matches = harness.Detect([0xDE, 0xAD, 0xBE, 0xEF, 0x00]);

        CollectionAssert.AreEquivalent(new string[] { "FT_SIMPLE" }, matches);
    }

    [TestMethod]
    public void DetectFileType_ChildDoesNotMatchWhenParentFails()
    {
        using TestHarness harness = new TestHarness(
            """
            <Ufex>
                <FileTypes>
                    <FileType>
                        <ID>FT_PARENT</ID>
                        <Description>Parent type</Description>
                        <Signatures>
                            <Signature>
                                <Rule type="byte" offset="0">0xAA</Rule>
                            </Signature>
                        </Signatures>
                    </FileType>
                    <FileType>
                        <ID>FT_CHILD</ID>
                        <ParentID>FT_PARENT</ParentID>
                        <Description>Child type</Description>
                        <Signatures>
                            <Signature>
                                <Rule type="byte" offset="1">0xBB</Rule>
                            </Signature>
                        </Signatures>
                    </FileType>
                </FileTypes>
            </Ufex>
            """);

        string[] matches = harness.Detect([0x00, 0xBB]);

        Assert.AreEqual(0, matches.Length);
    }

    [TestMethod]
    public void DetectFileTypeDetailed_IncludesSignatureDetails()
    {
        using TestHarness harness = new TestHarness(
            """
            <Ufex>
                <FileTypes>
                    <FileType>
                        <ID>FT_TEXT_UTF8</ID>
                        <Description>UTF-8 text marker</Description>
                        <Signatures>
                            <Signature>
                                <Rule type="string:utf-8" offset="0">UFEX</Rule>
                            </Signature>
                        </Signatures>
                    </FileType>
                </FileTypes>
            </Ufex>
            """);

        DetectionMatch[] matches = harness.DetectDetailed([0x55, 0x46, 0x45, 0x58]);

        Assert.AreEqual(1, matches.Length);
        Assert.AreEqual("FT_TEXT_UTF8", matches[0].FileType.ID);
        Assert.AreEqual(MatchMethod.Signature, matches[0].Method);
        Assert.AreEqual(1, matches[0].SignatureDetails.Count);
        Assert.AreEqual(0, matches[0].SignatureDetails[0].SignatureIndex);
        Assert.IsTrue(matches[0].SignatureDetails[0].MatchedRules.Any(x => x.Contains("Rule(type=string:utf-8")));
    }

    [TestMethod]
    public void DetectFileType_SearchRuleRegexHonorsMaxLengthWindow()
    {
        using TestHarness harness = new TestHarness(
            """
            <Ufex>
                <FileTypes>
                    <FileType>
                        <ID>FT_REGEX_WINDOW</ID>
                        <Description>Regex search with maxLength</Description>
                        <Signatures>
                            <Signature>
                                <SearchRule type="regex" offset="0" maxLength="4">ABCD</SearchRule>
                            </Signature>
                        </Signatures>
                    </FileType>
                </FileTypes>
            </Ufex>
            """);

        string[] outOfRangeMatches = harness.Detect([0x7A, 0x7A, 0x41, 0x42, 0x43, 0x44]);
        string[] inRangeMatches = harness.Detect([0x41, 0x42, 0x43, 0x44, 0x7A]);

        Assert.AreEqual(0, outOfRangeMatches.Length);
        CollectionAssert.AreEquivalent(new string[] { "FT_REGEX_WINDOW" }, inRangeMatches);
    }

    [TestMethod]
    public void DetectFileType_DocxRegexWith46ByteGap_DoesNotMatchValidCentralDirectoryEntry()
    {
        using TestHarness harness = new TestHarness(
            """
            <Ufex>
                <FileTypes>
                    <FileType>
                        <ID>DOCS_MS_DOCX</ID>
                        <Description>Microsoft Word Document</Description>
                        <Signatures>
                            <Signature>
                                <SearchRule type="regex" offset="0" maxLength="51200">\x50\x4B\x01\x02[\s\S]{46}word/</SearchRule>
                            </Signature>
                        </Signatures>
                    </FileType>
                </FileTypes>
            </Ufex>
            """);

        byte[] sample = BuildCentralDirectoryEntry("word/document.xml");
        string[] matches = harness.Detect(sample);

        Assert.AreEqual(0, matches.Length);
    }

    [TestMethod]
    public void DetectFileType_DocxRegexWith42ByteGap_MatchesValidCentralDirectoryEntry()
    {
        using TestHarness harness = new TestHarness(
            """
            <Ufex>
                <FileTypes>
                    <FileType>
                        <ID>DOCS_MS_DOCX</ID>
                        <Description>Microsoft Word Document</Description>
                        <Signatures>
                            <Signature>
                                <SearchRule type="regex" offset="0" maxLength="51200">\x50\x4B\x01\x02[\s\S]{42}word/</SearchRule>
                            </Signature>
                        </Signatures>
                    </FileType>
                </FileTypes>
            </Ufex>
            """);

        byte[] sample = BuildCentralDirectoryEntry("word/document.xml");
        string[] matches = harness.Detect(sample);

        CollectionAssert.AreEquivalent(new string[] { "DOCS_MS_DOCX" }, matches);
    }

    [TestMethod]
    public void DetectFileType_DocxMatchesWithFullParentChain()
    {
        // Mirrors the real config: ARCH_ZIP -> BASE_OPC -> DOCS_MS_DOCX
        using TestHarness harness = new TestHarness(
            """
            <Ufex>
                <FileTypes>
                    <FileType>
                        <ID>ARCH_ZIP</ID>
                        <Description>ZIP Archive</Description>
                        <Signatures>
                            <Signature>
                                <Rule type="bytes" offset="0">0x504B0304</Rule>
                            </Signature>
                        </Signatures>
                    </FileType>
                    <FileType>
                        <ID>BASE_OPC</ID>
                        <ParentID>ARCH_ZIP</ParentID>
                        <Description>OPC Container</Description>
                        <Signatures>
                            <Signature>
                                <SearchRule type="regex" offset="0" maxLength="51200">\x50\x4B\x01\x02[\s\S]{42}\[Content_Types\]\.xml</SearchRule>
                            </Signature>
                        </Signatures>
                    </FileType>
                    <FileType>
                        <ID>DOCS_MS_DOCX</ID>
                        <ParentID>BASE_OPC</ParentID>
                        <Description>Microsoft Word Document</Description>
                        <Signatures>
                            <Signature>
                                <SearchRule type="regex" offset="0" maxLength="51200">\x50\x4B\x01\x02[\s\S]{42}word/</SearchRule>
                            </Signature>
                        </Signatures>
                    </FileType>
                </FileTypes>
            </Ufex>
            """);

        byte[] sample = BuildMinimalDocxZip();
        string[] matches = harness.Detect(sample);

        CollectionAssert.Contains(matches, "DOCS_MS_DOCX");
        CollectionAssert.Contains(matches, "BASE_OPC");
        CollectionAssert.Contains(matches, "ARCH_ZIP");
    }

    [TestMethod]
    public void DetectFileType_DocxFailsWhenParentOpcRegexHas46ByteGap()
    {
        // Demonstrates that a 46-byte gap in the OPC regex breaks the whole chain
        using TestHarness harness = new TestHarness(
            """
            <Ufex>
                <FileTypes>
                    <FileType>
                        <ID>ARCH_ZIP</ID>
                        <Description>ZIP Archive</Description>
                        <Signatures>
                            <Signature>
                                <Rule type="bytes" offset="0">0x504B0304</Rule>
                            </Signature>
                        </Signatures>
                    </FileType>
                    <FileType>
                        <ID>BASE_OPC</ID>
                        <ParentID>ARCH_ZIP</ParentID>
                        <Description>OPC Container</Description>
                        <Signatures>
                            <Signature>
                                <SearchRule type="regex" offset="0" maxLength="51200">\x50\x4B\x01\x02[\s\S]{46}\[Content_Types\]\.xml</SearchRule>
                            </Signature>
                        </Signatures>
                    </FileType>
                    <FileType>
                        <ID>DOCS_MS_DOCX</ID>
                        <ParentID>BASE_OPC</ParentID>
                        <Description>Microsoft Word Document</Description>
                        <Signatures>
                            <Signature>
                                <SearchRule type="regex" offset="0" maxLength="51200">\x50\x4B\x01\x02[\s\S]{42}word/</SearchRule>
                            </Signature>
                        </Signatures>
                    </FileType>
                </FileTypes>
            </Ufex>
            """);

        byte[] sample = BuildMinimalDocxZip();
        string[] matches = harness.Detect(sample);

        // DOCX's own regex matches, but BASE_OPC's regex fails (46-byte gap is wrong),
        // so the parent chain breaks — DOCX is NOT reported.
        CollectionAssert.DoesNotContain(matches, "DOCS_MS_DOCX");
        CollectionAssert.DoesNotContain(matches, "BASE_OPC");
        // ZIP still matches (no parent dependency)
        CollectionAssert.Contains(matches, "ARCH_ZIP");
    }

    private static byte[] BuildCentralDirectoryEntry(string fileName)
    {
        byte[] fileNameBytes = Encoding.ASCII.GetBytes(fileName);
        byte[] fixedHeaderAfterSignature = new byte[42];

        // File name length field in central directory fixed header.
        fixedHeaderAfterSignature[24] = (byte)(fileNameBytes.Length & 0xFF);
        fixedHeaderAfterSignature[25] = (byte)((fileNameBytes.Length >> 8) & 0xFF);

        byte[] bytes = new byte[4 + fixedHeaderAfterSignature.Length + fileNameBytes.Length];
        bytes[0] = 0x50;
        bytes[1] = 0x4B;
        bytes[2] = 0x01;
        bytes[3] = 0x02;

        Buffer.BlockCopy(fixedHeaderAfterSignature, 0, bytes, 4, fixedHeaderAfterSignature.Length);
        Buffer.BlockCopy(fileNameBytes, 0, bytes, 46, fileNameBytes.Length);
        return bytes;
    }

    /// <summary>
    /// Builds a minimal byte sequence that looks like a ZIP file with central directory
    /// entries for "[Content_Types].xml" and "word/document.xml".
    /// Starts with the local file header signature so ARCH_ZIP's bytes rule matches at offset 0.
    /// </summary>
    private static byte[] BuildMinimalDocxZip()
    {
        using MemoryStream ms = new MemoryStream();

        // Local file header signature at offset 0 (so ARCH_ZIP matches)
        ms.Write(new byte[] { 0x50, 0x4B, 0x03, 0x04 });
        // Pad with enough zero bytes so the central directory entries are still
        // within the 51200-byte search window
        ms.Write(new byte[26]);

        // Central directory entry for [Content_Types].xml  (so BASE_OPC matches)
        byte[] contentTypesEntry = BuildCentralDirectoryEntry("[Content_Types].xml");
        ms.Write(contentTypesEntry);

        // Central directory entry for word/document.xml  (so DOCS_MS_DOCX matches)
        byte[] wordEntry = BuildCentralDirectoryEntry("word/document.xml");
        ms.Write(wordEntry);

        return ms.ToArray();
    }

    private sealed class TestHarness : IDisposable
    {
        private readonly string tempDirectory;
        private readonly string configPath;
        private readonly SignatureClassifier classifier;

        public TestHarness(string configXml)
        {
            tempDirectory = Path.Combine(Path.GetTempPath(), "UfexFileTypeTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDirectory);
            configPath = Path.Combine(tempDirectory, "test-config.xml");
            File.WriteAllText(configPath, configXml);

            FileTypeDb fileTypeDb = new FileTypeDb([new FileInfo(configPath)]);
            classifier = new SignatureClassifier
            {
                FileTypes = fileTypeDb,
            };
        }

        public string[] Detect(byte[] bytes)
        {
            using FileContext ctx = CreateTempFile(bytes);
            return classifier.DetectFileType(ctx.Path, ctx.Stream);
        }

        public DetectionMatch[] DetectDetailed(byte[] bytes)
        {
            using FileContext ctx = CreateTempFile(bytes);
            return classifier.DetectFileTypeDetailed(ctx.Path, ctx.Stream);
        }

        public void Dispose()
        {
            if(Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, true);
            }
        }

        private FileContext CreateTempFile(byte[] bytes)
        {
            string filePath = Path.Combine(tempDirectory, $"sample-{Guid.NewGuid():N}.bin");
            File.WriteAllBytes(filePath, bytes);
            FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return new FileContext(filePath, stream);
        }

        private readonly struct FileContext : IDisposable
        {
            public string Path { get; }
            public FileStream Stream { get; }

            public FileContext(string path, FileStream stream)
            {
                Path = path;
                Stream = stream;
            }

            public void Dispose()
            {
                Stream.Dispose();
            }
        }
    }
}
