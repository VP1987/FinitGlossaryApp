using FinitiGlossary.Application.DTOs.Request;
using FinitiGlossary.Application.DTOs.Response;
using FinitiGlossary.Application.DTOs.Term.Admin;
using FinitiGlossary.Application.Interfaces.Agregator;
using FinitiGlossary.Application.Interfaces.Repositories.Admin;
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

        // ======================================================================
        // 1. CreateAsync
        // ======================================================================

        [Fact(DisplayName = "CreateAsync_ShouldReturnSuccess_WhenDraftIsCreated")]
        public async Task CreateAsync_SuccessPath()
        {
            var request = new CreateGlossaryRequest("Test Term", "Test Definition");
            _repoMock.Setup(r => r.CreateAsync(It.IsAny<GlossaryTerm>())).ReturnsAsync(true);

            var result = await _sut.CreateTermAsync(request, _testUser);

            Assert.IsType<CreateResult>(result);
            Assert.Equal("Draft created successfully.", result.Message);
            _repoMock.Verify(r => r.CreateAsync(It.Is<GlossaryTerm>(t => t.Term == request.Term && t.Definition == request.Definition)), Times.Once);
        }

        [Fact(DisplayName = "CreateAsync_ShouldThrowException_WhenRepositoryFails")]
        public async Task CreateAsync_Fail()
        {
            var request = new CreateGlossaryRequest("X", "Y");

            _repoMock.Setup(r => r.CreateAsync(It.IsAny<GlossaryTerm>()))
                .ReturnsAsync(false);

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _sut.CreateTermAsync(request, _testUser));
        }

        // ======================================================================
        // 2. UpdateAsync
        // ======================================================================

        [Fact(DisplayName = "UpdateAsync_ShouldReturnSuccess_AndArchivePrevious")]
        public async Task UpdateAsync_SuccessPath()
        {
            const int termId = 10;
            var stableId = Guid.NewGuid();
            var existingTerm = new GlossaryTerm { Id = termId, StableId = stableId, Term = "Old", Definition = "Old", CreatedById = TestUserId.ToString() };
            var request = new UpdateGlossaryRequest("New Term", "New Def", TermStatus.Published);

            _repoMock.Setup(r => r.GetActiveByIdAsync(termId)).ReturnsAsync(existingTerm);
            _repoMock.Setup(r => r.GetArchivedByStableIdAsync(stableId)).ReturnsAsync(new List<ArchivedGlossaryTerm>());
            _repoMock.Setup(r => r.GetLatestVersionAsync(stableId)).ReturnsAsync(1);
            _repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

            var result = await _sut.UpdateTermAsync(termId, request, _testUser);

            Assert.IsType<UpdateResult>(result);
            _repoMock.Verify(r => r.AddArchivedTerm(It.Is<ArchivedGlossaryTerm>(a => a.Term == existingTerm.Term)), Times.Once);
            _repoMock.Verify(r => r.RemoveActiveTerm(existingTerm), Times.Once);
            _repoMock.Verify(r => r.AddActiveTerm(It.Is<GlossaryTerm>(t => t.Term == request.Term && t.Definition == request.Definition)), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact(DisplayName = "UpdateAsync_ShouldNotArchive_IfIdenticalVersionExists")]
        public async Task UpdateAsync_IdenticalVersionExists()
        {
            const int termId = 10;
            var stableId = Guid.NewGuid();

            var existingTerm = new GlossaryTerm { Id = termId, StableId = stableId, Term = "Same", Definition = "Same" };
            var archivedList = new List<ArchivedGlossaryTerm> { new ArchivedGlossaryTerm { Term = "Same", Definition = "Same" } };


            var request = new UpdateGlossaryRequest("X", "Y", TermStatus.Published);

            _repoMock.Setup(r => r.GetActiveByIdAsync(termId)).ReturnsAsync(existingTerm);
            _repoMock.Setup(r => r.GetArchivedByStableIdAsync(stableId)).ReturnsAsync(archivedList);
            _repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

            await _sut.UpdateTermAsync(termId, request, _testUser);

            _repoMock.Verify(r => r.AddArchivedTerm(It.IsAny<ArchivedGlossaryTerm>()), Times.Never);
        }

        [Fact(DisplayName = "UpdateAsync_ShouldThrow_WhenActiveTermNotFound")]
        public async Task UpdateAsync_NotFound()
        {
            _repoMock.Setup(r => r.GetActiveByIdAsync(10)).ReturnsAsync((GlossaryTerm?)null);

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _sut.UpdateTermAsync(10, new UpdateGlossaryRequest("X", "Y", TermStatus.Published), _testUser));
        }

        // ======================================================================
        // 3. Restore
        // ======================================================================

        [Fact(DisplayName = "RestoreAsync_ShouldRestoreVersion_WhenActiveExists")]
        public async Task RestoreAsync_Success()
        {
            var stableId = Guid.NewGuid();
            var archivedVersion = new ArchivedGlossaryTerm { StableId = stableId, Version = 3, Term = "V3", Definition = "DEF3", CreatedById = "99" };
            var existingActive = new GlossaryTerm { Id = 99, StableId = stableId, Term = "Active", Definition = "Active", CreatedById = TestUserId.ToString() };

            _repoMock.Setup(r => r.GetArchivedVersionAsync(stableId, 3)).ReturnsAsync(archivedVersion);
            _repoMock.Setup(r => r.GetActiveByStableIdAsync(stableId)).ReturnsAsync(existingActive);
            _repoMock.Setup(r => r.GetLatestVersionAsync(stableId)).ReturnsAsync(5);
            _repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

            var result = await _sut.RestoreTermVersionAsync(stableId, 3, _testUser);

            Assert.Contains("restored", result.Message);
            // Provera da li je postojeća aktivna verzija automatski arhivirana
            _repoMock.Verify(r => r.AddArchivedTerm(It.Is<ArchivedGlossaryTerm>(a => a.ChangeSummary.Contains("Auto-archived"))), Times.Once);
            // Provera da li je postojeća aktivna verzija uklonjena
            _repoMock.Verify(r => r.RemoveActiveTerm(existingActive), Times.Once);
            // Provera da li je nova (restorovana) verzija dodata
            _repoMock.Verify(r => r.AddActiveTerm(It.Is<GlossaryTerm>(t => t.Term == archivedVersion.Term)), Times.Once);
            // Provera da li je originalna arhivirana verzija ažurirana (RestoredAt polje)
            _repoMock.Verify(r => r.UpdateArchivedTerm(archivedVersion), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact(DisplayName = "RestoreAsync_ShouldReturnIdentical_WhenSameAsActive")]
        public async Task RestoreAsync_Identical()
        {
            var stableId = Guid.NewGuid();
            var archivedVersion = new ArchivedGlossaryTerm { StableId = stableId, Term = "Same", Definition = "Same" };
            var active = new GlossaryTerm { StableId = stableId, Term = "Same", Definition = "Same" };

            _repoMock.Setup(r => r.GetArchivedVersionAsync(stableId, 3)).ReturnsAsync(archivedVersion);
            _repoMock.Setup(r => r.GetActiveByStableIdAsync(stableId)).ReturnsAsync(active);

            var result = await _sut.RestoreTermVersionAsync(stableId, 3, _testUser);

            Assert.Equal("Identical version already active.", result.Message);
            // Provera da se ništa nije menjalo u bazi
            _repoMock.Verify(r => r.AddArchivedTerm(It.IsAny<ArchivedGlossaryTerm>()), Times.Never);
            _repoMock.Verify(r => r.RemoveActiveTerm(It.IsAny<GlossaryTerm>()), Times.Never);
            _repoMock.Verify(r => r.AddActiveTerm(It.IsAny<GlossaryTerm>()), Times.Never);
            _repoMock.Verify(r => r.UpdateArchivedTerm(It.IsAny<ArchivedGlossaryTerm>()), Times.Never);
        }

        [Fact(DisplayName = "RestoreAsync_ShouldThrow_WhenNotFound")]
        public async Task RestoreAsync_NotFound()
        {
            var stableId = Guid.NewGuid();

            _repoMock.Setup(r => r.GetArchivedVersionAsync(stableId, 99))
                .ReturnsAsync((ArchivedGlossaryTerm?)null);

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _sut.RestoreTermVersionAsync(stableId, 99, _testUser));
        }

        // ======================================================================
        // 4. History
        // ======================================================================

        [Fact(DisplayName = "GetHistoryAsync_ShouldReturnSuccess")]
        public async Task GetHistoryAsync_Success()
        {
            var stableId = Guid.NewGuid();

            _repoMock.Setup(r => r.GetActiveByStableIdAsync(stableId)).ReturnsAsync(new GlossaryTerm());
            _repoMock.Setup(r => r.GetArchivedByStableIdAsync(stableId)).ReturnsAsync(new List<ArchivedGlossaryTerm>());
            _userRepoMock.Setup(r => r.GetAllUsersAsync()).ReturnsAsync(new List<User> { _dbUser });

            _viewHistoryAggregatorMock.Setup(a => a.AggregateHistoryView(
                It.IsAny<List<GlossaryTerm>>(),
                It.IsAny<List<ArchivedGlossaryTerm>>(),
                It.IsAny<List<User>>()))
            .Returns(new List<GlossaryAdminTermDTO>());

            await _sut.GetTermHistoryAsync(stableId, _testUser);

            _viewHistoryAggregatorMock.Verify(a => a.AggregateHistoryView(
                It.IsAny<List<GlossaryTerm>>(),
                It.IsAny<List<ArchivedGlossaryTerm>>(),
                It.IsAny<List<User>>()), Times.Once);
        }

        [Fact(DisplayName = "GetHistoryAsync_ShouldThrow_WhenNoHistory")]
        public async Task GetHistoryAsync_NotFound()
        {
            var stableId = Guid.NewGuid();

            _repoMock.Setup(r => r.GetActiveByStableIdAsync(stableId)).ReturnsAsync((GlossaryTerm?)null);
            _repoMock.Setup(r => r.GetArchivedByStableIdAsync(stableId)).ReturnsAsync(new List<ArchivedGlossaryTerm>());

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _sut.GetTermHistoryAsync(stableId, _testUser));
        }

        // ======================================================================
        // 5. List filtering & sorting
        // ======================================================================

        [Fact(DisplayName = "GetListAsync_ShouldFilterBySearchTerm")]
        public async Task GetListAsync_FiltersBySearch()
        {
            var query = new AdminTermQuery { Offset = 0, Limit = 10, Search = "test" };

            // Setup za repozitorijum je nebitan ovde, jer testiramo filtriranje na listi
            _repoMock.Setup(r => r.GetActiveTermsForAdminViewAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<GlossaryTerm>());
            _repoMock.Setup(r => r.GetArchivedTermsForAdminViewAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<ArchivedGlossaryTerm>());
            _repoMock.Setup(r => r.GetAllUsersAsync()).ReturnsAsync(new List<User>());

            // Aggregator vraća listu, a mi proveravamo da li je filtriranje na toj listi ispravno.
            _viewAgregatorMock.Setup(a => a.AggregateAdminView(
                It.IsAny<List<GlossaryTerm>>(),
                It.IsAny<List<ArchivedGlossaryTerm>>(),
                It.IsAny<List<User>>(),
                It.IsAny<int?>(),
                It.IsAny<bool>()))
            .Returns(new List<GlossaryAdminTermDTO>
            {
                new GlossaryAdminTermDTO { Term = "Test term", Definition = "x", Status = 1, CreatedOrArchivedAt = DateTime.UtcNow },
                new GlossaryAdminTermDTO { Term = "Other", Definition = "none", Status = 1, CreatedOrArchivedAt = DateTime.UtcNow }
            });

            var result = await _sut.GetAdminTermListAsync(_testUser, query);

            Assert.Single(result.Data);
            Assert.Equal("Test term", result.Data.First().Term);
        }

        [Fact(DisplayName = "GetListAsync_ShouldSortByTermAscending")]
        public async Task GetListAsync_SortsByTermAsc()
        {
            var query = new AdminTermQuery { Offset = 0, Limit = 10, Sort = "az" };

            _repoMock.Setup(r => r.GetActiveTermsForAdminViewAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<GlossaryTerm>());
            _repoMock.Setup(r => r.GetArchivedTermsForAdminViewAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<ArchivedGlossaryTerm>());
            _repoMock.Setup(r => r.GetAllUsersAsync()).ReturnsAsync(new List<User>());

            _viewAgregatorMock.Setup(a => a.AggregateAdminView(
                It.IsAny<List<GlossaryTerm>>(),
                It.IsAny<List<ArchivedGlossaryTerm>>(),
                It.IsAny<List<User>>(),
                It.IsAny<int?>(),
                It.IsAny<bool>()))
            .Returns(new List<GlossaryAdminTermDTO>
            {
                new GlossaryAdminTermDTO { Term = "B term", Definition = "x", Status = 1, CreatedOrArchivedAt = DateTime.UtcNow },
                new GlossaryAdminTermDTO { Term = "A term", Definition = "y", Status = 1, CreatedOrArchivedAt = DateTime.UtcNow }
            });

            var result = await _sut.GetAdminTermListAsync(_testUser, query);

            Assert.Equal(2, result.Data.Count);
            Assert.Equal("A term", result.Data.First().Term);
        }

        [Fact(DisplayName = "GetListAsync_ShouldReturnEmpty_WhenNoMatches")]
        public async Task GetListAsync_NoResults()
        {
            var query = new AdminTermQuery { Offset = 0, Limit = 10, Search = "xyz" };

            _repoMock.Setup(r => r.GetActiveTermsForAdminViewAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<GlossaryTerm>());
            _repoMock.Setup(r => r.GetArchivedTermsForAdminViewAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<ArchivedGlossaryTerm>());
            _repoMock.Setup(r => r.GetAllUsersAsync()).ReturnsAsync(new List<User>());

            _viewAgregatorMock.Setup(a => a.AggregateAdminView(
                It.IsAny<List<GlossaryTerm>>(),
                It.IsAny<List<ArchivedGlossaryTerm>>(),
                It.IsAny<List<User>>(),
                It.IsAny<int?>(),
                It.IsAny<bool>()))
            .Returns(new List<GlossaryAdminTermDTO>
            {
                new GlossaryAdminTermDTO { Term = "Test term", Definition = "x", Status = 1, CreatedOrArchivedAt = DateTime.UtcNow }
            });

            var result = await _sut.GetAdminTermListAsync(_testUser, query);

            Assert.Empty(result.Data);
        }

        // ======================================================================
        // 6. Archive / Publish / Delete
        // ======================================================================

        [Fact(DisplayName = "ArchiveAsync_ShouldReturnSuccess")]
        public async Task ArchiveAsync_SuccessPath()
        {
            const int termId = 10;
            var stableId = Guid.NewGuid();
            var term = new GlossaryTerm { Id = termId, StableId = stableId, CreatedById = TestUserId.ToString() };

            _repoMock.Setup(r => r.GetActiveByIdAsync(termId)).ReturnsAsync(term);
            _repoMock.Setup(r => r.GetLatestVersionAsync(stableId)).ReturnsAsync(1);
            _repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

            var result = await _sut.ArchiveTermAsync(termId, _testUser);

            Assert.IsType<ArchiveResult>(result);
            _repoMock.Verify(r => r.AddArchivedTerm(It.Is<ArchivedGlossaryTerm>(a => a.OriginalTermId == termId)), Times.Once);
            _repoMock.Verify(r => r.RemoveActiveTerm(term), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact(DisplayName = "ArchiveAsync_ShouldThrow_WhenNotFound")]
        public async Task ArchiveAsync_NotFound()
        {
            _repoMock.Setup(r => r.GetActiveByIdAsync(10)).ReturnsAsync((GlossaryTerm?)null);

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _sut.ArchiveTermAsync(10, _testUser));
        }

        [Fact(DisplayName = "PublishAsync_ShouldPublish")]
        public async Task PublishAsync_SuccessPath()
        {
            const int termId = 10;
            var term = new GlossaryTerm { Id = termId, Status = TermStatus.Draft };

            _repoMock.Setup(r => r.GetActiveByIdAsync(termId)).ReturnsAsync(term);
            _repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

            var result = await _sut.PublishTermAsync(termId, _testUser);

            Assert.Equal(TermStatus.Published, term.Status);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact(DisplayName = "PublishAsync_ShouldThrow_WhenNotFound")]
        public async Task PublishAsync_NotFound()
        {
            _repoMock.Setup(r => r.GetActiveByIdAsync(10)).ReturnsAsync((GlossaryTerm?)null);

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _sut.PublishTermAsync(10, _testUser));
        }

        [Fact(DisplayName = "DeleteAsync_ShouldReturnSuccess")]
        public async Task DeleteAsync_SuccessPath()
        {
            const int termId = 10;
            var term = new GlossaryTerm { Id = termId };

            _repoMock.Setup(r => r.GetActiveByIdAsync(termId)).ReturnsAsync(term);
            _repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

            var result = await _sut.DeleteTermAsync(termId, _testUser);

            Assert.Equal("Deleted.", result.Message);
            _repoMock.Verify(r => r.RemoveActiveTerm(term), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact(DisplayName = "DeleteAsync_ShouldThrow_WhenNotFound")]
        public async Task DeleteAsync_NotFound()
        {
            _repoMock.Setup(r => r.GetActiveByIdAsync(10)).ReturnsAsync((GlossaryTerm?)null);

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _sut.DeleteTermAsync(10, _testUser));
        }
    }
}