/* ==========================================================================
   LaVaiPizza - Script completo do banco de dados (SQL Server / T-SQL)
   PIM IV - Etapa 7 (Projeto do Banco de Dados)

   Convencoes (aulas de Programacao de Banco de Dados):
   - Tabelas em PascalCase e no singular; colunas em PascalCase.
   - IDENTITY(1,1), DECIMAL(10,2) para dinheiro, DATETIME2 para datas.
   - CREATE OR ALTER para functions, views, procedures e triggers.
   - Prefixos trg_ (triggers) e IDX_ (indices). Sem cursores.
   - Atributos atomicos (endereco do cliente em colunas separadas - 1FN).

   Como executar (reexecutavel: apaga e recria o banco LaVaiPizzaDb):
     sqlcmd -S localhost\SQLEXPRESS -E -C -I -b -f 65001 -i script_banco.sql
   ou abra no SSMS e execute (F5).

   Senhas do seed (guardadas como hash do PasswordHasher do ASP.NET Core):
     admin@lavai.com       (Gerente)     Admin@123
     pizzaiolo1@lavai.com  (Pizzaiolo)   Pizza@123
     pizzaiolo2@lavai.com  (Pizzaiolo)   Pizza@123
     motoboy1@lavai.com    (Entregador)  Entrega@123
     motoboy2@lavai.com    (Entregador)  Entrega@123

   Dados de demonstracao: ao final do script e executada a procedure
   GerarDadosDemonstracao (pedidos com codigo DEMO-xxxxxxx). Para limpar:
     EXEC GerarDadosDemonstracao @Meses = 0;
   ========================================================================== */

USE master;
GO

IF DB_ID(N'LaVaiPizzaDb') IS NOT NULL
BEGIN
    ALTER DATABASE LaVaiPizzaDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE LaVaiPizzaDb;
END
GO

CREATE DATABASE LaVaiPizzaDb;
GO

USE LaVaiPizzaDb;
GO

/* ==========================================================================
   1. TABELAS
   ========================================================================== */

-- Credenciais de acesso (somente funcionarios fazem login)
CREATE TABLE Login (
    Id        INT IDENTITY(1,1) NOT NULL,
    Email     NVARCHAR(100)     NOT NULL,
    SenhaHash NVARCHAR(255)     NOT NULL,
    Nome      NVARCHAR(100)     NOT NULL,
    CONSTRAINT PK_Login PRIMARY KEY (Id),
    CONSTRAINT UQ_Login_Email UNIQUE (Email)
);
GO

CREATE TABLE Funcionario (
    Id       INT IDENTITY(1,1) NOT NULL,
    Nome     NVARCHAR(150)     NOT NULL,
    Cargo    NVARCHAR(20)      NOT NULL,
    Telefone NVARCHAR(20)      NULL,
    LoginId  INT               NOT NULL,
    CONSTRAINT PK_Funcionario PRIMARY KEY (Id),
    CONSTRAINT UQ_Funcionario_LoginId UNIQUE (LoginId),
    CONSTRAINT FK_Funcionario_Login FOREIGN KEY (LoginId) REFERENCES Login (Id),
    CONSTRAINT CK_Funcionario_Cargo CHECK (Cargo IN (N'Gerente', N'Pizzaiolo', N'Entregador'))
);
GO

-- Cliente nao tem login: informa os dados ao fazer o pedido
CREATE TABLE Cliente (
    Id          INT IDENTITY(1,1) NOT NULL,
    Nome        NVARCHAR(150)     NOT NULL,
    Telefone    NVARCHAR(20)      NOT NULL,
    Email       NVARCHAR(100)     NULL,
    Logradouro  NVARCHAR(150)     NOT NULL,
    Numero      NVARCHAR(10)      NOT NULL,
    Complemento NVARCHAR(60)      NULL,
    Bairro      NVARCHAR(80)      NOT NULL,
    Cidade      NVARCHAR(80)      NOT NULL CONSTRAINT DF_Cliente_Cidade DEFAULT N'São Paulo',
    Cep         NVARCHAR(9)       NOT NULL,
    CONSTRAINT PK_Cliente PRIMARY KEY (Id)
);
GO

-- Pizza nunca e apagada: e desativada (Ativa = 0) para preservar o historico
CREATE TABLE Pizza (
    Id        INT IDENTITY(1,1) NOT NULL,
    Nome      NVARCHAR(100)     NOT NULL,
    Descricao NVARCHAR(300)     NOT NULL CONSTRAINT DF_Pizza_Descricao DEFAULT N'',
    ImagemUrl NVARCHAR(300)     NULL,
    Ativa     BIT               NOT NULL CONSTRAINT DF_Pizza_Ativa DEFAULT 1,
    CONSTRAINT PK_Pizza PRIMARY KEY (Id),
    CONSTRAINT UQ_Pizza_Nome UNIQUE (Nome)
);
GO

-- Uma pizza tem um preco por tamanho (chave composta)
CREATE TABLE PizzaPreco (
    PizzaId                   INT           NOT NULL,
    Tamanho                   NVARCHAR(10)  NOT NULL,
    Preco                     DECIMAL(10,2) NOT NULL,
    AtualizadoEm              DATETIME2     NOT NULL CONSTRAINT DF_PizzaPreco_AtualizadoEm DEFAULT SYSDATETIME(),
    AtualizadoPorFuncionarioId INT          NULL,
    CONSTRAINT PK_PizzaPreco PRIMARY KEY (PizzaId, Tamanho),
    CONSTRAINT FK_PizzaPreco_Pizza FOREIGN KEY (PizzaId) REFERENCES Pizza (Id),
    CONSTRAINT FK_PizzaPreco_Funcionario FOREIGN KEY (AtualizadoPorFuncionarioId) REFERENCES Funcionario (Id),
    CONSTRAINT CK_PizzaPreco_Tamanho CHECK (Tamanho IN (N'Pequena', N'Media', N'Grande')),
    CONSTRAINT CK_PizzaPreco_Preco CHECK (Preco > 0)
);
GO

