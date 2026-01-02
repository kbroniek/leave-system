namespace LeaveSystem.Seed.PostgreSQL;

using System.Threading;
using LeaveSystem.Seed.PostgreSQL.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph;
using Microsoft.Graph.Models;

internal class GraphSeeder(OmbContext context, GraphServiceClient graphClient, string defaultUsersPassword, string issuer)
{
    internal async Task<IReadOnlyCollection<CreatedUser>> SeedUsers()
    {
        Console.WriteLine("Getting users from DB");
        var users = await context.Users.Select(x => new
        {
            x.Userid,
            FirstName = x.Name,
            LastName = x.Lastname,
            x.Email,
            JobTitle = x.PositionPosition.Description
        }).ToListAsync();
        var usersFromGraph = await GetUsersFromGraph();
        var results = new List<CreatedUser>();
        var i = 0;
        foreach (var user in users)
        {
            var userFound = usersFromGraph.FirstOrDefault(x => x.GivenName == user.FirstName && x.Surname == user.LastName);
            if (userFound is not null)
            {
                results.Add(new CreatedUser(userFound.Id, user.Userid, user.Email, $"{user.FirstName} {user.LastName}"));
                Console.WriteLine($"User found {user.Email}");
                ++i;
                continue;
            }
            var result = await CreateUser(user.Userid, user.Email, user.FirstName, user.LastName, user.JobTitle);
            if (result.Success)
            {
                results.Add(result.User);
                Console.Write($"\rCreated {++i} items");
            }
        }
        Console.WriteLine($"\rSaved {i} users to graph API");
        return results;
    }

    private async Task<IReadOnlyCollection<Microsoft.Graph.Models.User>> GetUsersFromGraph()
    {
        var users = await graphClient.Users
            .GetAsync(_ => _.QueryParameters.Select = ["id", "mail", "givenName", "surname",]);
        return await GetAll(graphClient, users, CancellationToken.None);
    }

    private async Task<(bool Success, CreatedUser? User)> CreateUser(int userid, string? email, string? firstName, string? lastName, string? jobTitle)
    {
        try
        {
            var principalId = Guid.NewGuid();
            var addedUser = await graphClient.Users.PostAsync(new Microsoft.Graph.Models.User
            {
                Mail = email,
                DisplayName = $"{firstName} {lastName}",
                GivenName = firstName,
                Surname = lastName,
                AccountEnabled = true,
                PasswordProfile = new PasswordProfile
                {
                    ForceChangePasswordNextSignIn = false,
                    Password = defaultUsersPassword
                },
                MailNickname = email?.Split('@').FirstOrDefault(),
                Identities =
                [
                    new() {
                        Issuer = issuer,
                        IssuerAssignedId = email,
                        SignInType = "emailAddress"
                    }
                ],
                PasswordPolicies = "DisablePasswordExpiration, DisableStrongPassword",
                UserPrincipalName = $"{principalId}@{issuer}",
                JobTitle = jobTitle
            });
            return (true, new(addedUser.Id, userid, email, $"{firstName} {lastName}"));
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("Another object with the same value for property proxyAddresses already exists"))
            {
                var emailSplit = email.Split('@');
                var newEmail = $"{emailSplit[0]}+{Random.Shared.Next(1, 9)}@{emailSplit[1]}";
                $"Error when creating user {email} {firstName} {lastName}. Try another approach. New email {newEmail}. {ex.Message}".WriteWarning();
                return await CreateUser(userid, newEmail, firstName, lastName, jobTitle);
            }
            $"Error when creating user {email}".WriteError();
            ex.WriteError();
            return (false, null);
        }
    }

    private static async Task<List<Microsoft.Graph.Models.User>> GetAll(GraphServiceClient graphClient, UserCollectionResponse users, CancellationToken cancellationToken)
    {
        var graphUsers = new List<Microsoft.Graph.Models.User>();
        var pageIterator = PageIterator<Microsoft.Graph.Models.User, UserCollectionResponse>
            .CreatePageIterator(graphClient, users,
                (user) =>
                {
                    graphUsers.Add(user);
                    return true;
                }
            );

        await pageIterator.IterateAsync(cancellationToken);
        return graphUsers;
    }
}

internal record CreatedUser(string Id, int OldId, string Email, string Name);
