namespace _123vendas.Integration.Helpers;

public static class HttpClientExtensions
{
    public static HttpClient WithToken(this HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    public static HttpClient WithoutToken(this HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization = null;

        return client;
    }
}
