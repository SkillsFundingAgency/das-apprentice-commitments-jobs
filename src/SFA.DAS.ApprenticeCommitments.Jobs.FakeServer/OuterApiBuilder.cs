using AutoFixture;
using System.Net;
using WireMock.Logging;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Settings;

namespace SFA.DAS.ApprenticeCommitments.Jobs.FakeServer;

public class OuterApiBuilder
{
    private readonly WireMockServer _server;
    private readonly Fixture _fixture;

    public OuterApiBuilder(int port)
    {
        _server = WireMockServer.Start(new WireMockServerSettings
        {
            Port = port,
            UseSSL = true,
            StartAdminInterface = true,
            Logger = new WireMockConsoleLogger(),
        });

        _fixture = new Fixture();
    }

    public static OuterApiBuilder Create(int port)
    {
        return new OuterApiBuilder(port);
    }

    public OuterApi Build()
    {
        return new OuterApi(_server);
    }

    public OuterApiBuilder WithUpdateApproval()
    {
        // handle 1 SendApprenticeshipInvitationCommand
        var reg = _fixture.Build<Api.Registration>()
                    .With(x => x.ApprenticeId, Guid.NewGuid()).Create();

        // https://localhost:5121/approvals/1/registration
        _server.Given(
                Request.Create()
                    .WithPath("/approvals/*/registration")
                    .UsingGet()
                     )
            .RespondWith(
                Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithBodyAsJson(reg));

        // handle 4 ApprenticeshipCreatedEvent
        _server.Given(
                Request.Create()
                    .WithPath("/approvals")
                    .UsingPost())
            .RespondWith(
                Response.Create()
                    .WithStatusCode(HttpStatusCode.OK));

        // handle 5 ApprenticeshipUpdatedApprovedEvent
        _server.Given(
                Request.Create()
                    .WithPath("/approvals")
                    .UsingPut())
            .RespondWith(
                Response.Create()
                    .WithStatusCode(HttpStatusCode.OK));

        // handle 6 ApprenticeshipStoppedEvent which calls ProcessStoppedApprenticeship after timeout
        _server.Given(
            Request.Create()
                .WithPath("/approvals/stopped")
                .UsingPost())
        .RespondWith(
            Response.Create()
                .WithStatusCode(HttpStatusCode.OK));

        // handle 7 ApprenticeEmailAddressChanged
        var apr = _fixture.Create<Api.Apprenticeship>();
        var aprw = new Api.ApprenticeshipsWrapper { Apprenticeships = new List<Api.Apprenticeship> { apr }};

        // https://localhost:5121/apprentices/1/apprenticeships
        _server.Given(
                Request.Create()
                    .WithPath("/apprentices/*/apprenticeships")
                    .WithPath(x => x.Contains("apprenticeships"))
                    .UsingGet())
            .RespondWith(
                Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithBodyAsJson(aprw));

        // handle 8 ApprenticeshipChangedEvent

        var app = _fixture.Build<Api.Apprentice>().Create();

        // https://localhost:5121/apprentices/72fee270-2db5-4383-86b4-1c81ba9535aa
        _server.Given(
                Request.Create()
                    .WithPath("/apprentices/*")
                    .WithPath(x => !x.Contains("apprenticeships"))
                    .UsingGet())
            .RespondWith(
                Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithBodyAsJson(app));

        var revs = new List<Api.ApprenticeshipRevision>() { 
                    _fixture.Create<Api.ApprenticeshipRevision>(), _fixture.Create<Api.ApprenticeshipRevision>() };
        var hist = _fixture.Build<Api.ApprenticeshipHistory>().With(x => x.Revisions, revs).Create();

        // https://localhost:5121/apprentices/72fee270-2db5-4383-86b4-1c81ba9535aa/apprenticeships/1/revisions
        _server.Given(
                Request.Create()
                    .WithPath("/apprentices/*/apprenticeships/*/revisions")
                    .WithPath(x => x.Contains("revisions"))
                    .UsingGet())
            .RespondWith(
                Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithBodyAsJson(hist));

        // 9 uses the apprentice from 8

        // 10 handle ApprenticeshipRegisteredEvent
        _server.Given(
                Request.Create()
                    .WithPath("/registrations/*")
                    .UsingGet()
                     )
            .RespondWith(
                Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithBodyAsJson(reg));

        return this;
    }
}