-- Tipo PERCENTUAL: Valor = % de desconto; VALOR_FIXO: Valor = R$ de desconto;
-- FRETE_GRATIS: zera a taxa de entrega. PizzaId NULL = vale para o pedido todo.
-- ValorMinimo = subtotal minimo do pedido para a promocao valer (opcional).
CREATE TABLE Promocao (
    Id          INT IDENTITY(1,1) NOT NULL,
    Nome        NVARCHAR(100)     NOT NULL,
    Tipo        NVARCHAR(15)      NOT NULL,
    Valor       DECIMAL(10,2)     NOT NULL CONSTRAINT DF_Promocao_Valor DEFAULT 0,
    ValorMinimo DECIMAL(10,2)     NULL,
    Inicio      DATETIME2         NOT NULL,
    Fim         DATETIME2         NOT NULL,
    Ativa       BIT               NOT NULL CONSTRAINT DF_Promocao_Ativa DEFAULT 1,
    PizzaId     INT               NULL,
    CONSTRAINT PK_Promocao PRIMARY KEY (Id),
    CONSTRAINT FK_Promocao_Pizza FOREIGN KEY (PizzaId) REFERENCES Pizza (Id),
    CONSTRAINT CK_Promocao_Tipo CHECK (Tipo IN (N'PERCENTUAL', N'VALOR_FIXO', N'FRETE_GRATIS')),
    CONSTRAINT CK_Promocao_Valor CHECK (Valor >= 0),
    CONSTRAINT CK_Promocao_Periodo CHECK (Fim >= Inicio)
);
GO

CREATE TABLE Pedido (
    Id                    INT IDENTITY(1,1) NOT NULL,
    CodigoAcompanhamento  NVARCHAR(12)      NOT NULL,
    Subtotal              DECIMAL(10,2)     NOT NULL,
    Desconto              DECIMAL(10,2)     NOT NULL CONSTRAINT DF_Pedido_Desconto DEFAULT 0,
    TaxaEntrega           DECIMAL(10,2)     NOT NULL CONSTRAINT DF_Pedido_TaxaEntrega DEFAULT 0,
    Total                 DECIMAL(10,2)     NOT NULL,
    Status                NVARCHAR(30)      NOT NULL CONSTRAINT DF_Pedido_Status DEFAULT N'Em Preparação',
    DataHora              DATETIME2         NOT NULL CONSTRAINT DF_Pedido_DataHora DEFAULT SYSDATETIME(),
    TempoEstimado         INT               NULL,
    ClienteId             INT               NOT NULL,
    FuncionarioPreparaId  INT               NULL,
    FuncionarioEntregaId  INT               NULL,
    CONSTRAINT PK_Pedido PRIMARY KEY (Id),
    CONSTRAINT UQ_Pedido_Codigo UNIQUE (CodigoAcompanhamento),
    CONSTRAINT FK_Pedido_Cliente FOREIGN KEY (ClienteId) REFERENCES Cliente (Id),
    CONSTRAINT FK_Pedido_FuncionarioPrepara FOREIGN KEY (FuncionarioPreparaId) REFERENCES Funcionario (Id),
    CONSTRAINT FK_Pedido_FuncionarioEntrega FOREIGN KEY (FuncionarioEntregaId) REFERENCES Funcionario (Id),
    CONSTRAINT CK_Pedido_Status CHECK (Status IN (N'Em Preparação', N'Aguardando Coleta', N'Saiu para Entrega', N'Entregue')),
    CONSTRAINT CK_Pedido_Valores CHECK (Subtotal >= 0 AND Desconto >= 0 AND TaxaEntrega >= 0 AND Total >= 0)
);
GO

-- Tabela associativa entre Pedido e Pizza. PrecoUnitario guarda o preco da
-- venda: mudar o preco da pizza depois nao altera o faturamento passado.
CREATE TABLE ItemPedido (
    PedidoId      INT           NOT NULL,
    PizzaId       INT           NOT NULL,
    Tamanho       NVARCHAR(10)  NOT NULL,
    Quantidade    INT           NOT NULL CONSTRAINT DF_ItemPedido_Quantidade DEFAULT 1,
    PrecoUnitario DECIMAL(10,2) NOT NULL,
    CONSTRAINT PK_ItemPedido PRIMARY KEY (PedidoId, PizzaId, Tamanho),
    CONSTRAINT FK_ItemPedido_Pedido FOREIGN KEY (PedidoId) REFERENCES Pedido (Id),
    CONSTRAINT FK_ItemPedido_PizzaPreco FOREIGN KEY (PizzaId, Tamanho) REFERENCES PizzaPreco (PizzaId, Tamanho),
    CONSTRAINT CK_ItemPedido_Quantidade CHECK (Quantidade > 0),
    CONSTRAINT CK_ItemPedido_Preco CHECK (PrecoUnitario > 0)
);
GO

-- Preenchida pelo trigger trg_AuditoriaPreco
CREATE TABLE HistoricoPreco (
    Id                       INT IDENTITY(1,1) NOT NULL,
    PizzaId                  INT           NOT NULL,
    Tamanho                  NVARCHAR(10)  NOT NULL,
    PrecoAntigo              DECIMAL(10,2) NOT NULL,
    PrecoNovo                DECIMAL(10,2) NOT NULL,
    AlteradoEm               DATETIME2     NOT NULL CONSTRAINT DF_HistoricoPreco_AlteradoEm DEFAULT SYSDATETIME(),
    AlteradoPorFuncionarioId INT           NULL,
    CONSTRAINT PK_HistoricoPreco PRIMARY KEY (Id),
    CONSTRAINT FK_HistoricoPreco_PizzaPreco FOREIGN KEY (PizzaId, Tamanho) REFERENCES PizzaPreco (PizzaId, Tamanho),
    CONSTRAINT FK_HistoricoPreco_Funcionario FOREIGN KEY (AlteradoPorFuncionarioId) REFERENCES Funcionario (Id)
);
GO

-- Preenchida pelo trigger trg_HistoricoStatusPedido (linha do tempo do pedido)
CREATE TABLE PedidoStatusHistorico (
    Id            INT IDENTITY(1,1) NOT NULL,
    PedidoId      INT          NOT NULL,
    Status        NVARCHAR(30) NOT NULL,
    AlteradoEm    DATETIME2    NOT NULL CONSTRAINT DF_PedidoStatusHistorico_AlteradoEm DEFAULT SYSDATETIME(),
    FuncionarioId INT          NULL,
    CONSTRAINT PK_PedidoStatusHistorico PRIMARY KEY (Id),
    CONSTRAINT FK_PedidoStatusHistorico_Pedido FOREIGN KEY (PedidoId) REFERENCES Pedido (Id),
    CONSTRAINT FK_PedidoStatusHistorico_Funcionario FOREIGN KEY (FuncionarioId) REFERENCES Funcionario (Id)
);
GO

