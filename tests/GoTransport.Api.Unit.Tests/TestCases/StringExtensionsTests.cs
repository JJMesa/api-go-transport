using GoTransport.Application.Extensions;

namespace GoTransport.Api.Unit.Tests.TestCases;

public class StringExtensionsTests
{
    [Theory]
    [InlineData("ABC", "%ABC%")]
    [InlineData("50%", "%50[%]%")]
    [InlineData("A_1", "%A[_]1%")]
    [InlineData("[x]", "%[[]x]%")]
    public void ToContainsPattern_ShouldEscapeLikeWildcards(string term, string expected)
    {
        Assert.Equal(expected, term.ToContainsPattern());
    }
}
