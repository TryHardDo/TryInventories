# 📦 TryInventories
TryInventories is an ASP.NET Core based RESTApi that communicates with Steam's inventory endpoint through a proxy. It provides a simple yet efficient way to retrieve Steam inventory contents and bypass rate limits.
> The pogram only works with [WebShare](https://www.webshare.io/?referral_code=tet73fy7n7vb) proxies! Other proxy providers does not have supported yet!

You can find the autobot implementation of the api [here](https://github.com/TryHardDo/tf2autobot/tree/try-inventories). Here you can find the description how you can implement
the code into your code base even if you have customized parts in your code.

## ⚙️ Program Installation
Since 1.1.3 the software will run inside a Docker container. It is a much easier way to use and update and it can run on any operating system and distribution
that Docker supports.

So lets get to the point:
- **Installing Docker on your operating system**: You should install Docker first to be able to run Docker images. If you have screen access and you want to work
with a nice UI I would suggest installing Docker Desktop!
You can find the instruction here for docker installation: [Docker Desktop Installation Instructions](https://docs.docker.com/desktop/).
If Docker Desktop is not available for your operating system or you don't have screen access then Docker Engine is your choice: [Instruction Here](https://docs.docker.com/engine/install/)

- **Getting docker-compose.yml and appsettings.json**: Docker is ready now we should use it somehow. For the easiest way of running the software I am using
docker compose file which will do everything for you with a single command. appsettings.json is the settings file for the software. You can set it your API keys
and some other parameters. We need to download these files to start working with it. I will always push these files with the [Releases](https://github.com/TryHardDo/TryInventories/releases) so you can download it
from the latest release! So basically when you are reading this you have to have a folder which contains `appsettings.json` and `docker-compose.yml`!

- **Editing appsettings.json**: Open the file and fill it with your details!
The `appsettings.json` file contains user settings located at the bottom:
```json
"TryInventories": {
    "AcceptTermsOfUse": false,
    "ProxyMode": "Internal",
    "ShuffleProxyPool": true,
    "InternalRotationSettings": {
      "WebShareApiKey": "",
      "RotationThreshold": 20,
      "DoScheduledPoolSync": true,
      "PoolSyncInterval": 30
    },
    "ExternalRotationSettings": {
      "ProxyHost": "p.webshare.io",
      "ProxyPort": 80,
      "UseAuthorization": true,
      "Username": "",
      "Password": "",
      "MaxCallRetry": 10,
      "RetryDelay": 1000
    },
    "AccessToken": ""
  }
```

___When new features will be added to the settings you should manually implement them.___

- __ProxyMode__:
There are 2 separate modes available right now: `Internal` and `External`.
    - ___Internal___: The rotation timing and proxy picking from the proxy pool is managed by the software. This is the most efficient, fastest and most reliable mode
    that we can achive right now.
    - ___External___: It means whe proxy rotation will be handled by WebShare. As you know they have built in rotation mode which rotates the proxy automatically for
    you and only have to make calls to a constant URL. The program supports this mode if you would like to use that one rather than `Internal` mode.

- __ShuffleProxyPool__: This is basically a shuffle logic switch which means your pool retrieved from the WebShare API will be mixed to make sure you do not
use you proxies always in the same order to preven exhausted proxies. If you set it to `false` you can disable this feature but __I HIGHLY RECOMMEND TO NOT!__

- __InternalRotationSettings__:
Holds internal mode related settings. If __ProxyMode__ is set to `External` these settings will be ignored.
    - __WebShareApiKey__: The WebShare API key which is used to retrieve the proxy list from aour account. You can find your _WebShare API key(s)_ [here](https://proxy2.webshare.io/userapi/keys) or you can make a new one if there is no existing one.
    - __RotationThreshold__: The maximum rotate amount which is allowed per request. If the rotation count reache this number it returns the last response.
    - __DoScheduledPoolSync__: If it is set to true it is atomatically refreshes your pool in a set intervall!
    - __PoolSyncInterval__: The syncronization interval in minutes!

- __AutoRotatedProxySettings__: Holds the external related proxy settings. If __ProxyMode__ is set to `Internal` these settings will be ignored.
    - __ProxyHost__: The host of your rotating proxy. Default value: `p.webshare.io`.
    - __ProxyPort__: The port of your rotating proxy. Default value: `80`.
    - __UseAuthorization__: If you keep it `true` then the software will use authentication with the credentials you specify here. If you are using IP based authentication then set it to `false`.
    - __Username__: The username of your rotating proxy. It looks like a randomly generated string by default.
    - __Password__: The password of your rotating proxy.
    - __MaxCallRetry__: The maximum retry amount until the software retries sending the request. If all fails it returs the last response from Steam.
    - __RetryDelay__: The delay beetwen request retries in ___MILLISECONDS___. So if you want 3 seconds of delay then `3 * 1000`.

- __AccessToken__: This is like a simple API key to access to the endpoint by this software. It is for those users who are using it in non-localhost network enviroment.
This is an `optional` value. If you keep it empty `""` then it won't autheticate request. If you are running the software on `localhost` then you can keep it empty since nobody can access to the API in this case only if you set
up a reverse proxy by yourself but this is your fault then :D.

 Note that the `"AccessToken"` is used to set the API key, which is required when sending a request to our endpoint. So when we will configure the autobot side of the
 program this key will be used in your `ecosystem.json` to set the API key of the alternative inventory source!

 - **Starting the software with Docker Compose plugin**: Now we have a fully configured `appsettings.json` and it is now ready to be deployed! Let us start the program with the
 following command in the directory where `docker-compose.yml` and `appsettings.json` is located! Use `docker compose up -d` to start the image pulling and the app setup and wait
 until Docker finishes it's job! Please make sure to keep port `3000` as free because program will use this port!

## Note:
___The documentation is not finished and there are some missing parts. If you have specific question please write me on discord! Username: `tryharddo`___
If you encounter any issue related to the software please open an **Issue** and describe the problem. I will try to fix it as soon as possible.
