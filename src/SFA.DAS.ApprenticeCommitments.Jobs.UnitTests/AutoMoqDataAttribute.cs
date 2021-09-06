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

        private static IFixture CreateFixture()
        {
            var fixture = new Fixture();
            fixture.Customize(new AutoMoqCustomization { ConfigureMembers = true });
            fixture.Customize<UrlConfiguration>(x => x
                .With(p => p.StartPageUrl, () => fixture.Create<Uri>().ToString()));
            return fixture;
        }
    }
}