/* ==========================================================================
   2. INDICES (prefixo IDX_)
   ========================================================================== */

CREATE INDEX IDX_Pedido_DataHora ON Pedido (DataHora);
GO
CREATE INDEX IDX_Pedido_Status ON Pedido (Status);
GO
CREATE INDEX IDX_Pedido_ClienteId ON Pedido (ClienteId);
GO
CREATE INDEX IDX_ItemPedido_PizzaId ON ItemPedido (PizzaId);
GO
CREATE INDEX IDX_PedidoStatusHistorico_PedidoId ON PedidoStatusHistorico (PedidoId);
GO
CREATE INDEX IDX_HistoricoPreco_PizzaId ON HistoricoPreco (PizzaId);
GO

/* ==========================================================================
   3. FUNCTIONS
   ========================================================================== */

-- Function escalar: calcula o desconto de uma promocao sobre um subtotal
CREATE OR ALTER FUNCTION dbo.CalcularDesconto
(
    @Subtotal DECIMAL(10,2),
    @Tipo     NVARCHAR(15),
    @Valor    DECIMAL(10,2)
)
RETURNS DECIMAL(10,2)
AS
BEGIN
    DECLARE @Desconto DECIMAL(10,2) = 0;

    IF @Tipo = N'PERCENTUAL'
        SET @Desconto = ROUND(@Subtotal * @Valor / 100, 2);
    ELSE IF @Tipo = N'VALOR_FIXO'
        SET @Desconto = @Valor;

    -- o desconto nunca passa do subtotal
    IF @Desconto > @Subtotal SET @Desconto = @Subtotal;
    IF @Desconto < 0 SET @Desconto = 0;

    RETURN @Desconto;
END
GO

-- Function de tabela (inline): promocoes vigentes em um instante
CREATE OR ALTER FUNCTION dbo.BuscarPromocoesVigentes (@Agora DATETIME2)
RETURNS TABLE
AS
RETURN
(
    SELECT Id, Nome, Tipo, Valor, ValorMinimo, PizzaId, Inicio, Fim
    FROM Promocao
    WHERE Ativa = 1
      AND @Agora BETWEEN Inicio AND Fim
);
GO

/* ==========================================================================
   4. VIEW
   ========================================================================== */

CREATE OR ALTER VIEW RelatorioVendasPorPizza AS
SELECT p.Id AS PizzaId,
       p.Nome AS Pizza,
       ip.Tamanho,
       SUM(ip.Quantidade) AS QuantidadeVendida,
       SUM(ip.Quantidade * ip.PrecoUnitario) AS Faturamento
FROM ItemPedido ip
JOIN Pizza p ON p.Id = ip.PizzaId
GROUP BY p.Id, p.Nome, ip.Tamanho;
GO

/* ==========================================================================
   5. TRIGGERS (prefixo trg_)
   ========================================================================== */

-- INSTEAD OF: "seguranca do banco". Bloqueia o DELETE de pizza.
CREATE OR ALTER TRIGGER trg_ProtegerPizzas
ON Pizza
INSTEAD OF DELETE
AS
BEGIN
    THROW 50001, N'Pizzas não podem ser apagadas, pois fazem parte do histórico de pedidos. Desative a pizza (Ativa = 0).', 1;
END
GO

-- AFTER UPDATE: "espiao" de auditoria. Registra toda mudanca de preco.
CREATE OR ALTER TRIGGER trg_AuditoriaPreco
ON PizzaPreco
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO HistoricoPreco (PizzaId, Tamanho, PrecoAntigo, PrecoNovo, AlteradoPorFuncionarioId)
    SELECT i.PizzaId, i.Tamanho, d.Preco, i.Preco, i.AtualizadoPorFuncionarioId
    FROM inserted i
    JOIN deleted d ON d.PizzaId = i.PizzaId AND d.Tamanho = i.Tamanho
    WHERE i.Preco <> d.Preco;
END
GO

-- AFTER INSERT, UPDATE: linha do tempo do pedido (quem e quando mudou o status)
CREATE OR ALTER TRIGGER trg_HistoricoStatusPedido
ON Pedido
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO PedidoStatusHistorico (PedidoId, Status, FuncionarioId)
    SELECT i.Id,
           i.Status,
           CASE
               WHEN i.Status IN (N'Saiu para Entrega', N'Entregue') THEN i.FuncionarioEntregaId
               WHEN i.Status = N'Aguardando Coleta' THEN i.FuncionarioPreparaId
               ELSE NULL
           END
    FROM inserted i
    LEFT JOIN deleted d ON d.Id = i.Id
    WHERE d.Id IS NULL OR d.Status <> i.Status;
END
GO

/* ==========================================================================
   6. PROCEDURES DE RELATORIO (parametrizadas)
   ========================================================================== */

-- Faturamento dos 12 meses de um ano (meses sem venda aparecem zerados)
CREATE OR ALTER PROCEDURE ObterFaturamentoMensal
    @Ano INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @Ano IS NULL SET @Ano = YEAR(SYSDATETIME());

    SELECT m.Mes,
           COUNT(p.Id) AS QuantidadePedidos,
           CAST(ISNULL(SUM(p.Total), 0) AS DECIMAL(10,2)) AS Faturamento,
           CAST(ISNULL(AVG(p.Total), 0) AS DECIMAL(10,2)) AS TicketMedio
    FROM (VALUES (1),(2),(3),(4),(5),(6),(7),(8),(9),(10),(11),(12)) AS m(Mes)
    LEFT JOIN Pedido p
           ON p.DataHora >= DATEFROMPARTS(@Ano, m.Mes, 1)
          AND p.DataHora <  DATEADD(MONTH, 1, DATEFROMPARTS(@Ano, m.Mes, 1))
    GROUP BY m.Mes
    ORDER BY m.Mes;
END
GO

-- Faturamento diario dos ultimos N dias (dias sem venda aparecem zerados)
CREATE OR ALTER PROCEDURE ObterFaturamentoDiario
    @Dias INT = 30
