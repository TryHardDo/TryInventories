# 📦 TryInventories
TryInventories is an ASP.NET Core based RESTApi that communicates with Steam's inventory endpoint through a proxy. It provides a simple yet efficient way to retrieve Steam inventory contents and bypass rate limits.

## ⚙️ Installation
- **Installing dependencies:** To run this program, you must first install the .NET 6.0 SDK. You can download the latest version from [HERE](https://download.visualstudio.microsoft.com/download/pr/56785524-dcd2-425a-8a95-3c2ee607b22f/e32ce2d12821f85c7d5e9cdee5ff5264/dotnet-sdk-6.0.411-win-x64.exe).
- **Download the latest release of the program:** After installing the SDK, download the latest release suitable for your operating system from [HERE](https://github.com/TryHardDo/TryInventories/releases).
- **Unzip the files into a directory of your choice.**

## 🗒️ appsettings.json
The `appsettings.json` file contains user settings located at the bottom:
```json
"Settings": {
    "ProxyHost": "http://something.com",
    "ProxyPort": 8080,
    "UseAuthorization": true,
    "ProxyAccess": {
      "Username": "username",
      "Password": "password"
    },
    "AccessKey": "place_some_api_key_here"
  }
```

Currently, only rotating proxy like `WebShare` is supported, but future updates will include a **proxy list feature** where you can load your proxies. Fill in your details for the host, port, etc. If you are using username and password authentication, set `"UseAuthorization"` to true; otherwise, it will ignore the credentials in the `"ProxyAccess"` object.

A simple authentication logic is in place to prevent misuse if the port is open to the world. The `"AccessKey"` is used to set the API key, which is required when sending a request to our endpoint.

For Windows users, you can start the program using the `TryInventories.exe` executable file. For Linux users, you should run the `TryInventories` file with command within the program's directory. It's highly recommended to use **Screen sessions** for this purpose.

To gracefully stop the app, press `CTRL + C` to exit.

## Note:
### The documentation is not finished and there are lot of missing parts. If you have specific question please write me on discord! Username: *tryharddo*
If you encounter any issue related to the software please open an **Issue** and describe the problem. I will try to fix it as soon as possible.
