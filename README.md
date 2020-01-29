# Training: HttpClient
These applications contain some examples of the use of HttpClient and HttpClientFactory.
The best approach dependes upos the app's requirements.
## Basic Usage
IHttpClientFactory can be regfistered by calling AddHttpClient.
An IHttpClientFactory can be request using dependecy injection and the code uses IHttpClientFactory to create a HttpClient instance.
This is a good way to refactory an exisitng app. It has no impact on how HttpClient is used.

## Named client
Named client are a good choice when:
 
* The app requires many distinct uses of HttpClient.
* Many HttpClients have diferent configuration.

## Typed client
Typed clients:

* Provide the same capabilities as named clients without the need to use strings as keys.
* Provides IntelliSense and compiler help when consuming clients.
* Provide a single location to configure and interact with a particular HttpClient. For example, a single typed client might be used:
  * For a single backend endpoint.
  * To encapsulate all logic dealing with the endpoint.
* Work with DI and can be injected where required in the app.
