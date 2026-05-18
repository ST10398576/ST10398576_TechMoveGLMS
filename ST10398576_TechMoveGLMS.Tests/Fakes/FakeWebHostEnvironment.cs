using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace ST10398576_TechMoveGLMS.Tests.Fakes
{
    public class FakeWebHostEnvironment : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Development";
        public string ApplicationName { get; set; } = "TestApp";
        public string WebRootPath { get; set; } = "wwwroot";
        public string ContentRootPath { get; set; } = "testroot";

        // Required interface members
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
