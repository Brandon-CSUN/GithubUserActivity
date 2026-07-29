using System.Linq.Expressions;
using System.Text.Json;
namespace GithubActivity{
    public class Program
    {
        
        public static async Task Main(String[] args)
        {
            if(args.Length == 0)
            {
                Console.WriteLine($"Usage: dotnet run <username>");
                return;
            }
            string userName = args[0];
            Console.WriteLine($"Fetching activity for {userName}...\n");
            
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "CSharp-Github-CLI");
            string url = $"https://api.github.com/users/{userName}/events";
            HttpResponseMessage response = await client.GetAsync(url);

            if(response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                Console.WriteLine($"Error: User '{userName}' not found");
                return;
            }

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error Github API responded with status code {response.StatusCode}");
                return;
            }

            string jsonResponse = await response.Content.ReadAsStringAsync();
            using JsonDocument doc = JsonDocument.Parse(jsonResponse);
            JsonElement root = doc.RootElement;
            
            if(root.GetArrayLength() == 0)
            {
                Console.WriteLine($"No recent activity found for '{userName}'");
                return;
            }
            Console.WriteLine($"Fetching activity for {userName}");

            foreach(JsonElement element in root.EnumerateArray())
            {
                string type = element.GetProperty("type").GetString() ?? "Unknown";
                string repoName = element.GetProperty("repo").GetProperty("name").GetString() ?? "Unknown";
                string message = type switch
                {
                    "PushEvent" => $"Pushed commits to {repoName}",
                    "WatchEvent" => $"Starred {repoName}",
                    "IssuesEvent" => $"Opened or updated an issue in {repoName}",
                    "IssueCommentEvent" => $"Commented on an issue in {repoName}",
                    "PullRequestEvent" => $"Worked on a pull request in {repoName}",
                    "CreateEvent" => $"Created a branch/repo in {repoName}",
                    _ => $"{type} in {repoName}" // Catch-all for any other event
                };
                Console.WriteLine($"- {message}");
            }
        }
    }
}
