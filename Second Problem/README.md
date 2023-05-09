# Problem

You're building a massively popular online game for mobile phones with a number of multiplayer modes. You're in charge of designing a service that will
help players choose which game mode to play.

This service allows players to perform two operations:

1. The game reports the user's region and selected game mode.
2. The game queries the *current* most popular game modes for the region in which the player is located.

> Region is a string that contains an ISO-3166 country code. For example CA and US.

Please design a REST service to support those operations. It should scale to millions of concurrent users. Your design should include:

- REST API specification (endpoints, input parameters, output).
- Service layer design.
- Storage layer design (please explain data access patterns).
- A discussion of how your design will scale up to millions of CCUs.

# Solution

## Table of Contents

- [Problem](#problem)
- [Solution](#solution)
  - [Table of Contents](#table-of-contents)
  - [1. Requirements](#1-requirements)
  - [2. REST API](#2-rest-api)
    - [Report player's current region and gamemode.](#report-players-current-region-and-gamemode)
      - [Parameters](#parameters)
      - [Responses](#responses)
      - [Example Request/Response](#example-requestresponse)
    - [Request top gamemodes of a region](#request-top-gamemodes-of-a-region)
      - [Parameters](#parameters-1)
      - [Responses](#responses-1)
      - [Example Request/Response](#example-requestresponse-1)
  - [3. Application Design](#3-application-design)
    - [1. Service Layer](#1-service-layer)
      - [Model class](#model-class)
    - [2. Business Logic Layer](#2-business-logic-layer)
    - [3. Data Access Layer](#3-data-access-layer)
    - [Some Design Principles of Note](#some-design-principles-of-note)
  - [4. Physical Data Storage](#4-physical-data-storage)
    - [Enter Redis](#enter-redis)
    - [Data Model](#data-model)
    - [Data Access Patterns](#data-access-patterns)
  - [5. Scalability Sonsiderations](#5-scalability-sonsiderations)
  - [6. Limitations](#6-limitations)


## 1. Requirements

* ### Functional Requirements
  * The service stores the player's current region and gamemode.
  * The service returns the list of top gamemodes for each region.

* ### Non Functional Requirements
  * The service has to scale up to 1 million concurrent users.

## 2. REST API

### Report player's current region and gamemode.

> Please expand each section

<details>
    <summary><code>POST</code> <code><b>/v1/profiles/{profileId}/gamemode</b></code> <code>(report player's region and gamemode)</code></summary>

#### Parameters

> | name      |  rqeuired | data type    | location  | description                                                           |
> |-----------|-----------|--------------|-----------|-----------------------------------------------------------------------|
> | profileId |  required | string       | path      | the unique identifier of the user reported by the game in UUID format |
> | region    |  required | string       | body      | the user's country in ISO-3166 format. e.g. CA                        |


#### Responses

> | http code     | content-type                      | response                                                            |
> |---------------|-----------------------------------|---------------------------------------------------------------------|
> | `200`         | `application/json;charset=UTF-8`        | `gamemode and region saved successfully`                      |
> | `400`         | `application/json:charset=UTF-8`        | `{"code":"400","message":"Bad Request"}`                      |

#### Example Request/Response

> ```javascript
>  POST /v1/profiles/934ba404-f883-4c76-b648-212c616c3735 HTTP/1.1
>  Host: api.samplegame.com
>  Content-Type: application/json
>  Accept: application/json
>  Accept-Charset: utf-8
>{
>	"region" : "CA"
>}
> ```

If successful, you will receive a response like that:

> ```javascript
> HTTP/1.1 200 OK
> Content-Type: application/json; charset=utf-8
> ```

</details>


------------------------------------------------------------------------------------------

### Request top gamemodes of a region

<details>
    <summary><code>GET</code> <code><b>/v1/regions/{region}/gamemode</b></code> <code>(get the top 5 popular gamemodes for the region)</code></summary>


#### Parameters

> | name      |  rqeuired | data type    | location  | description                                                           |
> |-----------|-----------|--------------|-----------|-----------------------------------------------------------------------|
> | region    |  required | string       | path      | the country code in ISO-3166 format. e.g. CA                          |
>

#### Responses

> | http code     | content-type                      | response                                                            |
> |---------------|-----------------------------------|---------------------------------------------------------------------|
> | `200`         | `application/json;charset=UTF-8`  | `check the below section`                                           |
> | `400`         | `application/json;charset=UTF-8`  | `{"code":"400","message":"Bad Request"}`                            |

#### Example Request/Response

> ```javascript
>  GET /v1/regions/CA/gamemode HTTP/1.1
>  Host: api.samplegame.com
>  Content-Type: application/json
>  Accept: application/json
>  Accept-Charset: utf-8
> ```

If successful, you will receive a response like that:

> ```javascript
> HTTP/1.1 200 OK
> Content-Type: application/json;charset=utf-8
> {
>    "gamemodes" : [
>       {
>           "mode":"GM01", 
>           "rank": 1
>       },
>       {
>           "mode":"GM02", 
>           "rank": 2
>       },
>       {
>           "mode":"GM03", 
>           "rank": 3
>       },
>       {
>           "mode":"GM04", 
>           "rank": 4
>       },
>       {
>           "mode":"GM05", 
>           "rank": 5
>       },
>   ]
> }
> ```

</details>

## 3. Application Design

The overall design of the application uses a 3 layer architecture. 
* On the outermost layer, we have what is usually defined as "service layer" because it's the one layer that "serves" the callers which in this case is the incoming HTTP requests.
* The second layer that is used by the first layer is our business logic layer.
* The innermost layer is the data access layer that interacts with our physical data storage.

### 1. Service Layer

This is the layer that interacts with the incoming HTTP requests and sends back the data. This layer is reponsible for a few things:
* Deserializing the HTTP JSON payload to our model classes.
* Validating the request for immediate errors (e.g. sending an empty profileId)
* Sending the proper validation errors and HTTP status codes in case of validation failure
* Calling the business logic layer and passing along the model objects.
* Receiving the response back from the business logic layer and serializing it to JSON to be sent back over HTTP to the client.
* Application level kill switching
* Starting a trace or perpetuating an existing trace that might have started in the game ( one of the pillars of observability. The other two being logs and metrics). Depending on the platform, this step might not be manual.
* Authentication and authorization if necessary
* Mapping the application level exceptions to error codes and http status codes.

#### Model class

Our model class maps the incoming HTTP payload to the business logic objects. In our case it has three simple properties: `ProfileId`, `Gamemode` and `Region`

### 2. Business Logic Layer

This is the layer that is not concerned with external interactions, like the previous layer, rather it focuses on the main logic of our application. It is responsible for:

* Application level validations, for example sending a Gamemode that doesn't exist, or is gated behind a season pass
* Logging and sending the required metrics for observability
* Interacting with the data layer to persist and retrieve data
* There are some key moments in the game that the application logic should be informed of and respond to. Two in particular are crucial 1) player connection 2) player disconnection

Our business logic class has two main methods for the two main use cases of this problem.

1. `SaveProfileGameMode(Model model)` This method receives the player's current gamemode and region from the game and saves it through the DAL. But it also has to do some validations. For example it might be the case that transition between a few gamemodes are unauthorized for some players. For example if you're not the holder of a season pass, you cannot play the GM10 gamemode.
   
2. `GetPopularGameModes(string region): [] string` This method receives a region and should return the *current* most popular gamemodes for that region. For the response we can use a model object to specify the rank for each gamemode as well.

### 3. Data Access Layer

This is the layer that interacts directly with our physical data storage. In the next section we will discuss our choice of data storage. This layer will use the SDK from the database vendor to interact with the physical storage. It is reponsible to open a connection, maintain the connection for the lifetime of the application

Our DAL has two main methods to support our business logic operations:

1. `CreateProfileGamemode(Model model)` This method writes directly to the storage. It saves `profileId`, `region` and the current `gamemode`. Because of the distributed nature of our application, this method has a subtle nuance that has to do with concurrency that we'll discuss in the next section.
2. `GetGameModes(string region): [] Gamemodes` This method looks up our storage and returns the top N gamemodes for each region. We'll discuss in the next section how this is achieved using our storage and data models.


![Class Diagram](class_diagram.jpg "class diagram")

### Some Design Principles of Note

1. Using interfaces: This kinda adheres to the O of SOLID design principles. We try to have *loose couplings* between different layers. The logic layer references the data layer, but should not depend on the concrete implementation of the DAL. If we decide to change the implmentation of the physical storage and consequently our DALs, this should not affect the other layers whatsoever. Moreover this makes it much easier to write tests, because we can easily mock our DALs using mocking frameworks.
2. Dependency Injection: It is ideal to inject dependencies (from constructors) using a dependency injection container which also manages the lifetime of our classes.

## 4. Physical Data Storage

One of the functional requirements of our application formulates that we are only interested in the current state of our players. We don't care about the past game sessions and gamemodes, we don't care about the disconnected players, but we do care about the ones that are playing. So our data is transient. That is the key to our choice of data storage.

### Enter Redis

Redis is an open-source, in-memory data structure store that is often used as a high-performance database, cache, and message broker. It is known for its speed, versatility, and ease of use.

One of Redis' key features is its ability to store and manipulate complex data structures, including strings, hashes, lists, sets, and *sorted sets*. It has a wide range of commands and operations for manipulating these data structures, making it a powerful tool for data analysis, message processing, and real-time applications.

Redis is particularly well-suited for transient data storage because it stores data in memory, which allows for very fast read and write operations. However, Redis also provides options for persisting data to disk, making it possible to use Redis for long-term data storage as well.

I have former experience with Redis building a datastore for matchmaking. In my experience the sortedset data structure is a perfect candidate to model our data.

### Data Model

We have two data structures for two different use cases. Hash to store profile data and sorted sets to store gamemode rankings per region.

1. **Hash** for the profile data
   Redis hashes are record types structured as collections of field-value pairs. You can use hashes to represent basic objects and to store groupings of counters, among other things. We store our profile id as the key and two fields of region and gamemode.

``` javascript
    > HSET profiles:934ba404-f883-4c76-b648-212c616c3735 gamemode GM01
    (integer) 1
    > HSET profiles:934ba404-f883-4c76-b648-212c616c3735 region CA
    (integer) 1
    > HSET profiles:7314c2e3-e87f-4aaf-aef6-5cc372fe3bd8 gamemode GM01
    (integer) 1
    > HSET profiles:7314c2e3-e87f-4aaf-aef6-5cc372fe3bd8 region US
    (integer) 1
```

2. **Sorted Sets** for the gamemode rankings per region.
   A Redis sorted set is a collection of unique strings (members) ordered by an associated score. For example, you can use sorted sets to easily maintain ordered lists of the highest scores in a massive online game. For our case we store region as the key and gamemode as the value and increment the score everytime a player in that region enters that gamemode. This allows us to easily get the top N records for that region. It's blazing fast with a O(log(n)) time complexity for the two operations we will use them for.

``` javascript
    > ZINCRBY regions:CA 1 GM01
    "1"
    > ZINCRBY regions:FR 1 GM01
    "1"
    > ZINCRBY regions:US 2 GM02
    "2"
```

    And if we want to get the top 3 ranks in the CA region:


``` javascript
    > ZREVRANGE regions:CA 0 2 WITHSCORES
    1) "GM03"
    2) "11"
    3) "GM02"
    4) "10"
    5) "GM01"
    6) "4"
    7) "GM04"
    8) "0"
```

### Data Access Patterns

Our physical data storage must support the two operations defined in our application data access layer. In this section we discuss how we can leverage Redis to support these two operations.

* `CreateProfileGamemode(Model model)`
  This method at its core does a few operations
  1. Remove the previous gamemode from the user's data.
  2. Add the new gamemode to the user's data, in this case if the hashfield doesn't exist, add it or if it does overwrite it with the new gamemode value.
  3. Increment the new gamemode ranking for the user's region.
  4. Decrement the previous gamemode ranking for the user's' region.
   
   There are a few considerations here.
   * All these operations should run together atomically. This is an ACID requirement. Redis has support for transactions using a few commands which guarantees two imporant things.
     * All commands in a transaction are serialized and executed sequentially. A request sent by another client will never be served in the middle of the execution of a Redis Transaction.
     * If the commit step of the transaction is not executed, or if the server calling the commit command loses the connection before that step (ah, yes we have multiple servers and we shall explain that in the Scalability section), none of these commands are executed.
  
  ``` javascript
      > MULTI // this is the comand that signals the start of a transaction
      > OK // response from Redis
      > HSET profiles:934ba404-f883-4c76-b648-212c616c3735 gamemode GM02 region US // this command writes the new values to the user's fields
      > QUEUED // response from Redis
      > ZINCRBY regions:CA -1 GM01
      > QUEUED // response from Redis
      > ZINCRBY regions:US  1 GM02
      > QUEUED // response from Redis
      > EXEC
      > 1) "2"
        2) "3"
        3) "3"
  ```
   * Another consideration is that we want this command to be executed only once in Isolation. Another ACID requirement. What do we mean by that? Well, because of the distributed of our web servers which is the response to 1 million concurrent users and scalability needs, we have multiple web servers running the application and it might be the case that the request to run this transaction to be sent by multiple webservers simultaneously. This situation we should protect our data against. Redis offers an optimistic locking mechanism that allows you to watch one or more keys in order to detect changes agains them. If at least one watched key is modified before the EXEC command, the whole transaction aborts, and EXEC returns a Null reply to notify that the transaction failed. We use the `WATCH` command to monitor a key.

  ``` javascript
      > WATCH profiles:934ba404-f883-4c76-b648-212c616c3735 regions:CA regions:US
      > MULTI
      > OK
      > HSET profiles:934ba404-f883-4c76-b648-212c616c3735 gamemode GM02 region US
      > QUEUED
      > ZINCRBY regions:CA -1 GM01
      > QUEUED
      > ZINCRBY regions:US  1 GM02
      > QUEUED
      > EXEC
      > 1) "2"
        1) "3"
        2) "3"
  ```
      Using the above code, if there are race conditions and another client modifies the profile in the time between our call to WATCH and our call to EXEC, the transaction will fail.

* `GetGameModes(string region): [] Gamemodes`: This is a readonly method and is easily supported as a consequence of our choice of data models. We just probe our sorted set for the top N records. Because it's already sorted, this is blazing fast with a O(log(n)) complexity as stated above.

``` javascript
    > ZREVRANGE regions:CA 0 2 WITHSCORES
    1) "GM03"
    2) "11"
    3) "GM02"
    4) "10"
    5) "GM01"
    6) "4"
```

## 5. Scalability Sonsiderations

## 6. Limitations