AS
BEGIN
    SET NOCOUNT ON;

    IF @Dias < 1 SET @Dias = 1;
    IF @Dias > 366 SET @Dias = 366;

    DECLARE @Hoje DATE = CAST(SYSDATETIME() AS DATE);
    DECLARE @Inicio DATE = DATEADD(DAY, -(@Dias - 1), @Hoje);

    WITH Dias AS (
        SELECT @Inicio AS Dia
        UNION ALL
        SELECT DATEADD(DAY, 1, Dia) FROM Dias WHERE Dia < @Hoje
    )
    SELECT d.Dia,
           COUNT(p.Id) AS QuantidadePedidos,
           CAST(ISNULL(SUM(p.Total), 0) AS DECIMAL(10,2)) AS Faturamento
    FROM Dias d
    LEFT JOIN Pedido p
           ON p.DataHora >= d.Dia
          AND p.DataHora <  DATEADD(DAY, 1, d.Dia)
    GROUP BY d.Dia
    ORDER BY d.Dia
    OPTION (MAXRECURSION 400);
END
GO

-- Ranking das pizzas mais pedidas (periodo opcional)
CREATE OR ALTER PROCEDURE ObterRankingPizzas
    @DataInicio DATE = NULL,
    @DataFim    DATE = NULL,
    @Top        INT  = 10
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (@Top)
           p.Id AS PizzaId,
           p.Nome AS Pizza,
           SUM(ip.Quantidade) AS QuantidadeVendida,
           CAST(SUM(ip.Quantidade * ip.PrecoUnitario) AS DECIMAL(10,2)) AS Faturamento
    FROM ItemPedido ip
    JOIN Pedido pe ON pe.Id = ip.PedidoId
    JOIN Pizza p ON p.Id = ip.PizzaId
    WHERE (@DataInicio IS NULL OR pe.DataHora >= @DataInicio)
      AND (@DataFim IS NULL OR pe.DataHora < DATEADD(DAY, 1, @DataFim))
    GROUP BY p.Id, p.Nome
    ORDER BY QuantidadeVendida DESC, Faturamento DESC;
END
GO

-- Vendas por tamanho (periodo opcional)
CREATE OR ALTER PROCEDURE ObterVendasPorTamanho
    @DataInicio DATE = NULL,
    @DataFim    DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT ip.Tamanho,
           SUM(ip.Quantidade) AS QuantidadeVendida,
           CAST(SUM(ip.Quantidade * ip.PrecoUnitario) AS DECIMAL(10,2)) AS Faturamento
    FROM ItemPedido ip
    JOIN Pedido pe ON pe.Id = ip.PedidoId
    WHERE (@DataInicio IS NULL OR pe.DataHora >= @DataInicio)
      AND (@DataFim IS NULL OR pe.DataHora < DATEADD(DAY, 1, @DataFim))
    GROUP BY ip.Tamanho
    ORDER BY QuantidadeVendida DESC;
END
GO

-- Quantidade de pedidos por status (todos os status aparecem, mesmo com zero)
CREATE OR ALTER PROCEDURE ObterPedidosPorStatus
    @DataInicio DATE = NULL,
    @DataFim    DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT s.Status,
           COUNT(p.Id) AS Quantidade
    FROM (VALUES (1, N'Em Preparação'),
                 (2, N'Aguardando Coleta'),
                 (3, N'Saiu para Entrega'),
                 (4, N'Entregue')) AS s(Ordem, Status)
    LEFT JOIN Pedido p
           ON p.Status = s.Status
          AND (@DataInicio IS NULL OR p.DataHora >= @DataInicio)
          AND (@DataFim IS NULL OR p.DataHora < DATEADD(DAY, 1, @DataFim))
    GROUP BY s.Ordem, s.Status
    ORDER BY s.Ordem;
END
GO

-- Pedidos por hora do dia nos ultimos N dias (horarios de pico)
CREATE OR ALTER PROCEDURE ObterHorariosPico
    @Dias INT = 30
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Desde DATETIME2 = DATEADD(DAY, -@Dias, SYSDATETIME());

    WITH Horas AS (
        SELECT 0 AS Hora
        UNION ALL
        SELECT Hora + 1 FROM Horas WHERE Hora < 23
    )
    SELECT h.Hora,
           COUNT(p.Id) AS QuantidadePedidos
    FROM Horas h
    LEFT JOIN Pedido p
           ON DATEPART(HOUR, p.DataHora) = h.Hora
          AND p.DataHora >= @Desde
    GROUP BY h.Hora
    ORDER BY h.Hora;
END
GO

-- Indicadores (cartoes) do dashboard: uma linha
CREATE OR ALTER PROCEDURE ObterResumoDashboard
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Hoje DATE = CAST(SYSDATETIME() AS DATE);
    DECLARE @InicioMes DATE = DATEFROMPARTS(YEAR(@Hoje), MONTH(@Hoje), 1);

    SELECT
        (SELECT COUNT(*) FROM Pedido WHERE DataHora >= @Hoje) AS PedidosHoje,
        CAST((SELECT ISNULL(SUM(Total), 0) FROM Pedido WHERE DataHora >= @Hoje) AS DECIMAL(10,2)) AS FaturamentoHoje,
        (SELECT COUNT(*) FROM Pedido WHERE DataHora >= @InicioMes) AS PedidosMes,
        CAST((SELECT ISNULL(SUM(Total), 0) FROM Pedido WHERE DataHora >= @InicioMes) AS DECIMAL(10,2)) AS FaturamentoMes,
        CAST((SELECT ISNULL(AVG(Total), 0) FROM Pedido WHERE DataHora >= @InicioMes) AS DECIMAL(10,2)) AS TicketMedioMes,
        (SELECT COUNT(*) FROM Pedido WHERE Status <> N'Entregue') AS PedidosEmAberto,
        (SELECT CAST(AVG(CAST(DATEDIFF(MINUTE, p.DataHora, h.AlteradoEm) AS DECIMAL(10,2))) AS DECIMAL(10,1))
           FROM Pedido p
           JOIN PedidoStatusHistorico h ON h.PedidoId = p.Id AND h.Status = N'Entregue'
          WHERE p.DataHora >= DATEADD(DAY, -30, @Hoje)) AS TempoMedioEntregaMin;
END
GO

/* ==========================================================================
   7. PROCEDURE DE DADOS DE DEMONSTRACAO
      Gera meses de pedidos para os graficos do dashboard.
      - Reexecutavel: apaga antes os pedidos DEMO-* e os clientes @demo.lavai.
      - @Meses = 0 apenas limpa os dados de demonstracao.
      - Roda em transacao: se algo falhar, nada e gravado (ROLLBACK).
   ========================================================================== */

