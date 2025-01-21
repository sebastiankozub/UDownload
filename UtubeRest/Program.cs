using Microsoft.Extensions.Options;
using UtubeRest.Controllers;

namespace UtubeRest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            ////

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    builder => builder.WithOrigins("http://localhost:64100")
                                        .WithOrigins("http://127.0.0.1:64100")
                                      .AllowAnyHeader()
                                      .AllowAnyMethod());
            });

            //options.AddPolicy("AllowSpecificOrigin",
            //    builder => builder.WithOrigins("http://localhost:64100")
            //          .AllowAnyHeader()
            //          .AllowAnyMethod());


            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            //
            app.UseCors("AllowSpecificOrigin");
            //

            //app.UseAuthorization();



            //
            app.MapControllers();

            app.MapAudioManifestEndpoints();
            //


            app.Run();


            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}
            //ccapp.UseRouting();
            //app.UseCors("AllowSpecificOrigin");
            //ccapp.UseAuthorization();
            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapControllers();
            //});
        }
    }
}
