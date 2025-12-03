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

        [Fact(DisplayName = "CreateAsync_ShouldReturnSuccess_WhenDraftIsCreated")]
        public async Task CreateAsync_SuccessPath()
        {
            var request = new CreateGlossaryRequest("Test Term", "Test Definition");
            _repoMock.Setup(r => r.CreateAsync(It.IsAny<GlossaryTerm>())).ReturnsAsync(true);

            var result = await _sut.CreateTermAsync(request, _testUser);

            Assert.Equal("Draft created successfully.", result.Message);
            _repoMock.Verify(r => r.CreateAsync(It.IsAny<GlossaryTerm>()), Times.Once);
        }

        [Fact(DisplayName = "CreateAsync_ShouldThrow_WhenRepoFails")]
        public async Task CreateAsync_Fail()
        {
            var request = new CreateGlossaryRequest("X", "Y");
            _repoMock.Setup(r => r.CreateAsync(It.IsAny<GlossaryTerm>())).ReturnsAsync(false);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.CreateTermAsync(request, _testUser));
        }

        [Fact(DisplayName = "UpdateAsync_ShouldArchivePreviousVersion")]
        public async Task UpdateAsync_SuccessPath()
        {
            const int termId = 10;
            var stableId = Guid.NewGuid();
            var existingActive = new GlossaryTerm
            {
                Id = termId,
                StableId = stableId,
                Term = "Old",
                Definition = "Old",
                CreatedById = TestUserId
            };

            var req = new UpdateGlossaryRequest("New", "NewD", TermStatus.Published);

            _repoMock.Setup(r => r.GetActiveByIdAsync(termId)).ReturnsAsync(existingActive);
            _repoMock.Setup(r => r.GetArchivedByStableIdAsync(stableId)).ReturnsAsync(new List<ArchivedGlossaryTerm>());
            _repoMock.Setup(r => r.GetLatestVersionAsync(stableId)).ReturnsAsync(1);
            _repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

            var result = await _sut.UpdateTermAsync(termId, req, _testUser);

            Assert.Equal("Term updated successfully.", result.Message);
            _repoMock.Verify(r => r.AddArchivedTerm(It.IsAny<ArchivedGlossaryTerm>()), Times.Once);
            _repoMock.Verify(r => r.AddActiveTerm(It.IsAny<GlossaryTerm>()), Times.Once);
        }

        [Fact(DisplayName = "UpdateAsync_ShouldNotArchive_WhenIdenticalExists")]
        public async Task UpdateAsync_IdenticalVersion()
        {
            const int id = 10;
            var stableId = Guid.NewGuid();

            var existing = new GlossaryTerm { Id = id, StableId = stableId, Term = "Same", Definition = "Same" };
            var archived = new List<ArchivedGlossaryTerm> { new ArchivedGlossaryTerm { Term = "Same", Definition = "Same" } };

            _repoMock.Setup(r => r.GetActiveByIdAsync(id)).ReturnsAsync(existing);
            _repoMock.Setup(r => r.GetArchivedByStableIdAsync(stableId)).ReturnsAsync(archived);
            _repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

            await _sut.UpdateTermAsync(id, new UpdateGlossaryRequest("X", "Y", TermStatus.Published), _testUser);

            _repoMock.Verify(r => r.AddArchivedTerm(It.IsAny<ArchivedGlossaryTerm>()), Times.Never);
        }

        [Fact(DisplayName = "UpdateAsync_ShouldThrow_WhenNotFound")]
        public async Task UpdateAsync_NotFound()
        {
            _repoMock.Setup(r => r.GetActiveByIdAsync(10)).ReturnsAsync((GlossaryTerm?)null);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.UpdateTermAsync(10, new UpdateGlossaryRequest("A", "B", TermStatus.Published), _testUser));
        }

        [Fact(DisplayName = "RestoreAsync_ShouldRestoreVersion")]
        public async Task RestoreAsync_Success()
        {
            var stable = Guid.NewGuid();
            var archived = new ArchivedGlossaryTerm { StableId = stable, Version = 3, Term = "V3", Definition = "D3", CreatedById = 99 };
            var active = new GlossaryTerm { Id = 5, StableId = stable, Term = "Active", Definition = "Active", CreatedById = 5 };

            _repoMock.Setup(r => r.GetArchivedVersionAsync(stable, 3)).ReturnsAsync(archived);
            _repoMock.Setup(r => r.GetActiveByStableIdAsync(stable)).ReturnsAsync(active);
            _repoMock.Setup(r => r.GetLatestVersionAsync(stable)).ReturnsAsync(5);
            _repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

            var result = await _sut.RestoreTermVersionAsync(stable, 3, _testUser);

            Assert.Contains("restored", result.Message);
            _repoMock.Verify(r => r.AddArchivedTerm(It.IsAny<ArchivedGlossaryTerm>()), Times.Once);
            _repoMock.Verify(r => r.RemoveActiveTerm(active), Times.Once);
            _repoMock.Verify(r => r.AddActiveTerm(It.IsAny<GlossaryTerm>()), Times.Once);
            _repoMock.Verify(r => r.UpdateArchivedTerm(archived), Times.Once);
        }

        [Fact(DisplayName = "RestoreAsync_ShouldReturnIdenticalMessage")]
        public async Task RestoreAsync_Identical()
        {
            var stable = Guid.NewGuid();
            var archived = new ArchivedGlossaryTerm { StableId = stable, Term = "Same", Definition = "Same" };
            var active = new GlossaryTerm { StableId = stable, Term = "Same", Definition = "Same" };

            _repoMock.Setup(r => r.GetArchivedVersionAsync(stable, 3)).ReturnsAsync(archived);
            _repoMock.Setup(r => r.GetActiveByStableIdAsync(stable)).ReturnsAsync(active);

            var result = await _sut.RestoreTermVersionAsync(stable, 3, _testUser);

            Assert.Equal("Identical version already active.", result.Message);
            _repoMock.Verify(r => r.AddArchivedTerm(It.IsAny<ArchivedGlossaryTerm>()), Times.Never);
        }

        [Fact(DisplayName = "RestoreAsync_ShouldThrow_WhenArchivedMissing")]
        public async Task RestoreAsync_NotFound()
        {
            var stable = Guid.NewGuid();

            _repoMock.Setup(r => r.GetArchivedVersionAsync(stable, 99))
                .ReturnsAsync((ArchivedGlossaryTerm?)null);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.RestoreTermVersionAsync(stable, 99, _testUser));
        }

        [Fact(DisplayName = "GetHistoryAsync_ShouldReturnHistory")]
        public async Task GetHistoryAsync_Success()
        {
            var stable = Guid.NewGuid();

            _repoMock.Setup(r => r.GetActiveByStableIdAsync(stable)).ReturnsAsync(new GlossaryTerm());
            _repoMock.Setup(r => r.GetArchivedByStableIdAsync(stable)).ReturnsAsync(new List<ArchivedGlossaryTerm>());
            _userRepoMock.Setup(r => r.GetAllUsersAsync()).ReturnsAsync(new List<User> { _dbUser });

            _viewHistoryAggregatorMock.Setup(a => a.AggregateHistoryView(
                It.IsAny<List<GlossaryTerm>>(),
                It.IsAny<List<ArchivedGlossaryTerm>>(),
                It.IsAny<List<User>>()))
            .Returns(new List<GlossaryAdminTermDTO>());

            await _sut.GetTermHistoryAsync(stable, _testUser);

            _viewHistoryAggregatorMock.Verify(a => a.AggregateHistoryView(
                It.IsAny<List<GlossaryTerm>>(),
                It.IsAny<List<ArchivedGlossaryTerm>>(),
                It.IsAny<List<User>>()), Times.Once);
        }

        [Fact(DisplayName = "GetHistoryAsync_ShouldThrow_WhenEmpty")]
        public async Task GetHistoryAsync_NotFound()
        {
            var stable = Guid.NewGuid();

            _repoMock.Setup(r => r.GetActiveByStableIdAsync(stable)).ReturnsAsync((GlossaryTerm?)null);
            _repoMock.Setup(r => r.GetArchivedByStableIdAsync(stable))
                .ReturnsAsync(new List<ArchivedGlossaryTerm>());

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.GetTermHistoryAsync(stable, _testUser));
        }

        [Fact(DisplayName = "GetListAsync_ShouldFilterBySearchTerm")]
        public async Task GetListAsync_FilterSearch()
        {
            var query = new AdminTermQuery { Offset = 0, Limit = 10, Search = "test" };

            _repoMock.Setup(r => r.GetActiveTermsForAdminViewAsync(It.IsAny<GetTermsAdminRequest>()))
                .ReturnsAsync(new List<GlossaryTerm>());
            _repoMock.Setup(r => r.GetArchivedTermsForAdminViewAsync(It.IsAny<GetTermsAdminRequest>()))
                .ReturnsAsync(new List<ArchivedGlossaryTerm>());
            _repoMock.Setup(r => r.GetAllUsersAsync()).ReturnsAsync(new List<User>());

            _viewAgregatorMock.Setup(a => a.AggregateAdminView(
                It.IsAny<List<GlossaryTerm>>(),
                It.IsAny<List<ArchivedGlossaryTerm>>(),
                It.IsAny<List<User>>(),
                It.IsAny<int>(),
                It.IsAny<bool>()))
            .Returns(new List<GlossaryAdminTermDTO>
            {
                new GlossaryAdminTermDTO { Term = "Test term", Definition = "x", Status = 1, CreatedOrArchivedAt = DateTime.UtcNow },
                new GlossaryAdminTermDTO { Term = "Other", Definition = "none", Status = 1, CreatedOrArchivedAt = DateTime.UtcNow }
            });

            var result = await _sut.GetAdminTermListAsync(_testUser, query);

            Assert.Single(result.Data);
        }

        [Fact(DisplayName = "GetListAsync_ShouldSortAscending")]
        public async Task GetListAsync_SortAZ()
        {
            var query = new AdminTermQuery { Offset = 0, Limit = 10, Sort = "az" };

            _repoMock.Setup(r => r.GetActiveTermsForAdminViewAsync(It.IsAny<GetTermsAdminRequest>()))
                .ReturnsAsync(new List<GlossaryTerm>());
            _repoMock.Setup(r => r.GetArchivedTermsForAdminViewAsync(It.IsAny<GetTermsAdminRequest>()))
                .ReturnsAsync(new List<ArchivedGlossaryTerm>());
            _repoMock.Setup(r => r.GetAllUsersAsync()).ReturnsAsync(new List<User>());

            _viewAgregatorMock.Setup(a => a.AggregateAdminView(
                It.IsAny<List<GlossaryTerm>>(),
                It.IsAny<List<ArchivedGlossaryTerm>>(),
                It.IsAny<List<User>>(),
                It.IsAny<int>(),
                It.IsAny<bool>()))
            .Returns(new List<GlossaryAdminTermDTO>
            {
                new GlossaryAdminTermDTO { Term = "B", Definition = "x", Status = 1, CreatedOrArchivedAt = DateTime.UtcNow },
                new GlossaryAdminTermDTO { Term = "A", Definition = "y", Status = 1, CreatedOrArchivedAt = DateTime.UtcNow }
            });

            var result = await _sut.GetAdminTermListAsync(_testUser, query);

            Assert.Equal("A", result.Data.First().Term);
        }

        [Fact(DisplayName = "GetListAsync_ShouldReturnEmptyWhenNoMatches")]
        public async Task GetListAsync_NoResults()
        {
            var query = new AdminTermQuery { Offset = 0, Limit = 10, Search = "xyz" };

            _repoMock.Setup(r => r.GetActiveTermsForAdminViewAsync(It.IsAny<GetTermsAdminRequest>()))
                .ReturnsAsync(new List<GlossaryTerm>());
            _repoMock.Setup(r => r.GetArchivedTermsForAdminViewAsync(It.IsAny<GetTermsAdminRequest>()))
                .ReturnsAsync(new List<ArchivedGlossaryTerm>());
            _repoMock.Setup(r => r.GetAllUsersAsync()).ReturnsAsync(new List<User>());

            _viewAgregatorMock.Setup(a => a.AggregateAdminView(
                It.IsAny<List<GlossaryTerm>>(),
                It.IsAny<List<ArchivedGlossaryTerm>>(),
                It.IsAny<List<User>>(),
                It.IsAny<int>(),
                It.IsAny<bool>()))
            .Returns(new List<GlossaryAdminTermDTO>
            {
                new GlossaryAdminTermDTO { Term = "Test", Definition = "x", Status = 1, CreatedOrArchivedAt = DateTime.UtcNow }
            });

            var result = await _sut.GetAdminTermListAsync(_testUser, query);

            Assert.Empty(result.Data);
        }

        [Fact(DisplayName = "ArchiveAsync_ShouldReturnSuccess")]
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
            _repoMock.Verify(r => r.AddArchivedTerm(It.IsAny<ArchivedGlossaryTerm>()), Times.Once);
            _repoMock.Verify(r => r.RemoveActiveTerm(term), Times.Once);
        }

        [Fact(DisplayName = "ArchiveAsync_ShouldThrow_WhenNotFound")]
        public async Task ArchiveAsync_NotFound()
        {
            _repoMock.Setup(r => r.GetActiveByIdAsync(10)).ReturnsAsync((GlossaryTerm?)null);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.ArchiveTermAsync(10, _testUser));
        }

        [Fact(DisplayName = "PublishAsync_ShouldPublish")]
        public async Task PublishAsync_Success()
        {
            const int id = 10;
            var term = new GlossaryTerm { Id = id, Status = TermStatus.Draft };

            _repoMock.Setup(r => r.GetActiveByIdAsync(id)).ReturnsAsync(term);
            _repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

            var result = await _sut.PublishTermAsync(id, _testUser);

            Assert.Equal(TermStatus.Published, term.Status);
        }

        [Fact(DisplayName = "PublishAsync_ShouldThrow_WhenNotFound")]
        public async Task PublishAsync_NotFound()
        {
            _repoMock.Setup(r => r.GetActiveByIdAsync(10)).ReturnsAsync((GlossaryTerm?)null);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.PublishTermAsync(10, _testUser));
        }

        [Fact(DisplayName = "DeleteAsync_ShouldReturnSuccess")]
        public async Task DeleteAsync_Success()
        {
            const int id = 10;
            var term = new GlossaryTerm { Id = id };

            _repoMock.Setup(r => r.GetActiveByIdAsync(id)).ReturnsAsync(term);
            _repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

            var result = await _sut.DeleteTermAsync(id, _testUser);

            Assert.Equal("Deleted.", result.Message);
        }

        [Fact(DisplayName = "DeleteAsync_ShouldThrow_WhenNotFound")]
        public async Task DeleteAsync_NotFound()
        {
            _repoMock.Setup(r => r.GetActiveByIdAsync(10)).ReturnsAsync((GlossaryTerm?)null);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.DeleteTermAsync(10, _testUser));
        }
    }
}
