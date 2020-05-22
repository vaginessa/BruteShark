﻿using System;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PcapAnalyzerTest
{
    [TestClass]
    public class AnalyzerTest
    {
        [TestMethod]
        public void Analyzer_LoadModules_LoadSuccess()
        {
            // Arrange.
            var analyzer = new PcapAnalyzer.Analyzer();

            // Act.
            var modulesList = analyzer.AvailableModulesNames;

            // Assert.
            Assert.AreEqual(3, modulesList.Count);
        }

        [TestMethod]
        public void Analyzer_AddModule_Adduccess()
        {
            // Arrange.
            var analyzer = new PcapAnalyzer.Analyzer();

            // Act (Add one module).
            analyzer.AddModule(analyzer.AvailableModulesNames.First());

            // Assert.
            Assert.AreEqual(1, analyzer.LoadedModulesNames.Count);
        }

        [TestMethod]
        public void Analyzer_RemoveModule_LoadSuccess()
        {
            // Arrange.
            var analyzer = new PcapAnalyzer.Analyzer();

            // Act (Add two modulem, remove one).
            analyzer.AddModule(analyzer.AvailableModulesNames[0]);
            analyzer.AddModule(analyzer.AvailableModulesNames[1]);
            analyzer.RemoveModule(analyzer.LoadedModulesNames[0]);

            // Assert.
            Assert.AreEqual(1, analyzer.LoadedModulesNames.Count);
        }

        [TestMethod]
        public void FtpPasswordParser_ParseFtpPassword_ParseSuccess()
        {
            // Arrange.
            var ftpParsrer = new PcapAnalyzer.FtpPasswordParser();
            var session = new PcapAnalyzer.TcpSession();
            session.SourceIp = "1.1.1.1";
            session.DestinationIp = "2.2.2.2";
            session.Data = Encoding.UTF8.GetBytes(
@"220 Chris Sanders FTP Server
USER csanders
331 Password required for csanders.
PASS echo
230 User csanders logged in.");

            // Act.
            PcapAnalyzer.NetworkPassword passsword = (ftpParsrer.Parse(session) as PcapAnalyzer.NetworkPassword);

            // Assert.
            Assert.AreEqual("csanders", passsword.Username);
            Assert.AreEqual("echo", passsword.Password);
        }

        [TestMethod]
        public void HttpBasicPasswordParser_ParseFtpPassword_ParseSuccess()
        {
            // Arrange.
            var parsrer = new PcapAnalyzer.HttpBasicPasswordParser();
            var packet = new PcapAnalyzer.TcpPacket();
            packet.SourceIp = "1.1.1.1";
            packet.DestinationIp = "2.2.2.2";
            packet.Data = Encoding.UTF8.GetBytes(
@"GET /password-ok.php HTTP/1.1
Host: browserspy.dk
Connection: keep-alive
Cache-Control: max-age=0
Authorization: Basic dGVzdDpmYWlsMw==
Accept: text/html,application/xhtml+xml");

            // Act.
            PcapAnalyzer.NetworkPassword password = (parsrer.Parse(packet) as PcapAnalyzer.NetworkPassword);

            // Assert.
            Assert.AreEqual("test", password.Username);
            Assert.AreEqual("fail3", password.Password);
        }

        [TestMethod]
        public void TelnetPasswordParser_ParseTelnetCharModePassword_ParseSuccess()
        {
            // Arrange
            var telnetParser = new PcapAnalyzer.TelnetPasswordParser();
            var session = new PcapAnalyzer.TcpSession();

            var expected = new PcapAnalyzer.NetworkPassword()
            {
                Username = "us",
                Password = "Secret123",
                Destination = "2.2.2.2",
                Source = "1.1.1.1",
                Protocol = "Telnet"
            };

            // Mock a session where the user is "us" and the password is "Secret123".
            session.Data = Encoding.ASCII.GetBytes("login:uuss" + Environment.NewLine + "Password:Secret123");
            session.Packets.Add(mockPacket("2.2.2.2", "1.1.1.1", 21, 5472, "login:"));
            session.Packets.Add(mockPacket("1.1.1.1", "2.2.2.2", 5472, 21, "u"));
            session.Packets.Add(mockPacket("2.2.2.2", "1.1.1.1", 21, 5472, "u"));
            session.Packets.Add(mockPacket("1.1.1.1", "2.2.2.2", 5472, 21, "s"));
            session.Packets.Add(mockPacket("2.2.2.2", "1.1.1.1", 21, 5472, "s"));
            session.Packets.Add(mockPacket("2.2.2.2", "1.1.1.1", 21, 5472, "Password:"));
            session.Packets.Add(mockPacket("1.1.1.1", "2.2.2.2", 5472, 21, "Secret123"));
            session.Packets.Add(mockPacket("2.2.2.2", "1.1.1.1", 21, 5472, "some dummy data"));

            // Act.
            PcapAnalyzer.NetworkPassword password = (telnetParser.Parse(session) as PcapAnalyzer.NetworkPassword);

            // Assert.
            Assert.AreEqual(expected, password);

        }

        [TestMethod]
        public void NtlmPasswordParser_ParseSmbNTLMv2Session_ParseSuccess()
        {
            // Arrange
            var ntlmParser = new PcapAnalyzer.NtlmsspHashParser();
            var session = new PcapAnalyzer.TcpSession();

            var serverPacket = new PcapAnalyzer.TcpPacket()
            {
                SourceIp = "2.2.2.2",
                DestinationIp = "1.1.1.1",
                Data = new byte[]
                {
                    0x00, 0x00, 0x00, 0xf8, 0xfe, 0x53, 0x4d, 0x42, 0x40, 0x00, 0x00, 0x00, 0x16, 0x00, 0x00, 0xc0,
                    0x01, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0xd5, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3d, 0x00, 0x00, 0x94,
                    0x00, 0x48, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x09, 0x00, 0x00, 0x00, 0x48, 0x00, 0xb0, 0x00, 0x4e, 0x54, 0x4c, 0x4d,
                    0x53, 0x53, 0x50, 0x00, 0x02, 0x00, 0x00, 0x00, 0x08, 0x00, 0x08, 0x00, 0x38, 0x00, 0x00, 0x00,
                    0x35, 0x02, 0x89, 0xe2, 0x01, 0x15, 0x18, 0x13, 0xd2, 0x89, 0x8c, 0xcd, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x70, 0x00, 0x70, 0x00, 0x40, 0x00, 0x00, 0x00, 0x0a, 0x00, 0x39, 0x38,
                    0x00, 0x00, 0x00, 0x0f, 0x53, 0x00, 0x55, 0x00, 0x53, 0x00, 0x45, 0x00, 0x02, 0x00, 0x08, 0x00,
                    0x53, 0x00, 0x55, 0x00, 0x53, 0x00, 0x45, 0x00, 0x01, 0x00, 0x0c, 0x00, 0x57, 0x00, 0x53, 0x00,
                    0x32, 0x00, 0x30, 0x00, 0x31, 0x00, 0x36, 0x00, 0x04, 0x00, 0x0e, 0x00, 0x73, 0x00, 0x75, 0x00,
                    0x73, 0x00, 0x65, 0x00, 0x2e, 0x00, 0x64, 0x00, 0x65, 0x00, 0x03, 0x00, 0x1c, 0x00, 0x57, 0x00,
                    0x53, 0x00, 0x32, 0x00, 0x30, 0x00, 0x31, 0x00, 0x36, 0x00, 0x2e, 0x00, 0x73, 0x00, 0x75, 0x00,
                    0x73, 0x00, 0x65, 0x00, 0x2e, 0x00, 0x64, 0x00, 0x65, 0x00, 0x05, 0x00, 0x0e, 0x00, 0x73, 0x00,
                    0x75, 0x00, 0x73, 0x00, 0x65, 0x00, 0x2e, 0x00, 0x64, 0x00, 0x65, 0x00, 0x07, 0x00, 0x08, 0x00,
                    0x8a, 0x8c, 0xe7, 0xa9, 0xf4, 0xce, 0xd2, 0x01, 0x00, 0x00, 0x00, 0x00
                }
            };

            var clientPacket = new PcapAnalyzer.TcpPacket()
            {
                SourceIp = "1.1.1.1",
                DestinationIp = "2.2.2.2",
                Data = new byte[]
                {
                    0x00, 0x00, 0x01, 0x68, 0xfe, 0x53, 0x4d, 0x42, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x01, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0xd5, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3d, 0x00, 0x00, 0x94,
                    0x00, 0x48, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x19, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x58, 0x00, 0x10, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x4e, 0x54, 0x4c, 0x4d,
                    0x53, 0x53, 0x50, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00,
                    0x9c, 0x00, 0x9c, 0x00, 0x40, 0x00, 0x00, 0x00, 0x08, 0x00, 0x08, 0x00, 0xdc, 0x00, 0x00, 0x00,
                    0x1a, 0x00, 0x1a, 0x00, 0xe4, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xfe, 0x00, 0x00, 0x00,
                    0x10, 0x00, 0x10, 0x00, 0x00, 0x01, 0x00, 0x00, 0x35, 0x02, 0x88, 0xe0, 0x39, 0xdb, 0xdb, 0xeb,
                    0x1b, 0xdd, 0x29, 0xb0, 0x7a, 0x5d, 0x20, 0xc8, 0xf8, 0x2f, 0x2c, 0xb7, 0x01, 0x01, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x8a, 0x8c, 0xe7, 0xa9, 0xf4, 0xce, 0xd2, 0x01, 0xe7, 0x96, 0x9a, 0x04,
                    0x87, 0x2c, 0x16, 0x89, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x08, 0x00, 0x53, 0x00, 0x55, 0x00,
                    0x53, 0x00, 0x45, 0x00, 0x01, 0x00, 0x0c, 0x00, 0x57, 0x00, 0x53, 0x00, 0x32, 0x00, 0x30, 0x00,
                    0x31, 0x00, 0x36, 0x00, 0x04, 0x00, 0x0e, 0x00, 0x73, 0x00, 0x75, 0x00, 0x73, 0x00, 0x65, 0x00,
                    0x2e, 0x00, 0x64, 0x00, 0x65, 0x00, 0x03, 0x00, 0x1c, 0x00, 0x57, 0x00, 0x53, 0x00, 0x32, 0x00,
                    0x30, 0x00, 0x31, 0x00, 0x36, 0x00, 0x2e, 0x00, 0x73, 0x00, 0x75, 0x00, 0x73, 0x00, 0x65, 0x00,
                    0x2e, 0x00, 0x64, 0x00, 0x65, 0x00, 0x05, 0x00, 0x0e, 0x00, 0x73, 0x00, 0x75, 0x00, 0x73, 0x00,
                    0x65, 0x00, 0x2e, 0x00, 0x64, 0x00, 0x65, 0x00, 0x07, 0x00, 0x08, 0x00, 0x8a, 0x8c, 0xe7, 0xa9,
                    0xf4, 0xce, 0xd2, 0x01, 0x00, 0x00, 0x00, 0x00, 0x53, 0x00, 0x55, 0x00, 0x53, 0x00, 0x45, 0x00,
                    0x61, 0x00, 0x64, 0x00, 0x6d, 0x00, 0x69, 0x00, 0x6e, 0x00, 0x69, 0x00, 0x73, 0x00, 0x74, 0x00,
                    0x72, 0x00, 0x61, 0x00, 0x74, 0x00, 0x6f, 0x00, 0x72, 0x00, 0x00, 0x00, 0xb2, 0xe8, 0x76, 0x55,
                    0x9c, 0x9c, 0x58, 0xb0, 0x34, 0x4b, 0xd5, 0xa9, 0x9f, 0x8e, 0x98, 0x55
                }
            };

            session.Packets.Add(serverPacket);
            session.Packets.Add(clientPacket);

            // Act.
            var hash = ntlmParser.Parse(session) as PcapAnalyzer.NtlmHash;

            // Assert.
            Assert.AreEqual("NTLMSSP", hash.Protocol);
            Assert.AreEqual("administrator", hash.User);
            Assert.AreEqual("SUSE", hash.Domain);
            Assert.AreEqual(hash.NtHash.Length, 312);
        }

        [TestMethod]
        public void KerberosHshParser_ParseUdpSession_ParseSuccess()
        {
            // Arrange
            var kerberosParser = new PcapAnalyzer.KerberosHashParser();

            var kerberosAsRequestPacket = new PcapAnalyzer.UdpPacket
            {
                SourceIp = "2.2.2.2",
                DestinationIp = "1.1.1.1",
                Data = new byte[]
                {
                    0x6a, 0x82, 0x01, 0x1f, 0x30, 0x82, 0x01, 0x1b, 0xa1, 0x03, 0x02, 0x01, 0x05, 0xa2, 0x03, 0x02,
                    0x01, 0x0a, 0xa3, 0x5f, 0x30, 0x5d, 0x30, 0x48, 0xa1, 0x03, 0x02, 0x01, 0x02, 0xa2, 0x41, 0x04,
                    0x3f, 0x30, 0x3d, 0xa0, 0x03, 0x02, 0x01, 0x17, 0xa2, 0x36, 0x04, 0x34, 0x09, 0xa2, 0x24, 0x48,
                    0x93, 0xaf, 0xf5, 0xf3, 0x84, 0xf7, 0x9c, 0x37, 0x88, 0x3f, 0x15, 0x4a, 0x32, 0xd3, 0x96, 0xa9,
                    0x14, 0xa4, 0xd0, 0xa7, 0x8e, 0x97, 0x9b, 0xa7, 0x5d, 0x4f, 0xf5, 0x3c, 0x1d, 0xb7, 0x29, 0x41,
                    0x41, 0x76, 0x0f, 0xee, 0x05, 0xe4, 0x34, 0xc1, 0x2e, 0xcf, 0x8d, 0x5b, 0x9a, 0xa5, 0x83, 0x9e,
                    0x30, 0x11, 0xa1, 0x04, 0x02, 0x02, 0x00, 0x80, 0xa2, 0x09, 0x04, 0x07, 0x30, 0x05, 0xa0, 0x03,
                    0x01, 0x01, 0xff, 0xa4, 0x81, 0xad, 0x30, 0x81, 0xaa, 0xa0, 0x07, 0x03, 0x05, 0x00, 0x40, 0x81,
                    0x00, 0x10, 0xa1, 0x10, 0x30, 0x0e, 0xa0, 0x03, 0x02, 0x01, 0x01, 0xa1, 0x07, 0x30, 0x05, 0x1b,
                    0x03, 0x64, 0x65, 0x73, 0xa2, 0x08, 0x1b, 0x06, 0x44, 0x45, 0x4e, 0x59, 0x44, 0x43, 0xa3, 0x1b,
                    0x30, 0x19, 0xa0, 0x03, 0x02, 0x01, 0x02, 0xa1, 0x12, 0x30, 0x10, 0x1b, 0x06, 0x6b, 0x72, 0x62,
                    0x74, 0x67, 0x74, 0x1b, 0x06, 0x44, 0x45, 0x4e, 0x59, 0x44, 0x43, 0xa5, 0x11, 0x18, 0x0f, 0x32,
                    0x30, 0x33, 0x37, 0x30, 0x39, 0x31, 0x33, 0x30, 0x32, 0x34, 0x38, 0x30, 0x35, 0x5a, 0xa6, 0x11,
                    0x18, 0x0f, 0x32, 0x30, 0x33, 0x37, 0x30, 0x39, 0x31, 0x33, 0x30, 0x32, 0x34, 0x38, 0x30, 0x35,
                    0x5a, 0xa7, 0x06, 0x02, 0x04, 0x0b, 0xc4, 0xdd, 0x7e, 0xa8, 0x19, 0x30, 0x17, 0x02, 0x01, 0x17,
                    0x02, 0x02, 0xff, 0x7b, 0x02, 0x01, 0x80, 0x02, 0x01, 0x03, 0x02, 0x01, 0x01, 0x02, 0x01, 0x18,
                    0x02, 0x02, 0xff, 0x79, 0xa9, 0x1d, 0x30, 0x1b, 0x30, 0x19, 0xa0, 0x03, 0x02, 0x01, 0x14, 0xa1,
                    0x12, 0x04, 0x10, 0x58, 0x50, 0x31, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
                    0x20, 0x20, 0x20
                }
            };

            // Act.
            var hash = kerberosParser.Parse(kerberosAsRequestPacket) as PcapAnalyzer.KerberosHash;

            // Assert.
            Assert.AreEqual("Kerberos V5", hash.HashType);
            Assert.AreEqual("des", hash.User);
            Assert.AreEqual("DENYDC", hash.Domain);
            Assert.AreEqual(hash.Hash, "32d396a914a4d0a78e979ba75d4ff53c1db7294141760fee05e434c12ecf8d5b9aa5839e09a2244893aff5f384f79c37883f154a");
        }

        [TestMethod]
        public void Utilities_GetDataBetweenHeaderAndFooter_ParseSuccess()
        {
            // Arrange
            var data = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a };
            var header = new byte[] { 0x02, 0x03 };
            var footer = new byte[] { 0x06, 0x07 };
            var expected = new byte[] { 0x02, 0x03, 0x04, 0x05, 0x06, 0x07 };

            // Act
            var res = PcapAnalyzer.Utilities.GetDataBetweenHeaderAndFooter(data, header, footer);

            // Assert
            Assert.IsTrue(res.SequenceEqual(expected));
        }


        private PcapAnalyzer.TcpPacket mockPacket(string sourceIp, string destinationIp, int sourcePort, int destinationPort, string data)
        {
            return new PcapAnalyzer.TcpPacket()
            {
                SourceIp = sourceIp,
                DestinationIp = destinationIp,
                SourcePort = sourcePort,
                DestinationPort = destinationPort,
                Data = Encoding.ASCII.GetBytes(data)
            };
        }

    }
}