CREATE OR ALTER PROCEDURE GerarDadosDemonstracao
    @Meses INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @Meses < 0 SET @Meses = 0;
    IF @Meses > 12 SET @Meses = 12;

    BEGIN TRY
        BEGIN TRAN;

        -- 1) Limpa a demonstracao anterior (ordem respeita as chaves estrangeiras)
        DELETE FROM PedidoStatusHistorico
         WHERE PedidoId IN (SELECT Id FROM Pedido WHERE CodigoAcompanhamento LIKE N'DEMO-%');
        DELETE FROM ItemPedido
         WHERE PedidoId IN (SELECT Id FROM Pedido WHERE CodigoAcompanhamento LIKE N'DEMO-%');
        DELETE FROM Pedido WHERE CodigoAcompanhamento LIKE N'DEMO-%';
        DELETE FROM Cliente WHERE Email LIKE N'%@demo.lavai';

        IF @Meses > 0
        BEGIN
            DECLARE @Hoje DATE = CAST(SYSDATETIME() AS DATE);
            DECLARE @Inicio DATE = DATEADD(DAY, -(@Meses * 30), @Hoje);

            -- 2) Clientes de demonstracao
            INSERT INTO Cliente (Nome, Telefone, Email, Logradouro, Numero, Bairro, Cidade, Cep)
            VALUES
            (N'João Silva',      N'(11) 98888-7777', N'joao@demo.lavai',     N'Rua das Flores',     N'123',  N'Vila Mariana',  N'São Paulo', N'04101-000'),
            (N'Maria Oliveira',  N'(11) 97777-6666', N'maria@demo.lavai',    N'Avenida Paulista',   N'1500', N'Bela Vista',    N'São Paulo', N'01310-100'),
            (N'Carlos Souza',    N'(11) 96666-5555', N'carlos@demo.lavai',   N'Rua Amazonas',       N'45',   N'Mooca',         N'São Paulo', N'03102-000'),
            (N'Ana Costa',       N'(11) 95555-4444', N'ana@demo.lavai',      N'Rua Sergipe',        N'890',  N'Higienópolis',  N'São Paulo', N'01243-001'),
            (N'Pedro Santos',    N'(11) 94444-3333', N'pedro@demo.lavai',    N'Rua da Paz',         N'10',   N'Santana',       N'São Paulo', N'02012-000'),
            (N'Beatriz Lima',    N'(11) 93333-2222', N'beatriz@demo.lavai',  N'Rua Augusta',        N'700',  N'Consolação',    N'São Paulo', N'01304-001'),
            (N'Lucas Pereira',   N'(11) 92222-1111', N'lucas@demo.lavai',    N'Rua Oscar Freire',   N'320',  N'Jardins',       N'São Paulo', N'01426-001'),
            (N'Fernanda Rocha',  N'(11) 91111-0000', N'fernanda@demo.lavai', N'Rua Vergueiro',      N'2100', N'Paraíso',       N'São Paulo', N'04102-000'),
            (N'Rafael Alves',    N'(11) 98765-4321', N'rafael@demo.lavai',   N'Rua Tabapuã',        N'55',   N'Itaim Bibi',    N'São Paulo', N'04533-010'),
            (N'Camila Ribeiro',  N'(11) 98123-4567', N'camila@demo.lavai',   N'Rua Pamplona',       N'900',  N'Jardim Paulista', N'São Paulo', N'01405-001'),
            (N'Diego Martins',   N'(11) 97654-3210', N'diego@demo.lavai',    N'Rua Vergueiro',      N'3300', N'Vila Mariana',  N'São Paulo', N'04102-001'),
            (N'Juliana Gomes',   N'(11) 96543-2109', N'juliana@demo.lavai',  N'Avenida Brasil',     N'410',  N'Jardim América', N'São Paulo', N'01430-000'),
            (N'Thiago Barbosa',  N'(11) 95432-1098', N'thiago@demo.lavai',   N'Rua Haddock Lobo',   N'250',  N'Cerqueira César', N'São Paulo', N'01414-000'),
            (N'Patrícia Nunes',  N'(11) 94321-0987', N'patricia@demo.lavai', N'Rua Teodoro Sampaio', N'1020', N'Pinheiros',     N'São Paulo', N'05406-050'),
            (N'Eduardo Castro',  N'(11) 93210-9876', N'eduardo@demo.lavai',  N'Rua Cardeal Arcoverde', N'800', N'Pinheiros',    N'São Paulo', N'05407-001'),
            (N'Larissa Freitas', N'(11) 92109-8765', N'larissa@demo.lavai',  N'Rua Joaquim Floriano', N'66', N'Itaim Bibi',     N'São Paulo', N'04534-000'),
            (N'Bruno Teixeira',  N'(11) 91098-7654', N'bruno@demo.lavai',    N'Avenida Ipiranga',   N'200',  N'República',     N'São Paulo', N'01046-010'),
            (N'Amanda Cardoso',  N'(11) 99987-6543', N'amanda@demo.lavai',   N'Rua Domingos de Morais', N'1500', N'Vila Mariana', N'São Paulo', N'04010-100'),
            (N'Gustavo Pinto',   N'(11) 98876-5432', N'gustavo@demo.lavai',  N'Rua Apinajés',       N'1100', N'Sumaré',        N'São Paulo', N'05017-000'),
            (N'Renata Dias',     N'(11) 97765-4321', N'renata@demo.lavai',   N'Rua Cotoxó',         N'450',  N'Perdizes',      N'São Paulo', N'05021-000');

            -- 3) Tabelas de apoio (temporarias; somem ao fim da procedure)
            -- Pizzas ativas: as primeiras da lista ficam um pouco mais populares
            SELECT ROW_NUMBER() OVER (ORDER BY Id) - 1 AS Idx, Id
              INTO #PizzasAtivas FROM Pizza WHERE Ativa = 1;

            SELECT ROW_NUMBER() OVER (ORDER BY Id) - 1 AS Idx, Id
              INTO #Pizzaiolos FROM Funcionario WHERE Cargo = N'Pizzaiolo';
            SELECT ROW_NUMBER() OVER (ORDER BY Id) - 1 AS Idx, Id
              INTO #Entregadores FROM Funcionario WHERE Cargo = N'Entregador';
            SELECT ROW_NUMBER() OVER (ORDER BY Id) - 1 AS Idx, Id
              INTO #ClientesDemo FROM Cliente WHERE Email LIKE N'%@demo.lavai';

            DECLARE @NumPizzas INT = (SELECT COUNT(*) FROM #PizzasAtivas);
            DECLARE @NumPizzaiolos INT = (SELECT COUNT(*) FROM #Pizzaiolos);
            DECLARE @NumEntregadores INT = (SELECT COUNT(*) FROM #Entregadores);
            DECLARE @NumClientes INT = (SELECT COUNT(*) FROM #ClientesDemo);

            IF @NumPizzas = 0 OR @NumPizzaiolos = 0 OR @NumEntregadores = 0
                THROW 50002, N'Cadastre pizzas ativas, pizzaiolos e entregadores antes de gerar a demonstração.', 1;

            WITH N AS (
                SELECT 1 AS n
                UNION ALL
                SELECT n + 1 FROM N WHERE n < 60
            )
            SELECT n INTO #Numeros FROM N;

            -- Quantos pedidos por dia: mais no fim de semana e crescendo aos poucos
            CREATE TABLE #Dias (Dia DATE NOT NULL PRIMARY KEY, Qtd INT NOT NULL);

            WITH Dias AS (
                SELECT @Inicio AS Dia
                UNION ALL
                SELECT DATEADD(DAY, 1, Dia) FROM Dias WHERE Dia < @Hoje
            )
            INSERT INTO #Dias (Dia, Qtd)
            SELECT Dia,
                   4 + ABS(CHECKSUM(NEWID()) % 1000003) % 6
                     + CASE WHEN DATEDIFF(DAY, 0, Dia) % 7 >= 4 THEN 4 ELSE 0 END
                     + DATEDIFF(DAY, @Inicio, Dia) / 40
              FROM Dias
            OPTION (MAXRECURSION 400);

            -- 4) Pedidos: horario no almoco (25%) ou na janta (75%)
            -- Cada coluna R* guarda um sorteio independente (0 a 1.000.002) por pedido.
            -- O sorteio e gravado na tabela para ser lido varias vezes com o mesmo valor.
            CREATE TABLE #Pedidos (
                Seq        INT IDENTITY(1,1) PRIMARY KEY,
                Codigo     NVARCHAR(12) NULL,
                DataHora   DATETIME2    NOT NULL,
                StatusRank INT          NULL,
                M1         INT          NULL,
                M2         INT          NULL,
                M3         INT          NULL,
                RCliente   INT          NOT NULL,
                RPrep      INT          NOT NULL,
                REntr      INT          NOT NULL,
                RItens     INT          NOT NULL,
                RTempo     INT          NOT NULL,
                RM1        INT          NOT NULL,
                RM2        INT          NOT NULL,
                RM3        INT          NOT NULL
            );

            INSERT INTO #Pedidos (DataHora, RCliente, RPrep, REntr, RItens, RTempo, RM1, RM2, RM3)
            SELECT DATEADD(MINUTE,
                           CASE WHEN ABS(CHECKSUM(NEWID()) % 1000003) % 100 < 25
                                THEN 11 * 60 + ABS(CHECKSUM(NEWID()) % 1000003) % 180
                                ELSE 18 * 60 + ABS(CHECKSUM(NEWID()) % 1000003) % 240 END,
                           CAST(d.Dia AS DATETIME2)),
                   ABS(CHECKSUM(NEWID()) % 1000003), ABS(CHECKSUM(NEWID()) % 1000003),
                   ABS(CHECKSUM(NEWID()) % 1000003), ABS(CHECKSUM(NEWID()) % 1000003),
                   ABS(CHECKSUM(NEWID()) % 1000003), ABS(CHECKSUM(NEWID()) % 1000003),
                   ABS(CHECKSUM(NEWID()) % 1000003), ABS(CHECKSUM(NEWID()) % 1000003)
              FROM #Dias d
              JOIN #Numeros n ON n.n <= d.Qtd
             ORDER BY d.Dia, n.n;

            -- pedidos de hoje nao podem estar no futuro: ficam nas ultimas 2 horas
            UPDATE #Pedidos
               SET DataHora = DATEADD(MINUTE, -(1 + ABS(CHECKSUM(NEWID()) % 1000003) % 120), SYSDATETIME())
             WHERE DataHora > SYSDATETIME();

            -- Status conforme a idade do pedido e minutos de cada etapa
            -- (etapa 2 ate 14 min, etapa 3 ate 23 min, etapa 4 ate 54 min: sempre antes do proximo limite)
            UPDATE #Pedidos
               SET Codigo = N'DEMO-' + RIGHT(N'0000000' + CAST(Seq AS NVARCHAR(7)), 7),
                   StatusRank = CASE
                                    WHEN DATEDIFF(MINUTE, DataHora, SYSDATETIME()) < 15 THEN 1
                                    WHEN DATEDIFF(MINUTE, DataHora, SYSDATETIME()) < 30 THEN 2
                                    WHEN DATEDIFF(MINUTE, DataHora, SYSDATETIME()) < 60 THEN 3
                                    ELSE 4
                                END,
                   M1 = 5 + RM1 % 10,
                   M2 = 5 + RM1 % 10 + 4 + RM2 % 6,
                   M3 = 5 + RM1 % 10 + 4 + RM2 % 6 + 12 + RM3 % 20;

            -- 5) Grava os pedidos (valores calculados depois, a partir dos itens).
            -- O trigger de historico e desligado: o historico demo e gravado abaixo
            -- com os horarios historicos corretos.
            DISABLE TRIGGER trg_HistoricoStatusPedido ON Pedido;

            INSERT INTO Pedido (CodigoAcompanhamento, Subtotal, Desconto, TaxaEntrega, Total, Status, DataHora,
                                TempoEstimado, ClienteId, FuncionarioPreparaId, FuncionarioEntregaId)
            SELECT p.Codigo, 0, 0, 0, 0,
                   CASE p.StatusRank
                        WHEN 1 THEN N'Em Preparação'
                        WHEN 2 THEN N'Aguardando Coleta'
                        WHEN 3 THEN N'Saiu para Entrega'
                        ELSE N'Entregue'
                   END,
                   p.DataHora,
                   30 + p.RTempo % 25,
                   c.Id,
                   CASE WHEN p.StatusRank >= 2 THEN pz.Id END,
                   CASE WHEN p.StatusRank >= 3 THEN en.Id END
              FROM #Pedidos p
              JOIN #ClientesDemo c ON c.Idx = p.RCliente % @NumClientes
              JOIN #Pizzaiolos pz ON pz.Idx = p.RPrep % @NumPizzaiolos
              JOIN #Entregadores en ON en.Idx = p.REntr % @NumEntregadores
             ORDER BY p.Seq;

            -- 6) Itens: 1 a 3 pizzas por pedido (pizza/tamanho repetidos somam a quantidade).
            -- A pizza e sorteada com leve preferencia pelas primeiras da lista
            -- (diferenca entre dois sorteios); o tamanho: 35% Grande, 45% Media, 20% Pequena.
            CREATE TABLE #Slots (
                Seq  INT NOT NULL,
                Slot INT NOT NULL,
                RP1  INT NOT NULL,
                RP2  INT NOT NULL,
                RTam INT NOT NULL,
                RQtd INT NOT NULL
            );

            INSERT INTO #Slots (Seq, Slot, RP1, RP2, RTam, RQtd)
            SELECT p.Seq, s.n,
                   ABS(CHECKSUM(NEWID()) % 1000003), ABS(CHECKSUM(NEWID()) % 1000003),
                   ABS(CHECKSUM(NEWID()) % 1000003), ABS(CHECKSUM(NEWID()) % 1000003)
              FROM #Pedidos p
              JOIN #Numeros s ON s.n <= 1 + p.RItens % 3;

            WITH Escolhas AS (
                SELECT sl.Seq,
                       ABS((sl.RP1 % @NumPizzas) - (sl.RP2 % @NumPizzas)) AS PizzaIdx,
                       CASE WHEN sl.RTam % 100 < 35 THEN N'Grande'
                            WHEN sl.RTam % 100 < 80 THEN N'Media'
                            ELSE N'Pequena' END AS Tamanho,
                       CASE WHEN sl.RQtd % 5 = 0 THEN 2 ELSE 1 END AS Qtd
                  FROM #Slots sl
            )
            INSERT INTO ItemPedido (PedidoId, PizzaId, Tamanho, Quantidade, PrecoUnitario)
            SELECT pe.Id, pa.Id, e.Tamanho, SUM(e.Qtd), pp.Preco
              FROM Escolhas e
              JOIN #Pedidos p ON p.Seq = e.Seq
              JOIN Pedido pe ON pe.CodigoAcompanhamento = p.Codigo
              JOIN #PizzasAtivas pa ON pa.Idx = e.PizzaIdx
              JOIN PizzaPreco pp ON pp.PizzaId = pa.Id AND pp.Tamanho = e.Tamanho
             GROUP BY pe.Id, pa.Id, e.Tamanho, pp.Preco;

            -- seguranca: descarta pedido que ficou sem item (pizza sem preco no tamanho sorteado)
            DELETE FROM Pedido
             WHERE CodigoAcompanhamento LIKE N'DEMO-%'
               AND NOT EXISTS (SELECT 1 FROM ItemPedido i WHERE i.PedidoId = Pedido.Id);

            -- 7) Totais: desconto de 10% (promocao do site) e frete gratis acima de R$ 50
            UPDATE pe
               SET Subtotal    = t.Sub,
                   Desconto    = dbo.CalcularDesconto(t.Sub, N'PERCENTUAL', 10),
                   TaxaEntrega = CASE WHEN t.Sub >= 50 THEN 0 ELSE 8.00 END,
                   Total       = t.Sub - dbo.CalcularDesconto(t.Sub, N'PERCENTUAL', 10)
                                 + CASE WHEN t.Sub >= 50 THEN 0 ELSE 8.00 END
              FROM Pedido pe
              JOIN (SELECT PedidoId, SUM(Quantidade * PrecoUnitario) AS Sub
                      FROM ItemPedido
                     GROUP BY PedidoId) t ON t.PedidoId = pe.Id
             WHERE pe.CodigoAcompanhamento LIKE N'DEMO-%';

            -- 8) Historico de status (linha do tempo) com os horarios de cada etapa
            INSERT INTO PedidoStatusHistorico (PedidoId, Status, AlteradoEm, FuncionarioId)
            SELECT pe.Id, N'Em Preparação', p.DataHora, NULL
              FROM #Pedidos p JOIN Pedido pe ON pe.CodigoAcompanhamento = p.Codigo
            UNION ALL
            SELECT pe.Id, N'Aguardando Coleta', DATEADD(MINUTE, p.M1, p.DataHora), pe.FuncionarioPreparaId
              FROM #Pedidos p JOIN Pedido pe ON pe.CodigoAcompanhamento = p.Codigo
             WHERE p.StatusRank >= 2
            UNION ALL
            SELECT pe.Id, N'Saiu para Entrega', DATEADD(MINUTE, p.M2, p.DataHora), pe.FuncionarioEntregaId
              FROM #Pedidos p JOIN Pedido pe ON pe.CodigoAcompanhamento = p.Codigo
             WHERE p.StatusRank >= 3
            UNION ALL
            SELECT pe.Id, N'Entregue', DATEADD(MINUTE, p.M3, p.DataHora), pe.FuncionarioEntregaId
              FROM #Pedidos p JOIN Pedido pe ON pe.CodigoAcompanhamento = p.Codigo
             WHERE p.StatusRank >= 4;

            ENABLE TRIGGER trg_HistoricoStatusPedido ON Pedido;
        END

        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;
        THROW;
    END CATCH

    SELECT (SELECT COUNT(*) FROM Pedido WHERE CodigoAcompanhamento LIKE N'DEMO-%') AS PedidosDemonstracao,
           (SELECT COUNT(*) FROM Cliente WHERE Email LIKE N'%@demo.lavai') AS ClientesDemonstracao;
