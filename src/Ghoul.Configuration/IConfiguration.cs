using System;

namespace Ghoul.Configuration
{
    public interface IConfiguration
    {
        T Get<T>(string name);
    }
}
