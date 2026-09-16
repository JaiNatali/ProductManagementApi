using System.Linq.Expressions;
using Application.Interfaces;
using Moq;

namespace Application.Tests.Services;

internal static class MockExtensions
{
    public static void SetupRepository<T>(this Mock<IRepository<T>> repository, Func<Expression<Func<T, bool>>, Task<T?>> resultFactory) where T : class
    {
        repository.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<T, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Expression<Func<T, bool>> predicate, CancellationToken _) => resultFactory(predicate).Result);
    }
}