END
GO

/* ==========================================================================
   8. DADOS INICIAIS (seed)
   ========================================================================== */

-- Logins dos funcionarios (hashes gerados pelo PasswordHasher do ASP.NET Core)
INSERT INTO Login (Email, SenhaHash, Nome) VALUES
(N'admin@lavai.com',      N'AQAAAAIAAYagAAAAEBfaHE4+zBRjvVJDlzOGF3ngrZ8UJXgEauDqUetEmZQsOgmJKLkdoYhGm8Y+qEww3A==', N'Administrador'),
(N'pizzaiolo1@lavai.com', N'AQAAAAIAAYagAAAAEBRKGTS6POTkhpuVNj0UYl0dudyXxv36ykDe4KTHhy2bZLxjHCIMwC+hoO+gw8018g==', N'Roberto Massa'),
(N'pizzaiolo2@lavai.com', N'AQAAAAIAAYagAAAAEP2T7llTtWfYH11HqbgdC9CYznFpDLdz2YlSy79mpEXEwFystKmdyWW4iAh8wJ8+qw==', N'Marcos Forno'),
(N'motoboy1@lavai.com',   N'AQAAAAIAAYagAAAAECi1lv5L6Hr+ShAAiZjMl83Fcmlhtq4S7oky5ioKM6jFIMet1EtiSpuZKJEVTaWzzg==', N'Ricardo Veloz'),
(N'motoboy2@lavai.com',   N'AQAAAAIAAYagAAAAELDsB21H/+xLFnxqtNC5EVvy2xamFgD6JFgbjF/TgkK254/cC1e4CiadRla2IXq+1Q==', N'Lucas Rápido');
GO

