# Playwright OAuth2 Access Token Acquisition

This is a simple demo showing how you can use [Playwright](https://playwright.dev/) to automate the process of getting an OAuth2 access token from Azure Active Directory with user delegated permissions, for the purposes of performing automated testing.

*NOTE: This automation will only work if Multi Factor Authentication is not enabled for the user being used to authenticate during the testing process.*

To run this sample you will need to first create a user in your Azure Active Directory tenant for the test automation user, login with the new credentials, and set a password.

Then create an App Registration in Azure Active Directory setting the redirect uri to the same as defined in your code (this sample uses [https://oidcdebugger.com/debug](https://oidcdebugger.com)), and create a client secret for the App Registration, which is required for MSAL to later exchange the auth code for an access token. Then in the App Registration API Permission, select the user delegated permissions you require in the access token.

Once you have the user and App Registration setup, you should grant consent for scope required for the test automation user for the client application. You can do that via the App Registration blade in the portal, or by manually requesting it by crafting a login uri. This code sample assumes consent is already granted.

If using Visual Studio, right-click on the project in Solution Explorer and select Manage User Secrets. Copy the contents of the `appsettings.json` file and complete the values as per the values created above.

Before running you will need to install the Playwright browser dependencies. You can do this by first building the project and then running the PowerShell script copied into the project `bin` folder: `pwsh bin/Debug/netX/playwright.ps1 install`.

By default the sample will run in headerless mode, meaning you will not see the browser window. If you wish to see the automation, simply change line 43 of `Program.cs` to `Headerless = false`.
