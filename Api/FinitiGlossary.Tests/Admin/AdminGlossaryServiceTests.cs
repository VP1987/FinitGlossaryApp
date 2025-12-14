using FinitiGlossary.Application.DTOs.Request;
using FinitiGlossary.Application.DTOs.Term.Admin;
using FinitiGlossary.Application.Interfaces.Agregator;
using FinitiGlossary.Application.Interfaces.Repositories.Term.Admin;
using FinitiGlossary.Application.Interfaces.Repositories.UserIRepo;
using FinitiGlossary.Application.Services.Term.Admin;
using FinitiGlossary.Domain.Entities.Terms;
using FinitiGlossary.Domain.Entities.Terms.Status;
using FinitiGlossary.Domain.Entities.Users;
using Moq;
using System.Security.Claims;

namespace FinitiGlossary.Tests.Admin
{
    public class AdminGlossaryServiceTests
    {
        private readonly Mock<IAdminGlossaryRepository> _repoMock;
        private readonly Mock<IGlossaryAdminViewAggregator> _viewAgregatorMock;
        private readonly Mock<IGlossaryAdminViewHistoryAggregator> _viewHistoryAggregatorMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly AdminGlossaryService _sut;

        private const int TestUserId = 123;

        private readonly ClaimsPrincipal _testUser = new ClaimsPrincipal(
            new ClaimsIdentity(new[]
            {
                new Claim("id", TestUserId.ToString()),
                new Claim(ClaimTypes.Role, "Admin")
            }, "mock"));

        private readonly User _dbUser = new User { Id = TestUserId, Username = "Test Admin" };

        public AdminGlossaryServiceTests()
        {
            _repoMock = new Mock<IAdminGlossaryRepository>();
            _userRepoMock = new Mock<IUserRepository>();
            _viewAgregatorMock = new Mock<IGlossaryAdminViewAggregator>();
            _viewHistoryAggregatorMock = new Mock<IGlossaryAdminViewHistoryAggregator>();

            _sut = new AdminGlossaryService(
                _repoMock.Object,
                _viewAgregatorMock.Object,
                _viewHistoryAggregatorMock.Object,
                _userRepoMock.Object);

            _userRepoMock.Setup(r => r.GetUserByIdAsync(TestUserId)).ReturnsAsync(_dbUser);
        }

        [Fact]
        public async Task GetListAsync_FilterSearch()
        {
            var query = new AdminTermQuery { Offset = 0, Limit = 10, Search = "test" };

            var rows = new List<AdminTermRow>
            {
                new() { Term = "Test term", Definition = "x", Status = 1 }
            };

            _repoMock.Setup(r => r.GetAdminTermsPageAsync(
                    TestUserId,
                    "Admin",
                    query.Tab,
                    query.Search,
                    query.Sort,
                    query.Offset,
                    query.Limit))
                .ReturnsAsync((rows, 1));

            var result = await _sut.GetAdminTermListAsync(_testUser, query);

            Assert.Single(result.Data);
            Assert.Equal("Test term", result.Data.First().Term);
        }

        [Fact]
        public async Task GetListAsync_SortAZ()
        {
            var query = new AdminTermQuery { Offset = 0, Limit = 10, Sort = "az" };

            var rows = new List<AdminTermRow>
            {
                new() { Term = "A" },
                new() { Term = "B" }
            };

            _repoMock.Setup(r => r.GetAdminTermsPageAsync(
                    TestUserId,
                    "Admin",
                    query.Tab,
                    query.Search,
                    query.Sort,
                    query.Offset,
                    query.Limit))
                .ReturnsAsync((rows, 2));

            var result = await _sut.GetAdminTermListAsync(_testUser, query);

            Assert.Equal("A", result.Data.First().Term);
        }

        [Fact]
        public async Task GetListAsync_NoResults()
        {
            var query = new AdminTermQuery { Offset = 0, Limit = 10, Search = "xyz" };

            _repoMock.Setup(r => r.GetAdminTermsPageAsync(
                    TestUserId,
                    "Admin",
                    query.Tab,
                    query.Search,
                    query.Sort,
                    query.Offset,
                    query.Limit))
                .ReturnsAsync((new List<AdminTermRow>(), 0));

            var result = await _sut.GetAdminTermListAsync(_testUser, query);

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task CreateAsync_SuccessPath()
        {
            var request = new CreateGlossaryRequest("Test Term", "Test Definition");
            _repoMock.Setup(r => r.CreateAsync(It.IsAny<GlossaryTerm>())).ReturnsAsync(true);

            var result = await _sut.CreateTermAsync(request, _testUser);

            Assert.Equal("Draft created successfully.", result.Message);
        }

        [Fact]
        public async Task UpdateAsync_NotFound()
        {
            _repoMock.Setup(r => r.GetActiveByIdAsync(10)).ReturnsAsync((GlossaryTerm?)null);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.UpdateTermAsync(10, new UpdateGlossaryRequest("A", "B", TermStatus.Published), _testUser));
        }

        [Fact]
        public async Task ArchiveAsync_Success()
        {
            var termId = 10;
            var stableId = Guid.NewGuid();

            var term = new GlossaryTerm { Id = termId, StableId = stableId, CreatedById = TestUserId };

            _repoMock.Setup(r => r.GetActiveByIdAsync(termId)).ReturnsAsync(term);
            _repoMock.Setup(r => r.GetLatestVersionAsync(stableId)).ReturnsAsync(1);
            _repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

            var result = await _sut.ArchiveTermAsync(termId, _testUser);

            Assert.Equal("Term archived successfully.", result.Message);
        }

        [Fact]
        public async Task PublishAsync_NotFound()
        {
            _repoMock.Setup(r => r.GetActiveByIdAsync(10)).ReturnsAsync((GlossaryTerm?)null);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.PublishTermAsync(10, _testUser));
        }

        [Fact]
        public async Task DeleteAsync_NotFound()
        {
            _repoMock.Setup(r => r.GetActiveByIdAsync(10)).ReturnsAsync((GlossaryTerm?)null);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.DeleteTermAsync(10, _testUser));
        }
    }
}
