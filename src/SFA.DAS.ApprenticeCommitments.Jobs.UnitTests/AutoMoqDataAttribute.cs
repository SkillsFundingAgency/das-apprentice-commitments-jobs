using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.NUnit3;
using SFA.DAS.ApprenticeCommitments.Jobs.Functions.Infrastructure;
using System;

namespace SFA.DAS.ApprenticeCommitments.Jobs.UnitTests
{
    public class AutoMoqDataAttribute : AutoDataAttribute
    {
        public AutoMoqDataAttribute()
            : base(() => CreateFixture())
        {
        }

        public static IFixture CreateFixture()
        {
            var fixture = new Fixture();
            fixture.Customize(new AutoMoqCustomization { ConfigureMembers = true });
            fixture.Customize<LoginServiceOptions>(x => x
                .With(p => p.CallbackUrl, () => fixture.Create<Uri>().ToString())
                .With(p => p.RedirectUrl, () => fixture.Create<Uri>().ToString()));
            return fixture;
        }
    }
}