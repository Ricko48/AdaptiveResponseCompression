namespace ApiServerDemo.Extensions;

public static class WebApplicationExtensions
{
    public static void Configure(this WebApplication app)
    {
        // CORS configuration for the Blazor app
        app.UseCors("AllowSpecificOrigins");

        // swagger
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();
    }
}