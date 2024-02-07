# ServiceInjection: Una biblioteca para automatizar la inyección de dependencias

ServiceInjection es una biblioteca que utiliza generadores de código fuente para generar automáticamente el código necesario para la inyección de servicios en C#. Los servicios se definen mediante una clase y un atributo personalizado.

## Uso de la biblioteca

A continuación, se muestra un ejemplo de cómo usar esta biblioteca en tu proyecto:

1. Define tu servicio e implementa la interfaz correspondiente:

```csharp
public interface IMyInterface
{
    void DoSomething();
}

public class MyService : IMyInterface
{
    // Implementación del servicio
}
```

2. Aplica el atributo `Service` a tu clase de servicio:

```csharp
[Service(ServiceType.Scoped, nameof(IMyInterface))]
public class MyService : IMyInterface
{
    // Implementación del servicio
}
```

3. Si tienes más de un servicio que implementa la misma interfaz, puedes usar una clave para diferenciarlos:

```csharp
[Service(ServiceType.Scoped, nameof(IMyInterface), "serv1")]
public class MyService1 : IMyInterface
{
    // Implementación del servicio
}

[Service(ServiceType.Scoped, nameof(IMyInterface), "serv2")]
public class MyService2 : IMyInterface
{
    // Implementación del servicio
}
```

4. La biblioteca ServiceInjection generará automáticamente el código necesario para la inyección de estos servicios en tu contenedor de inyección de dependencias:

```csharp
public static class ServiceInjectionExtension
{
    public static IServiceCollection AddServiceInjection(this IServiceCollection services)
    {
        services.AddScoped<IMyInterface, MyService1>("serv1");
        services.AddScoped<IMyInterface, MyService2>("serv2");

        return services;
    }
}
```

5. Luego, en tu método `ConfigureServices` en `Startup.cs`, puedes usar la extensión generada automáticamente para agregar tus servicios al contenedor de inyección de dependencias:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddControllers();
    services.AddServiceInjection();
}
```

6. Finalmente, puedes inyectar tus servicios en tus controladores usando la interfaz y la clave:

```csharp
public class MyController : Controller
{
    private readonly IMyInterface _service1;
    private readonly IMyInterface _service2;

    public MyController([FromKeyedServices("serv1")] IMyInterface myService1, [FromKeyedServices("serv2")] IMyInterface myService2)
    {
        _service1 = myService1;
        _service2 = myService2;
    }

    public IActionResult Index()
    {
        _service1.DoSomething();
        _service2.DoSomething();

        return View();
    }
}
```

Si no estás utilizando una interfaz, puedes omitir el parámetro de la interfaz al aplicar el atributo `Service`:

```csharp
[Service(ServiceType.Singleton)]
public class MySingletonService
{
    // Implementación del servicio
}
```

En este caso, la biblioteca ServiceInjection generará el siguiente código:

```csharp
public static class ServiceInjectionExtension
{
    public static IServiceCollection AddServiceInjection(this IServiceCollection services)
    {
        services.AddSingleton<MySingletonService>();

        return services;
    }
}
```

Y puedes inyectar tu servicio directamente en tus controladores:

```csharp
public class MyController : Controller
{
    private readonly MySingletonService _service;

    public MyController(MySingletonService service)
    {
        _service = service;
    }

    public IActionResult Index()
    {
        _service.DoSomething();

        return View();
    }
}
```