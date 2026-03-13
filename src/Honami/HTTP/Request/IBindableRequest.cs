using Microsoft.AspNetCore.Http;

namespace Shiron.Honami.HTTP.Request;

public interface IBindableRequest<TSelf> where TSelf : IBindableRequest<TSelf> {
    static abstract void Bind(HttpContext context, out TSelf result);
}
