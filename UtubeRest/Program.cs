using Azure.Data.Tables;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Options;
using UtubeRest.Controllers;
using UtubeRest.Data;
using UtubeRest.Options;
using UtubeRest.Service;

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


            var configuration = builder.Configuration;

            //builder.Services.Configure<TableStorageOptions>(
            //    builder.Configuration.GetSection(TableStorageOptions.Section));
            builder.Services.AddOptions<TableStorageOptions>()
                .Bind(builder.Configuration.GetSection(TableStorageOptions.Section))
                .ValidateDataAnnotations()
                .ValidateOnStart();
            builder.Services.AddSingleton(s => s.GetRequiredService<IOptions<TableStorageOptions>>().Value);


            builder.Services.AddOptions<YtDlpOptions>()
                .Bind(builder.Configuration.GetSection("YtDlp"));
            
            builder.Services.AddTransient<YtService>();

            // Add services to the container.


            builder.Services.AddTransient<ITableRepository<TriggerDownloadEntity>, TriggerDownloadRepository>();

            builder.Services.AddHttpClient();

            builder.Services.AddAzureClients(clientBuilder =>
            {
                //var str1 = host.Configuration.GetSection("PayloadStorage:StorageConnectionString");
                //clientBuilder.AddBlobServiceClient(host.Configuration.GetSection("PayloadStorage:StorageConnectionString"))
                //    .WithName("ApiFetchAndCache");

                var str2 = builder.Configuration.GetSection("TableStorage:StorageConnectionString");
                clientBuilder.AddTableServiceClient(builder.Configuration.GetSection("TableStorage:StorageConnectionString")).WithName("UtubeRestClient");
            });

            //builder.Services.AddScoped<TableServiceClient>(sp =>
            //{
            //    var tblStorageOptions = sp.GetRequiredService<TableStorageOptions>();
            //    return new TableServiceClient(tblStorageOptions.StorageConnectionString);
            //});


            //
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
