Daemon-DotNet
=============

In this sample a Windows console application calls a web API using it's app identity.  This scenario is useful for situations where headless or unattended job or process needs to run as an application identity, instead of as a user's identity.  The application uses the Active Directory Authentication Library (ADAL) to get a token from Azure AD using the OAuth 2.0 client credential flow, where the client credential is a password.