INSERT INTO Funcionario (Nome, Cargo, Telefone, LoginId)
SELECT v.Nome, v.Cargo, v.Telefone, l.Id
  FROM (VALUES
        (N'Administrador',  N'Gerente',    N'(11) 95555-6666', N'admin@lavai.com'),
        (N'Roberto Massa',  N'Pizzaiolo',  N'(11) 91111-2222', N'pizzaiolo1@lavai.com'),
        (N'Marcos Forno',   N'Pizzaiolo',  N'(11) 92222-3333', N'pizzaiolo2@lavai.com'),
        (N'Ricardo Veloz',  N'Entregador', N'(11) 93333-4444', N'motoboy1@lavai.com'),
        (N'Lucas Rápido',   N'Entregador', N'(11) 94444-5555', N'motoboy2@lavai.com')
       ) AS v(Nome, Cargo, Telefone, Email)
  JOIN Login l ON l.Email = v.Email;
GO

-- Cardapio (mesmas 6 pizzas do site). ImagemUrl relativa ao front; uploads
-- feitos pelo admin ficam em /uploads/... (servidos pela API).
INSERT INTO Pizza (Nome, Descricao, ImagemUrl) VALUES
(N'Calabresa Especial',    N'Calabresa premium defumada, cebola roxa e um toque de orégano fresco sobre massa artesanal.',  N'assets/images/pizza.png'),
(N'Margherita Premium',    N'Muçarela de búfala, manjericão orgânico e fatias de tomate italiano selecionados.',            N'assets/images/pizza2.png'),
(N'Frango com Catupiry',   N'Frango desfiado temperado com ervas finas e o legítimo Catupiry original.',                    N'assets/images/pizza3.png'),
(N'Ouriço Quatro Queijos', N'A combinação perfeita de Muçarela, Provolone, Parmesão e Gorgonzola.',                         N'assets/images/pizza4.png'),
(N'Portuguesa LaVai',      N'Presunto, ovos, cebola, ervilha e azeitonas pretas sobre molho de tomate pelati.',             N'assets/images/pizza5.png'),
(N'Chocolate com Avelã',   N'Chocolate meio amargo derretido com creme de avelã e pedaços de castanha.',                    N'assets/images/pizza7.png');
GO

