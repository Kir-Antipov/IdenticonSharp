# IdenticonSharp [![Core NuGet][core-badge]][core-nuget] [![AspNetCore NuGet][asp-badge]][asp-nuget]

Cross-platform extendable library that allows you to create a variety of identicons without any efforts

-------------

## Goal

`IdenticonSharp` aims to collect as many identicon generation algorithms as possible, so anyone could customize the default appearance of avatars in a way that he and his users will like. Down with the boring static default avatars!

So if you have an idea to implement a new (or existing one) identicon - you're welcome to **fork** the project!

------

## Currently available identicons


### [GitHub Identicon](https://github.blog/2013-08-14-identicons/)

![GitHub Identicons](media/github.png)

 - `Provider`: [`IdenticonSharp.Identicons.Defaults.GitHub.GitHubIdenticonProvider`](https://github.com/Kir-Antipov/IdenticonSharp/blob/master/KE.IdenticonSharp/Identicons/Defaults/GitHub/GitHubIdenticonProvider.cs)
 - `Options`:  [`IdenticonSharp.Identicons.Defaults.GitHub.GitHubIdenticonOptions`](https://github.com/Kir-Antipov/IdenticonSharp/blob/master/KE.IdenticonSharp/Identicons/Defaults/GitHub/GitHubIdenticonOptions.cs)

### [QR code](https://www.qrcode.com/)

![QR code](media/qr.png)

Yes, I know that QR code is not essentially an identicon... but why not?

 - `Provider`: [`IdenticonSharp.Identicons.Defaults.QR.QRIdenticonProvider`](https://github.com/Kir-Antipov/IdenticonSharp/blob/master/KE.IdenticonSharp/Identicons/Defaults/QR/QRIdenticonProvider.cs)
 - `Options`:  [`IdenticonSharp.Identicons.Defaults.QR.QRIdenticonOptions`](https://github.com/Kir-Antipov/IdenticonSharp/blob/master/KE.IdenticonSharp/Identicons/Defaults/QR/QRIdenticonOptions.cs)

------

## Usage

Since you're likely to use a single type of identicon within the project, `IdenticonSharp` provides the [`IdenticonManager`](https://github.com/Kir-Antipov/IdenticonSharp/blob/master/KE.IdenticonSharp/IdenticonManager.cs) class, whose `Default` property represents the default identicon generator.

All identicon generators implement the [`IIdenticonProvider`](https://github.com/Kir-Antipov/IdenticonSharp/blob/master/KE.IdenticonSharp/Identicons/IIdenticonProvider.cs) interface, so you can generate an image (or an `svg`-document, if the generator supports it) from `string` or `byte[]`:

```csharp
string value = ...;
// or
byte[] value = ...;

if (IdenticonManager.Default.ProvidesSvg)
{
    var svg = IdenticonManager.Default.CreateSvg(value);
    svg.Save("identicon.svg");
}
else
{
    var img = IdenticonManager.Default.Create(value);
    img.Save("identicon.png");
}
```

If you need to replace *and/or* configure the default identicon generator, you can use the `ConfigureDefault` method as shown below:

```csharp
// Passing the type of generator and its options
IdenticonManager.ConfigureDefault<GitHubIdenticonProvider, GitHubIdenticonOptions>(options => {
    // Configuring the parameters
    options.Background = new Rgba32(240, 240, 240);
    options.SpriteSize = 10;
    options.Size = 256;
    options.HashAlgorithm = HashProvider.SHA512;
});
```

The same can be done less verbose:

```csharp
IdenticonManager.ConfigureDefault<GitHubIdenticonProvider>();

// If the type name adheres to the general style, then you can use its short form
IdenticonManager.ConfigureDefault("GitHub");

IdenticonManager.ConfigureDefault<GitHubIdenticonOptions>("GitHub", options => {
    ...
});

// If the name of the option type adheres to the general style, 
// and its short form coincides with the desired generator, 
// you can use the following form of method invocation
IdenticonManager.ConfigureDefault<GitHubIdenticonOptions>(options => {
    ...
});
```

### ASP.NET Core

Also you can use [`KE.IdenticonSharp.AspNetCore`][asp-nuget] to interact with `ASP.NET Core` projects!

To register `IdenticonSharp` as a service, just edit the `ConfigureServices` method in your `Startup.cs` as follows:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    ...
    services
        .AddIdenticonSharp<GitHubIdenticonProvider, GitHubIdenticonOptions>(options =>
        {
            // Configuring parameters of default IdenticonProvider 
            options.SpriteSize = 10;
            options.Size = 256;
            options.HashAlgorithm = HashProvider.SHA512;
        })
        .Configure<QRIdenticonOptions>("qr", options => 
        {
            // Configuring parameters of QRIdenticonProvider's instance 
            options.Background = new Rgba32(0, 0, 255);
            options.Foreground = new Rgba32(0, 255, 0);
            options.Border = 40;
            options.Scale = 10;
            options.CorrectionLevel = CorrectionLevel.High;
            options.CenterImage = Image.Load(path);
        });
}
```

As you may have noticed, the `AddIdenticonSharp` call is very similar to `ConfigureDefault`. And it's true, so this can be written exactly the same shorter:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    ...
    services.AddIdenticonSharp<GitHubIdenticonOptions>(options => {
        ...
    }).Configure<QRIdenticonOptions>(options => {
        ...
    });
}
```

After that you can enjoy all the charms of *dependency injection* in `ASP.NET Core`:

```csharp
public class HomeController : Controller
{
    private readonly IIdenticonProvider IdenticonProvider;
    private readonly QRIdenticonProvider QRProvider;

    // Framework will pass the configured identicon generators 
    // to the constructor of your controller
    public HomeController(IIdenticonProvider identiconProvider, QRIdenticonProvider qrProvider)
    {
        // Default IdenticonProvider
        IdenticonProvider = identiconProvider;

        // Configured QRIdenticonProvider's instance 
        QRProvider = qrProvider;
    }
}
```

### TagHelpers

`KE.IdenticonSharp.AspNetCore` provides several tag helpers for your convenience.

To import them, simply add the following line to your page (or immediately into the `_ViewImports.cshtml`):

```html
@addTagHelper *, KE.IdenticonSharp.AspNetCore
```

Now you can easily use the following helpers:

```html
@{
    string userEmail = ...;
    string userSecret = ...;
}

<!-- Generates <img> containing an identicon (by IdenticonManager.Default) -->
<identicon width="256px" height="256px" value="@userEmail">
    
<!-- Generates <svg> containing an identicon (by IdenticonManager.Default) -->
<identicon width="256px" height="256px" value="@userEmail" svg>
    
<!-- Generates <img> containing a gravatar -->
<gravatar width="256px" height="256px" value="@userEmail">

<!-- Generates <img> containing a QR code (by configured QRIdenticonProvider instance) -->
<qr width="256px" height="256px" value="@userEmail">

<!-- Generates <svg> containing a QR code (by configured QRIdenticonProvider instance) -->
<qr width="256px" height="256px" value="@userEmail" svg>

<!-- Generates <img> containing a QR code for otpauth (by configured QRIdenticonProvider instance) -->
<!-- (Can be used with Google Authenticator, for example) -->
<!-- https://github.com/google/google-authenticator/wiki/Key-Uri-Format -->
<!-- If encode-secret attribute was not specified, the secret will be encoded only if it contains non-Base32 characters -->
<!-- If encode-secret="true", the secret will be Base32-encoded -->
<!-- If encode-secret="false", the secret will not be Base32-encoded -->
<otp width="256px" height="256px" secret="@userSecret" issuer="My Cool Site" user="@userEmail">

<!-- Generates <svg> containing a QR code for otpauth (by configured QRIdenticonProvider instance) -->
<!-- (Can be used with Google Authenticator, for example) -->
<!-- https://github.com/google/google-authenticator/wiki/Key-Uri-Format -->
<!-- If encode-secret attribute was not specified, the secret will be encoded only if it contains non-Base32 characters -->
<!-- If encode-secret="true", the secret will be Base32-encoded -->
<!-- If encode-secret="false", the secret will not be Base32-encoded -->
<otp width="256px" height="256px" secret="@userSecret" issuer="My Cool Site" user="@userEmail" svg>
```

## Links

 - [`KE.IdenticonSharp` on NuGet][core-nuget]
 - [`KE.IdenticonSharp.AspNetCore` on NuGet][asp-nuget]


 [core-nuget]: https://www.nuget.org/packages/KE.IdenticonSharp/ 
 [asp-nuget]: https://www.nuget.org/packages/KE.IdenticonSharp.AspNetCore/

 [core-badge]: https://img.shields.io/nuget/v/KE.IdenticonSharp
 [asp-badge]: https://img.shields.io/nuget/v/KE.IdenticonSharp.AspNetCore