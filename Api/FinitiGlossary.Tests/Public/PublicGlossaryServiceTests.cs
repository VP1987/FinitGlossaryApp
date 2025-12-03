using FinitiGlossary.Application.DTOs.Term.Public;
using FinitiGlossary.Application.Interfaces.Repositories.Term.Public;
using FinitiGlossary.Application.Services.Term.Public;
using Moq;

namespace FinitiGlossary.Tests.Public
{
    public class PublicGlossaryServiceTests
    {
        private readonly Mock<IPublicGlossaryRepository> _repoMock;
        private readonly PublicGlossaryService _sut;

        public PublicGlossaryServiceTests()
        {
            _repoMock = new Mock<IPublicGlossaryRepository>();
            _sut = new PublicGlossaryService(_repoMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnRepositoryResult()
        {
            var query = new PublicTermQuery
            {
                Offset = 0,
                Limit = 10,
                Search = "abc",
                Sort = "dateDesc"
            };

            var expected = new PublicTermListResult(
                new PublicTermListMeta(
                    Offset: 0,
                    Limit: 10,
                    Total: 1,
                    HasMore: false,
                    Sort: "dateDesc",
                    Search: "abc"
                ),
                new List<GlossaryPublicTermDTO>
                {
                    new GlossaryPublicTermDTO(
                        Id: 1,
                        StableId: Guid.NewGuid(),
                        Term: "Test",
                        Definition: "TestDef",
                        Version: 1,
                        Status: 2,
                        CreatedAt: DateTime.UtcNow,
                        CreatedById: 1
                    )
                }
            );

            _repoMock
                .Setup(r => r.GetAllAsync(query))
                .ReturnsAsync(expected);

            var result = await _sut.GetAllAsync(query);

            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task GetTermByIdAsync_ShouldReturnTerm_WhenFound()
        {
            const int id = 10;

            var expected = new PublicGlossaryDetail(
                id,
                "ABC",
                "DEF",
                DateTime.UtcNow,
                "Tester"
            );

            _repoMock
                .Setup(r => r.GetTermByIdAsync(id))
                .ReturnsAsync(expected);

            var result = await _sut.GetTermByIdAsync(id);

            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task GetTermByIdAsync_ShouldThrow_WhenRepositoryFails()
        {
            const int id = 10;

            _repoMock
                .Setup(r => r.GetTermByIdAsync(id))
                .ThrowsAsync(new Exception("x"));

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.GetTermByIdAsync(id));

            Assert.Contains("Failed to find term", ex.Message);
        }
    }
}