INSERT INTO PizzaPreco (PizzaId, Tamanho, Preco)
SELECT pz.Id, v.Tamanho, v.Preco
  FROM (VALUES
        (N'Calabresa Especial',    N'Pequena', 20.00), (N'Calabresa Especial',    N'Media', 35.00), (N'Calabresa Especial',    N'Grande', 45.90),
        (N'Margherita Premium',    N'Pequena', 25.00), (N'Margherita Premium',    N'Media', 40.00), (N'Margherita Premium',    N'Grande', 49.90),
        (N'Frango com Catupiry',   N'Pequena', 22.00), (N'Frango com Catupiry',   N'Media', 38.00), (N'Frango com Catupiry',   N'Grande', 52.90),
        (N'Ouriço Quatro Queijos', N'Pequena', 28.00), (N'Ouriço Quatro Queijos', N'Media', 45.00), (N'Ouriço Quatro Queijos', N'Grande', 55.00),
        (N'Portuguesa LaVai',      N'Pequena', 24.00), (N'Portuguesa LaVai',      N'Media', 39.00), (N'Portuguesa LaVai',      N'Grande', 50.00),
        (N'Chocolate com Avelã',   N'Pequena', 30.00), (N'Chocolate com Avelã',   N'Media', 45.00), (N'Chocolate com Avelã',   N'Grande', 58.00)
       ) AS v(Pizza, Tamanho, Preco)
  JOIN Pizza pz ON pz.Nome = v.Pizza;
GO

-- Promocoes (vigentes por 1 ano a partir de hoje)
INSERT INTO Promocao (Nome, Tipo, Valor, ValorMinimo, Inicio, Fim, Ativa, PizzaId)
VALUES
(N'10% de desconto no pedido',            N'PERCENTUAL',  10, NULL, SYSDATETIME(), DATEADD(YEAR, 1, SYSDATETIME()), 1, NULL),
(N'Frete grátis acima de R$ 50',          N'FRETE_GRATIS', 0, 50.00, SYSDATETIME(), DATEADD(YEAR, 1, SYSDATETIME()), 1, NULL),
(N'Quarta da Calabresa: 20% de desconto', N'PERCENTUAL',  20, NULL, SYSDATETIME(), DATEADD(YEAR, 1, SYSDATETIME()), 1,
    (SELECT Id FROM Pizza WHERE Nome = N'Calabresa Especial'));
GO

-- Dados de demonstracao para os graficos (remova esta linha para iniciar vazio)
EXEC GerarDadosDemonstracao @Meses = 10;
GO

/* ==========================================================================
   9. CONFERENCIA
   ========================================================================== */
SELECT N'Login' AS Tabela, COUNT(*) AS Registros FROM Login
UNION ALL SELECT N'Funcionario', COUNT(*) FROM Funcionario
UNION ALL SELECT N'Cliente', COUNT(*) FROM Cliente
UNION ALL SELECT N'Pizza', COUNT(*) FROM Pizza
UNION ALL SELECT N'PizzaPreco', COUNT(*) FROM PizzaPreco
UNION ALL SELECT N'Promocao', COUNT(*) FROM Promocao
UNION ALL SELECT N'Pedido', COUNT(*) FROM Pedido
UNION ALL SELECT N'ItemPedido', COUNT(*) FROM ItemPedido
UNION ALL SELECT N'PedidoStatusHistorico', COUNT(*) FROM PedidoStatusHistorico
UNION ALL SELECT N'HistoricoPreco', COUNT(*) FROM HistoricoPreco;
GO
