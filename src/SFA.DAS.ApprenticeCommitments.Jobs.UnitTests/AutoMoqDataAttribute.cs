using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.NUnit3;

namespace SFA.DAS.ApprenticeCommitments.Jobs.UnitTests
{
    public class AutoMoqDataAttribute : AutoDataAttribute
    {
        public AutoMoqDataAttribute()
            : base(() => new Fixture().Customize(new AutoMoqCustomization()))
        {
        }
    }
}