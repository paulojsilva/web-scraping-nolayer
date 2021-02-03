# Web Scraping

Deliver an API that returns the total number of lines and the total number of bytes of all the files of a given public Github repository, grouped by file extension.
Data is retrieved using pure web scraping techniques, without Github's API or web scraping libraries.

## Play

``GET`` https://web-scraping-paulera.herokuapp.com/api/WebScraping?url=&api_key=

- ``url``: Public GitHub repository, e.g. `https://github.com/paulojsilva/web-scraping-nolayer`
- ``api_key``: If Authentication enabled, set the security key

## Tecnologies

Web application developed in ASP NET Core 3.1, C# 8.0 with:

- HttpClient (native)
- AngleSharp (HTML parser)
- MemoryCache
- StackExchange.Redis (Redis Cache)
- Semaphore process synchronization
- ParallelForEach
- Unit tests with XUnit and FluentAssertions
- Docker

## Heroku Deploy

- Create a [Hekoru account](https://www.heroku.com/)
- Create new app, like **web-scraping-paulera** =D
- Download and install [Heroku CLI](https://devcenter.heroku.com/articles/heroku-command-line)
- Configure ``Dockerfile`` (need to be in the same folder as .csproj WebApi and .sln files)
- Navigate to Dockerfile folder and run:
- Docker build image: ``docker build --rm -f "Dockerfile" -t "web-scraping-paulera:latest" .``
- Login to Heroku (will open the browser): ``heroku login``
- Sign into Container Registry: ``heroku container:login``
- Push docker image: ``heroku container:push web -a web-scraping-paulera``
- Release the newly pushed images to deploy your app: ``heroku container:release web -a web-scraping-paulera``
- Its done! If you need check logs/errors/warnings, run: ``heroku logs --tail -a web-scraping-paulera``
- My deployed app is: https://web-scraping-paulera.herokuapp.com/

## Docker Hub

- Create a [Docker ID (account)](https://hub.docker.com/)
- Create a repository
- Navigate to Dockerfile folder and run:
- build image: ``docker build --rm -f "Dockerfile" -t "YourDockerIdHere/YourDockerRepositoryHere:latest" .``
- login (enter your ID and password): ``docker login``
- push: ``docker push YourDockerIdHere/YourDockerRepositoryHere:latest``
- My Docker Hub: https://hub.docker.com/r/paulojustinosilvadocker/web-scraping