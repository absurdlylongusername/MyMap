using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MyMap.ApiService.Data;

public static class AppDbContextOptions
{
    

    public static DbContextOptionsBuilder<TContext> ConfigureDesignDbContextOptions<TContext>(this DbContextOptionsBuilder<TContext> options)
        where TContext : DbContext
    {
        options.UseNpgsql("Host=localhost;Database=design_time;Username=postgres;Password=postgres", o => o.UseNetTopologySuite())
            .ConfigureSharedOptions();
        return options;
    }

    public static DbContextOptionsBuilder ConfigureDbContextOptions(this DbContextOptionsBuilder options, NpgsqlDataSource dataSource)
    {
        options.UseNpgsql(dataSource, o => o.UseNetTopologySuite())
           .ConfigureSharedOptions();
        return options;
    }

    private static DbContextOptionsBuilder<TContext> ConfigureSharedOptions<TContext>(this DbContextOptionsBuilder<TContext> options)
        where TContext : DbContext
    {
        options.UseSnakeCaseNamingConvention();
        return options;
    }

    private static DbContextOptionsBuilder ConfigureSharedOptions(this DbContextOptionsBuilder options)
    {
        options.UseSnakeCaseNamingConvention();
        return options;
    }
}