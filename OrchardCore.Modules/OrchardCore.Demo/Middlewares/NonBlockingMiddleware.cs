﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Demo
{
    public class NonBlockingMiddleware
    {
        private readonly RequestDelegate _next;

        public NonBlockingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            httpContext.Response.Headers.Add("Orchard", "2.0");
            await _next.Invoke(httpContext);
        }
    }
}
