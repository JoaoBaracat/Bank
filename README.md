# Bank

Web Api criada para receber transações financeiras e consultar seus status. A principal finalidade do projeto é controlar transações que serão posteriormente enviadas para uma Api de terceiro.

Para criação do banco de dados, executar scripts na pasta doc do projeto. Necessário instalar RabbitMQ (preferencialmente versão recente). Configurar no appsetting os dados de conexão do banco de dados e da fila.

Proojeto criado em C# .Net Core 3.1 com separação em camadas utilizando SOLID, patterns entre outras boas práticas utilizando banco de dados relacional SQL Server.

Tecnologias em uso no projeto: .Net Core, EF Core, SQL Server, Automapper, JWT, RabbitMQ, Serilog, FluentValidation, Identity entre outros.
