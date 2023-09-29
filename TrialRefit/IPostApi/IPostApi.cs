using Refit;
using TrailRefit.PostModel;

namespace TrialRefit.IPostApi;

public interface IPostApi
{
    [Get("/posts")]
    Task<List<Post>> GetPosts();

    [Get("/posts/{id}")]
    Task<Post> GetPost(int id);

    [Post("/posts")]
    Task<Post> CreatePost([Body] Post post);

    [Put("/posts/{id}")]
    Task<Post> UpdatePost(int id, [Body] Post post);

    [Delete("/posts/{id}")]
    Task DeletePost(int id);
}
