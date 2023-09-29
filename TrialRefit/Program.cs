using Refit;
using TrialRefit.IPostApi;

var api=RestService.For<IPostApi>("https://jsonplaceholder.typicode.com");

var posts = await api.GetPosts();

foreach(var post in posts)
{
    Console.WriteLine($"Post Id:{post.Id} \nTitle: {post.Title} \nBody: {post.Body}");
    Console.WriteLine();
}

Console.ReadKey();