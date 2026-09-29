using SFA.DAS.EmployerProfiles.Data.UnitTests.DatabaseMock;
using SFA.DAS.EmployerProfiles.Data.Users;
using SFA.DAS.EmployerProfiles.Domain.UserProfiles;

namespace SFA.DAS.EmployerProfiles.Data.UnitTests.Users;

public class WhenGettingAllUsers
{
    [Test, RecursiveMoqAutoData]
    public async Task Then_Pages_Use_A_Stable_User_Id_Order_And_Return_Each_User_Once(
        List<UserProfileEntity> users,
        [Frozen] Mock<IEmployerProfilesDataContext> context,
        UserProfileRepository repository)
    {
        var unorderedUsers = users.OrderByDescending(user => user.Id).ToList();
        context.Setup(x => x.UserProfileEntities).ReturnsDbSet(unorderedUsers);

        var returnedUsers = new List<UserProfileEntity>();
        for (var page = 1; page <= users.Count; page++)
        {
            var result = await repository.GetAllUsers(1, page);
            result.TotalCount.Should().Be(users.Count);
            result.UserProfiles.Should().ContainSingle();
            returnedUsers.AddRange(result.UserProfiles);
        }

        returnedUsers.Select(user => user.Id).Should().Equal(users.OrderBy(user => user.Id).Select(user => user.Id));
        returnedUsers.Select(user => user.Id).Should().OnlyHaveUniqueItems();
    }

    [Test, RecursiveMoqAutoData]
    public async Task Then_A_Page_After_The_Last_User_Is_Empty_But_Retains_The_Total_Count(
        List<UserProfileEntity> users,
        [Frozen] Mock<IEmployerProfilesDataContext> context,
        UserProfileRepository repository)
    {
        context.Setup(x => x.UserProfileEntities).ReturnsDbSet(users);

        var result = await repository.GetAllUsers(users.Count, 2);

        result.TotalCount.Should().Be(users.Count);
        result.UserProfiles.Should().BeEmpty();
    }
}
