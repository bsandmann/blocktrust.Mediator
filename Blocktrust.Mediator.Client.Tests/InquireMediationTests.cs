namespace Blocktrust.Mediator.Client.RootsIntegrationTests;

using System.Net;
using Blocktrust.Mediator.Common.Commands.CreatePeerDid;
using Commands.MediatorCoordinator.InquireMediation;
using Common;
using DIDComm.Secrets;
using FluentAssertions;
using MediatR;
using Moq;
using Moq.Protected;
using Xunit;

public class InquireMediationTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private InquireMediationHandler _inquireMediationHandler;
    private readonly CreatePeerDidHandler _createPeerDidHandler;

    public InquireMediationTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        _createPeerDidHandler = new CreatePeerDidHandler();

        _mediatorMock.Setup(p => p.Send(It.IsAny<CreatePeerDidRequest>(), It.IsAny<CancellationToken>()))
            .Returns(async (CreatePeerDidRequest request, CancellationToken token) => await _createPeerDidHandler.Handle(request, token));
    }
   
    /// <summary>
    /// Mocking the happy path is complicated:
    /// Since the return message from the mediator is encrypted, we need to mock the initial peer did, or at least its secrets to also be unable to decrypt the message.
    /// So I just mock a un-happy path for now and the real tests are left to the integration tests.
    /// </summary>
    
    [Fact]
    public async Task InitiateMediateRequests_cannot_be_unpacked_with_valid_privateKeys_for_initially_created_peerDID()
    {
        // Arrange
        var oobInvitationRootsLocal =
            "eyJ0eXBlIjoiaHR0cHM6Ly9kaWRjb21tLm9yZy9vdXQtb2YtYmFuZC8yLjAvaW52aXRhdGlvbiIsImlkIjoiMmI5ZjFiZDMtMGQxZC00ODAzLThkZTctNTBhMjM5OTZkOGM2IiwiZnJvbSI6ImRpZDpwZWVyOjIuRXo2TFNyNTU3a25wdHJmcUVQNnFGeXd3N2hnQlU3aDhwV1NQVnFrQjUzV2h5ZXV2di5WejZNa3dBSmNlZUJBMnVWRU1LTGoycFZUdUpVeDQzelpnQlF1b2hkV1k5WThiNDh6LlNleUpwWkNJNkltNWxkeTFwWkNJc0luUWlPaUprYlNJc0luTWlPaUpvZEhSd09pOHZNVEkzTGpBdU1DNHhPamd3TURBaUxDSmhJanBiSW1ScFpHTnZiVzB2ZGpJaVhYMCIsImJvZHkiOnsiZ29hbF9jb2RlIjoicmVxdWVzdC1tZWRpYXRlIiwiZ29hbCI6IlJlcXVlc3RNZWRpYXRlIiwibGFiZWwiOiJNZWRpYXRvciIsImFjY2VwdCI6WyJkaWRjb21tL3YyIl19fQ";
        var request = new InquireMediationRequest(oobInvitationRootsLocal);
        var mediatorResponseContent =
            """{"protected":"eyJ0eXAiOiJhcHBsaWNhdGlvbi9kaWRjb21tLWVuY3J5cHRlZCtqc29uIiwiYWxnIjoiRUNESC0xUFUrQTI1NktXIiwiZW5jIjoiQTI1NkNCQy1IUzUxMiIsImFwdSI6IlpHbGtPbkJsWlhJNk1pNUZlalpNVTNKbFkxTkZabkZZVUdwdlkxWjJRMk5CZW5Ca2EycDBiMUkwTlhsYVZreHJTelJEVUVOWFJtOU1kbTlaTGxaNk5rMXJaamx6VWxGaE9EZEllR1JrVVZwYVEyTlVURzQzYURoQmRESlZWbGRVZGpKUldYSlJlR1YwWTNKeVUzSXVVMlY1U25CYVEwazJTVzAxYkdSNU1YQmFRMGx6U1c1UmFVOXBTbXRpVTBselNXNU5hVTlwU205a1NGSjNUMms0ZGsxVVNUTk1ha0YxVFVNMGVFOXFaM2ROUkVGcFRFTkthRWxxY0dKSmJWSndXa2RPZG1KWE1IWmtha2xwV0Znd0l6Wk1VM0psWTFORlpuRllVR3B2WTFaMlEyTkJlbkJrYTJwMGIxSTBOWGxhVmt4clN6UkRVRU5YUm05TWRtOVoiLCJhcHYiOiJGT001dmNpd1BUNnFPNzg5dTVyUW9QMGp6Tms0QjRWUWl2dm1JamFzRHhZIiwic2tpZCI6ImRpZDpwZWVyOjIuRXo2TFNyZWNTRWZxWFBqb2NWdkNjQXpwZGtqdG9SNDV5WlZMa0s0Q1BDV0ZvTHZvWS5WejZNa2Y5c1JRYTg3SHhkZFFaWkNjVExuN2g4QXQyVVZXVHYyUVlyUXhldGNyclNyLlNleUpwWkNJNkltNWxkeTFwWkNJc0luUWlPaUprYlNJc0luTWlPaUpvZEhSd09pOHZNVEkzTGpBdU1DNHhPamd3TURBaUxDSmhJanBiSW1ScFpHTnZiVzB2ZGpJaVhYMCM2TFNyZWNTRWZxWFBqb2NWdkNjQXpwZGtqdG9SNDV5WlZMa0s0Q1BDV0ZvTHZvWSIsImVwayI6eyJjcnYiOiJYMjU1MTkiLCJ4IjoielZxLW1RNFkyLVBnT2s4dktxd0owVWFCN2RYUUZ5UnRfY2wzbWFmYXF5NCIsImt0eSI6Ik9LUCJ9fQ","recipients":[{"header":{"kid":"did:peer:2.Ez6LSoYyDq4trgzFfsG5i6zkuRzruZkLW4KSLmhKisqF6eGRJ.Vz6MkvXxQL9tQLuGcJwyrHyx42ApCepXUF5t4qADjbsUAcg1U#6LSoYyDq4trgzFfsG5i6zkuRzruZkLW4KSLmhKisqF6eGRJ"},"encrypted_key":"PCIpNfZmDi5F9Qq02-5iVMsS78dDEjJaxWHlXkQLkwSwEcXm2lBNd8mhAP5QXJ1CMslghrMFuOQEMF67s1oI62H9ItGfx_YI"}],"iv":"J9KYYPp4aGrKSTtyjNL4Lg","ciphertext":"jqoMgAAftTet0cScDQM9GlffbCdKT3--2WxqIdIwJyBjtyr1Aasp71wwACfAShdILjFdAilLz80IMdT3IH9y2mR4fwekamLSzvMYKXONafYe0lWr9JalnecXjga7IexBlQIDwXR7ruMeBrjg5GOm6ER24DD65FDe_VKdMMqnxcjaHmzUzVia2porFrF9ZfzJQq4auwFJG5ae3wPw_novzpzN2sHS3DG5M0oI_Ip78PwM-gLPLgg54gcwSJBDgYmyP25Lt4jNqdJRnmnJp_OzhIMOYJEmvy7xnBMWXXs27fAhbxCrfedEDDIhdyiDTbYWBmanTU3jtywTyM1K7Ft69xHjJajCNYaUsRPepuGRVFcCEcK4SheDCUXOXY2I4QB4btWwxgKWwr44pqViHvM0iH8rm282HtgCfZ5zxuCCRf3WArlTQPp2jEcuVaCI5PzKWQ4K7Z64-WoV5_ikXNcs6J-S24XraDIbTK04dBwiUoLPBYnEP4xP5CrtNNBB-UxkfSZGXgqgpspTI2tYZozAba22tlLoeCZ_p7Elo04ZSBNli1EkNTchvFDcc9jeqXiHUpgVMW2xIJ_hTYIvA8cut6ce49B0SqLKhasLnAmfRYrA76HUVUw2hzyLRzANlBh_SO-JRSNIiQzET6dBaF_fopB2onTZjCZALgJq4EAPCns3NEI48RZY4DzVYpWa845bVYSzgWHkb-lQkyEO-w8obPA-_4GqGDJ0_geRdFixcLS7PWL_HsSfw4iyMTdjQSm6lTQb7TxkorXOywXE0eY8kw0cw97I0qPXQfd1QxwSO__dsGw89MMppqXdJaM5XQs2X4xy_nG1dn_ZtuMhR1DvsUT9WbaiE4wJ7K9UPhKilD53U1FiJv7FS_F-Jd1lOgNLLg5Jyjjl0jvogiS3RVd-ax-nl9dDByEXymnHskhrUIh3N9x0GnGx0NszNUUZPfKYVaAr4qtuYBMYXz8gYNbhaywyVid6hhw01I1E6CJJUQky5Y2ch_QMdPNXGN41_S2YoS9yg_GAKAvo26qrDebwWuVnEdQ32qTcxZ9m38BDt5AV2ZxvqAEcJWH_gr2JxXuvSMI2WexJNo8v3dqeJKkSW3Cs1sDrT07Lh7wJM918S_99IhggsNJFH9L8Yw1X2DbXRtP5NnBB1Il6GrLfzL87KHmVX-umwMldJN_u1NPDFe3kWfbtpi4IFKQE2efrwzzopzg94UgRmgq5y6El2QZ8AU9TlE02YpPmBgBciR48trd2oKJ_gSG0fHxOU5d0sCYXFCHzsdGOa_T9HYJEo1jTsSC_6385L0WFW4hytTKEjN_UPfihhYaofVVFF_et57wMXxecaj3P7-GnKCZZGomo4SMrWJiGD7IBfvtjbom6miUN8h8KtIKekI2ylpuTWHIv6uh5pt2ZpHtGfflOw0mX_yoTMvBuKffyhHeNBm6NTSsOYhrmGNBMLm6_K2IMxW8tbwjClBqtUpXsP9vp7kH_DvoJqulv7Dacgrprvy7eVJGXGqiGPZ__VNYsJ-oGmWN8eRkJXAJlxjQa3EI514ZIt1vOlWQ1tlJ6iBQUMJinoysjtOROk0lET8WRkcn5NAnu7WtVr6yNlOueYkt2h4f_u0DV9exzqWsUsYfTAmpSxLLitsH9xk7xeSS6MvY_A9aLS-o_-xl3cAvQZOmwMxEdzj1rT3zKuCvA2qjvmZLt0A8_Jrr1qBdGCOBvLDEDpK5kwOVkXYraKXKDOJV8d-GzqiXjDhcZJGMzUeeGEr_PhMhdI5gszxFnn0XZwxzjNOQTz90H9o51CoKXSbE39l9MDn1l4G-UxykYrf12fQgG2OvY7okMTVhjdSNb1m_SZCS3TbCIMu_KBXjCCatOuIYe5_lYKC9CCyYM7smc1j7MbrecqnwmpUa9cOb2BQpG_BbJ-n2LuporgWh9m4WZhXXApg","tag":"TBWjh75NvSiFHitMGMQGQRbwC1Oo079NxbViK0vSwQc"}""";
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(mediatorResponseContent),
            })
            .Verifiable();
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://notimportant:1234")
        };

        // Act
        _inquireMediationHandler = new InquireMediationHandler(_mediatorMock.Object, httpClient, new SimpleDidDocResolver(), new SecretResolverInMemory());
        var result = await _inquireMediationHandler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.First().Message.Should().Contain("Message cannot be parsed: The Secret 'did:peer:2.Ez6LSoYyDq4trgzFfsG5i6zkuRzruZkLW4KSLmhKisqF6eGRJ.Vz6MkvXxQL9tQLuGcJwyrHyx42ApCepXUF5t4qADjbsUAcg1U#6LSoYyDq4trgzFfsG5i6zkuRzruZkLW4KSLmhKisqF6eGRJ' not found");//not found
    }
}