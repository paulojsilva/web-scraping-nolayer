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

O projeto foi modelado com DDD (Domain Driven Design) para guiar a construção do projeto baseado no Domínio.
O domínio possui o conhecimento de como buscar as informações via Web Scraping no GitHub, lidar com requisições HTTP, paralelismo, etc.
Divisão das camadas:

- Api: Expõe na Web uma API que tem como entrada a URL a ser aplicada WebScraping. Se relaciona com a Application e delega a ela responsabilidade de executar a operação de WebScraping.
- Application: Camada controlodora responsável por receber pedidos de webscraping e delegar à camada superior - Domain. A Application não tem conhecimento de como aplicar WebScraping.
- Domain: Camada principal que possui conhecimento de como aplicar WebScraping.
- Domain.Shared: Concentra DTO - Data Transfer Objects compartilháveis em toda aplicação e objetos de configuração do sistema - appsettings.json
- CrossCutting: Camada transversal as outras. Concentra extensions e exceptions utilizadas em todo projeto.
- CrossCutting.IoC: Limitado a Dependency Injection. A IoC aqui (Inversion of Control) foi usado no sentido de apenas injetarmos o serviço no cliente, ao invés do próprio cliente procurar e construir o serviço que irá utilizar.
- Infra.Data:
	- Possui elementos relacionados a Dados. No nosso caso, ao serviço de Cache. 
	- Mas poderia estabelecer repositórios (de Cache, NoSQL, Banco relacional), ou até concentrar lógica de obter dados HTML (o ponto central do WebScraping)
	- Repositórios de Dados de HTML não foram criadas aqui, porque a complexidade de se lidar com recursividade, paralelismo e performance é alta, logo segregar dados HTML em outra camada aumentaria mais ainda a complexidade.

## Domain in Details

Ao receber o request, o domínio instancia o HttpClient - responsável pelas requisições das páginas HTML do GitHub e Semaphore - responsável por controlar o acesso paralelo ao HttpClient.

O método ProcessAsync é chamado pela 1° vez para buscar o HTML da 1° página.
A lib AngleSharp é usada como HTML Parser (fantástica por sinal!) e por ela fazemos perguntas do tipo:

- O HTML recebido representa uma página do GitHub de listagem de diretórios?
- Ou representa o conteúdo (linhas) de um arquivo?

Com essa pergunta respondida, conseguimos determinar se:

- Chamamos o ProcessAsync recursivamente para carregar outra página de listagem de diretórios
- Ou se procuramos no DOM as informações de quantidade de linhas e tamanho (em bytes)

A medida que as páginas de conteúdo de arquivo são encontradas, armazena-se na `ConcurrentBag temporaryFiles` os arquivos encontrados (nome, linhas e tamanho em byte).
Fazer esse processo recursivamente e 1 a 1 é lento com força. Então, utilizamos ParallelForEach para dar um UP na recursividade.
Todavia, o GitHub controla o acesso de seus recursos com **Rate Limiting**, então se consumirmos muitas páginas em pouco tempo, o GitHub bloqueia o acesso com 429 Too Many Requests.
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