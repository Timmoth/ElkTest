using Xunit;

namespace ElkTest.Test;

public class ElkTestBase : IClassFixture<ElkTestFixture>
{
    protected readonly ElkTestFixture _fixture;

    public ElkTestBase(ElkTestFixture fixture)
    {
        _fixture = fixture;
    }
}