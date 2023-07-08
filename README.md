# 📦 TryInventories
TryInventories is an ASP.NET Core based RESTApi that communicates with Steam's inventory endpoint through a proxy. It provides a simple yet efficient way to retrieve Steam inventory contents and bypass rate limits.
> The pogram only work with [WebShare](https://www.webshare.io/?referral_code=tet73fy7n7vb) proxies and it support two modes. Auto rotate mode where the proxy rotation is handled by WebShare and the second is the self rotated mode
where the software will handle the rotation. You can read about it later how you can set it up.

You can find the autobot implementation example of the api [here](https://github.com/TryHardDo/tf2autobot/tree/try-inventories).

## ⚙️ Program Installation
- **Installing dependencies:** To run this program, you must first install the .NET 6.0 SDK. You can find the installation documentation [here](https://learn.microsoft.com/en-us/dotnet/core/install/).
- **Download the latest release of the program:** After installing the SDK, download the latest release suitable for your operating system from [here](https://github.com/TryHardDo/TryInventories/releases).
- **Unzip the files into a directory of your choice.**

## 🗒️ appsettings.json
The `appsettings.json` file contains user settings located at the bottom:
```json
"Settings": {
    "SelfRotatedProxy": true,
    "SelfRotatedProxySettings": {
      "WebShareApiKey": "place_your_webshare_api_key_here",
      "MaxRotatePerRequest": 20
    },
    "AutoRotatedProxySettings": {
      "ProxyHost": "p.webshare.io",
      "ProxyPort": 80,
      "UseAuthorization": true,
      "AuthorizationCredentials": {
        "Username": "username",
        "Password": "passwd"
      },
      "MaxRetryPerRequest": 10,
      "RetryDelay": 1000
    },
    "AccessKey": "api_key_if_you_need"
  }
```

When new features will be added to the settings you should manually implement them.

- __SelfRotatedProxy__:
If you set it true the proxy rotation will be handled by the software.
Note that you have to set your API key in `"SelfRotatedProxySettings"`.`"WebShareApiKey"` to use this feature! You can read this settig below.

- __SelfRotatedProxySettings__:
Holds self rotated proxy related settings. If `SelfRotatedProxy` is set to false these settings will be ignored.
    - __WebShareApiKey__: The WebShare API key which is used to retrieve the proxy list from aour account. You can find your _WebShare API key(s)_ [here](https://proxy2.webshare.io/userapi/keys) or you can make a new one if there is no existing one.
    - __MaxRotatePerRequest__: The maximum rotate amount which is allowed per request. If the rotation count reache this number it returns the last response.

- __AutoRotatedProxySettings__: Holds the auto rotated proxy settings. If `SelfRotatedProxy` is set to true these settings will be ignored.
    - __ProxyHost__: The host of your rotating proxy. Default value: `p.webshare.io`.
    - __ProxyPort__: The port of your rotating proxy. Default value: `80`.
    - __UseAuthorization__: If you keep it `true` then the software will use authentication with the credentials you specified in this object. If you are using IP based authentication then set it to `false`.
    - __AuthorizationCredentials__: The authentication details which will be used if the `UseAuthorization` is set to `true`.
        - __Username__: The username of your rotating proxy. It looks like a randomly generated string by default.
        - __Password__: The password of your rotating proxy.
    - __MaxRetryPerRequest__: The maximum retry amount until the software retries sending the request. If all fails it returs the last response from Steam.
    - __RetryDelay__: The delay beetwen request retries in ___MILLISECONDS___. So if you want 3 seconds of delay then `3 * 1000`.

- __AccessKey__: A simple authentication logic is in place to prevent misuse if the port is open to the world. This is an `optional` value. If you keep it empty `""` then it won't autheticate request. If you are running the software on `localhost` then you can keep it empty since nobody can access to the API in this case.

 The `"AccessKey"` is used to set the API key, which is required when sending a request to our endpoint.

For Windows users, you can start the program using the `TryInventories.exe` executable file. For Linux users, you should run the `TryInventories` file with command within the program's directory. It's highly recommended to use **Screen sessions** for this purpose.

## Note:
___The documentation is not finished and there are some missing parts. If you have specific question please write me on discord! Username: `tryharddo`___
If you encounter any issue related to the software please open an **Issue** and describe the problem. I will try to fix it as soon as possible.
