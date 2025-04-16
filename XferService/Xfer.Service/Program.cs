using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.Services;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.Text;
using Xfer.Service.Services;
using Xfer.Data;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Options;

namespace Xfer.Service;

public class Program {
    public static void Main(string[] args) {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddMvc();
        builder.Services.AddSingleton<IConfigureOptions<MvcOptions>, ConfigureMvcOptions>();
        builder.Services.AddSingleton<XferInputFormatter>();
        builder.Services.AddSingleton<XferOutputFormatter>();
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Xfer API", Version = "v1" });
            c.OperationFilter<DefaultMediaTypeFilter>();
        });
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment()) {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.Use(async (context, next) =>
        {
            if (!context.Request.Headers.ContainsKey("Accept")) {
                context.Request.Headers["Accept"] = "application/xfer";
            }
            await next();
        });
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}

public class ConfigureMvcOptions : IConfigureOptions<MvcOptions> {
    private readonly ILogger<XferInputFormatter> _inputFormatterLogger;
    private readonly XferInputFormatter _xferInputFormatter;
    private readonly ILogger<XferOutputFormatter> _outputFormatterLogger;
    private readonly XferOutputFormatter _xferOutputFormatter;

    public ConfigureMvcOptions(ILogger<XferInputFormatter> inputFormatterLogger, XferInputFormatter xferInputFormatter, ILogger<XferOutputFormatter> outputFormatterLogger, XferOutputFormatter xferOutputFormatter) {
        _inputFormatterLogger = inputFormatterLogger;
        _xferInputFormatter = xferInputFormatter;
        _outputFormatterLogger = outputFormatterLogger;
        _xferOutputFormatter = xferOutputFormatter;
    }

    public void Configure(MvcOptions options) {
        options.InputFormatters.Insert(0, _xferInputFormatter);
        options.OutputFormatters.Insert(0, _xferOutputFormatter);
    }
}

public class DefaultMediaTypeFilter : IOperationFilter {
    public void Apply(OpenApiOperation operation, OperationFilterContext context) {
        // Add Xfer as a supported media type
        operation.Responses["200"].Content["application/xfer"] = new OpenApiMediaType();

        // Add JSON as another supported media type
        operation.Responses["200"].Content["application/json"] = new OpenApiMediaType();
    }
}

public class XferInputFormatter : TextInputFormatter {
    private readonly ILogger<XferInputFormatter> _logger;

    public XferInputFormatter(ILogger<XferInputFormatter> logger) {
        _logger = logger;
        SupportedMediaTypes.Add("application/xfer");
        SupportedEncodings.Add(Encoding.UTF8);
    }

    protected override bool CanReadType(Type type) => type != null;

    public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding) {
        var request = context.HttpContext.Request;
        using var reader = new StreamReader(request.Body, encoding);
        var content = await reader.ReadToEndAsync();
        _logger.LogInformation(content);

        try {
            // Use the new overload to deserialize into the target type
            var result = XferConvert.Deserialize(content, context.ModelType);
            return await InputFormatterResult.SuccessAsync(result);
        }
        catch (Exception ex) {
            context.ModelState.AddModelError(context.ModelName, $"Invalid Xfer input: {ex.Message}");
            return await InputFormatterResult.FailureAsync();
        }
    }
}

public class XferOutputFormatter : TextOutputFormatter {
    private readonly ILogger<XferOutputFormatter> _logger;
    public XferOutputFormatter(ILogger<XferOutputFormatter> logger) {
        _logger = logger;
        SupportedMediaTypes.Add("application/xfer");
        SupportedEncodings.Add(Encoding.UTF8);
    }

    protected override bool CanWriteType(Type? type) => true;

    public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding) {
        var response = context.HttpContext.Response;
        var content = XferConvert.Serialize(context.Object, Formatting.Pretty);
        _logger.LogInformation(content);
        await response.WriteAsync(content, selectedEncoding);
    }
}