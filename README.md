# Web Scraping

Deliver an API that returns the total number of lines and the total number of bytes of all the files of a given public Github repository, grouped by file extension.
Data is retrieved using pure web scraping techniques, without Github's API or web scraping libraries.

## Play

``GET`` https://web-scraping-paulera.herokuapp.com/api/WebScraping?url=&api_key=

- ``url``: Public GitHub repository, e.g. `https://github.com/paulojsilva/web-scraping-nolayer`
- ``api_key``: If Authentication enabled, set the security key

## Tecnologies

Web application developed in ASP NET Core 3.1, C# 8.0 with:

- DDD (Domain Driven Design)
- Notification pattern
- Semaphore process synchronization
- Parallelism (ParallelForEach)
- MemoryCache (native)
- Redis Cache (StackExchange.Redis)
- HttpClient (native)
- AngleSharp (HTML parser)
- Unit tests with XUnit and FluentAssertions
- Docker

## Project

O projeto foi modelado com DDD (Domain Driven Design) para guiar a constru��o do projeto baseado no Dom�nio.
O dom�nio possui o conhecimento de como buscar as informa��es via Web Scraping no GitHub, lidar com requisi��es HTTP, paralelismo, etc.
Divis�o das camadas:

- Api: Exp�e na Web uma API que tem como entrada a URL a ser aplicada WebScraping. Se relaciona com a Application e delega a ela responsabilidade de executar a opera��o de WebScraping.
- Application: Camada controlodora respons�vel por receber pedidos de webscraping e delegar � camada superior - Domain. A Application n�o tem conhecimento de como aplicar WebScraping.
- Domain: Camada principal que possui conhecimento de como aplicar WebScraping.
- Domain.Shared: Concentra DTO - Data Transfer Objects compartilh�veis em toda aplica��o e objetos de configura��o do sistema - appsettings.json
- CrossCutting: Camada transversal as outras. Concentra extensions e exceptions utilizadas em todo projeto.
- CrossCutting.IoC: Limitado a Dependency Injection. A IoC aqui (Inversion of Control) foi usado no sentido de apenas injetarmos o servi�o no cliente, ao inv�s do pr�prio cliente procurar e construir o servi�o que ir� utilizar.
- Infra.Data:
	- Possui elementos relacionados a Dados. No nosso caso, ao servi�o de Cache. 
	- Mas poderia estabelecer reposit�rios (de Cache, NoSQL, Banco relacional), ou at� concentrar l�gica de obter dados HTML (o ponto central do WebScraping)
	- Reposit�rios de Dados de HTML n�o foram criadas aqui, porque a complexidade de se lidar com recursividade, paralelismo e performance � alta, logo segregar dados HTML em outra camada aumentaria mais ainda a complexidade.

## Domain in Details

Ao receber o request, o dom�nio instancia o HttpClient - respons�vel pelas requisi��es das p�ginas HTML do GitHub e Semaphore - respons�vel por controlar o acesso paralelo ao HttpClient.

O m�todo ProcessAsync � chamado pela 1� vez para buscar o HTML da 1� p�gina.
A lib AngleSharp � usada como HTML Parser (fant�stica por sinal!) e por ela fazemos perguntas do tipo:

- O HTML recebido representa uma p�gina do GitHub de listagem de diret�rios?
- Ou representa o conte�do (linhas) de um arquivo?

Com essa pergunta respondida, conseguimos determinar se:

- Chamamos o ProcessAsync recursivamente para carregar outra p�gina de listagem de diret�rios
- Ou se procuramos no DOM as informa��es de quantidade de linhas e tamanho (em bytes)

A medida que as p�ginas de conte�do de arquivo s�o encontradas, armazena-se na `ConcurrentBag temporaryFiles` os arquivos encontrados (nome, linhas e tamanho em byte).
Fazer esse processo recursivamente e 1 a 1 � lento com for�a. Ent�o, utilizamos ParallelForEach para dar um UP na recursividade.
Todavia, o GitHub controla o acesso de seus recursos com **Rate Limiting**, ent�o se consumirmos muitas p�ginas em pouco tempo, o GitHub bloqueia o acesso com 429 Too Many Requests.
Para contornar isso, usamos Semaphore, que limita a quantidade de tasks que utilizam um determinado recurso (no nosso caso, limita a quantidade de tasks que acessam o HttpClient).

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